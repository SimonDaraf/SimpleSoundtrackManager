using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class LoopStream : IWaveProvider, IDisposable
    {
        private WaveStream sourceStream;

        public long StartPosition { get; set; } = 0;
        public long LoopPosition { get; set; }

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

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset, count);
                if (bytesRead == 0 || sourceStream.Position > sourceStream.Length)
                {
                    // Faulty source stream.
                    if (sourceStream.Position == 0)
                    {
                        break;
                    }

                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        public void Dispose()
        {
            sourceStream.Dispose();
        }
    }
}
