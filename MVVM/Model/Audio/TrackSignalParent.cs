using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class TrackSignalParent : ISampleProvider, IDisposable
    {
        public event EventHandler<long>? PositionUpdated;

        public float Volume { get; set; }
        private byte[] readBuffer;

        private LoopStream loopStream;
        private LoopStream? toLoad;
        private bool isTransitioning;
        private long toLoadPosition;

        public TrackSignalParent(LoopStream loopStream)
        {
            this.loopStream = loopStream;
            readBuffer = [];
        }

        public WaveFormat WaveFormat => loopStream.WaveFormat;

        public void ForcePositionChange(long pos)
        {
            loopStream.Position = pos;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int bytesPerSample = loopStream.WaveFormat.BitsPerSample / 8;
            if (readBuffer == null || readBuffer.Length < count * bytesPerSample)
                readBuffer = new byte[count * bytesPerSample];

            int bytesRead = loopStream.Read(readBuffer, 0, count * bytesPerSample);
            int samplesRead = bytesRead / bytesPerSample;

            if (toLoad is null && isTransitioning)
            {

            }

            long bufferStartPosition = loopStream.Position - bytesRead;
            long fadeInEndPosition = loopStream.StartPosition + loopStream.TransitionLength;
            long fadeOutStartPosition = loopStream.LoopPosition - loopStream.TransitionLength;
            for (int i = 0; i < samplesRead; i++)
            {
                float fadeVolume = 1f;

                long samplePosition = bufferStartPosition + (i * bytesPerSample);
                if (samplePosition < fadeInEndPosition)
                {
                    float t = (float)(samplePosition - loopStream.StartPosition) / loopStream.TransitionLength;
                    fadeVolume = MathF.Max(0, MathF.Min(1, t));
                }
                else if (samplePosition > fadeOutStartPosition)
                {
                    float t = 1 - (float)(samplePosition - fadeOutStartPosition) / loopStream.TransitionLength;
                    fadeVolume = MathF.Max(0, MathF.Min(1, t));
                }
                buffer[offset + i] = BitConverter.ToSingle(readBuffer, i * bytesPerSample) * Volume * fadeVolume;
            }

            PositionUpdated?.Invoke(this, loopStream.Position);
            return samplesRead;
        }

        public void Dispose()
        {
            loopStream.Dispose();
        }
    }
}
