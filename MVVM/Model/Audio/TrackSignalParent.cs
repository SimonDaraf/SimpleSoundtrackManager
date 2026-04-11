using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class TrackSignalParent : ISampleProvider, IDisposable
    {
        public event EventHandler<long>? PositionUpdated;

        public float Volume { get; set; }
        private byte[] readBuffer;
        private byte[] toLoadBuffer;

        private LoopStream loopStream;
        private LoopStream? toLoad;
        private bool isTransitioning;

        public TrackSignalParent(LoopStream loopStream)
        {
            this.loopStream = loopStream;
            readBuffer = [];
            toLoadBuffer = [];
        }

        public WaveFormat WaveFormat => loopStream.WaveFormat;

        public void ForcePositionChange(long pos)
        {
            loopStream.Position = pos;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int bytesPerSample = loopStream.WaveFormat.BitsPerSample / 8;
            int toLoadBytePerSamples = toLoad is not null ? toLoad.WaveFormat.BitsPerSample / 8 : bytesPerSample;

            if (readBuffer == null || readBuffer.Length < count * bytesPerSample)
                readBuffer = new byte[count * bytesPerSample];

            int bytesRead = loopStream.Read(readBuffer, 0, count * bytesPerSample);
            int samplesRead = bytesRead / bytesPerSample;

            long fadeOutStartPosition = loopStream.LoopPosition - loopStream.TransitionLength;
            if (isTransitioning)
            {
                if (toLoad is not null)
                {
                }
            }

            long bufferStartPosition = loopStream.Position - bytesRead;
            long fadeInEndPosition = loopStream.StartPosition + loopStream.TransitionLength;
            
            for (int i = 0; i < samplesRead; i++)
            {
                float fadeVolume = 1f;
                float transitionSample = isTransitioning && toLoadBuffer.Length > 0 ?
                    BitConverter.ToSingle(toLoadBuffer, i * toLoadBytePerSamples) : 0f;

                long samplePosition = bufferStartPosition + (i * bytesPerSample);
                if (samplePosition < fadeInEndPosition)
                {
                    float t = (float)(samplePosition - loopStream.StartPosition) / loopStream.TransitionLength;
                    fadeVolume = MathF.Max(0, MathF.Min(1, t));
                }
                else if (samplePosition > fadeOutStartPosition)
                {
                    isTransitioning = true;
                    float t = 1 - (float)(samplePosition - fadeOutStartPosition) / loopStream.TransitionLength;
                    fadeVolume = MathF.Max(0, MathF.Min(1, t));
                }
                buffer[offset + i] = (BitConverter.ToSingle(readBuffer, i * bytesPerSample) 
                    + (transitionSample * (1 - fadeVolume))) // Apend fade sample
                    * Volume * fadeVolume; // Track volume, and fade volume.
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
