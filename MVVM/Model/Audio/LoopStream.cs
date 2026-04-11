using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class LoopStream : IWaveProvider, IDisposable
    {
        private WaveStream sourceStream;

        public long TransitionLength { get; set; }
        public long StartPosition { get; set; } = 0;
        public long LoopPosition { get; set; }
        private bool isFirstRead = false;

        public LoopStream(WaveStream stream)
        {
            sourceStream = stream;
            LoopPosition = stream.Length;
        }

        public long Length
        {
            get { return sourceStream.Length; }
        }

        public long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            // https://markheath.net/post/looped-playback-in-net-with-naudio
            int totalBytesRead = 0;

            if (!isFirstRead)
            {
                sourceStream.Position = StartPosition;
                isFirstRead = true;
            }
                

            while (totalBytesRead < count)
            {
                int bytesToRead = (int)Math.Min(count - totalBytesRead, LoopPosition - sourceStream.Position);
                if (bytesToRead <= 0)
                {
                    sourceStream.Position = StartPosition;
                    bytesToRead = (int)Math.Min(count - totalBytesRead, LoopPosition - sourceStream.Position);
                }

                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, bytesToRead);
                if (bytesRead == 0)
                {
                    if (bytesToRead > 0)
                    {
                        sourceStream.Position = StartPosition;
                    }
                    else
                    {
                        break;
                    }
                }
                    

                totalBytesRead += bytesRead;

                if (sourceStream.Position >= LoopPosition)
                    sourceStream.Position = StartPosition;
            }

            return totalBytesRead;
        }

        public void Dispose()
        {
            sourceStream.Dispose();
        }
    }
}
