using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class SessionTrack : ISampleProvider, IDisposable
    {
        public event EventHandler<float[]>? OnBufferProcessed;

        private LoopableCachedAudio audio;
        private LoopableCachedAudio? toReplace;

        public WaveFormat WaveFormat => audio.WaveFormat;

        public float Volume { get; set; } = 1f;

        public SessionTrack(LoopableCachedAudio audio)
        {
            this.audio = audio;
        }

        public void RequestReplacement(LoopableCachedAudio audio)
        {
            audio.Position = audio.StartPosition;
            toReplace = audio;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = 0;
            float[] copy = [];
            if (toReplace is null)
            {
                samplesRead = audio.Read(buffer, offset, count);
                copy = new float[samplesRead];
                for (int i = 0; i < samplesRead; i++)
                {
                    buffer[i + offset] *= Volume;
                    copy[i] = buffer[i + offset];
                }
            }
            else
            {
                int bpsToReplace = toReplace.WaveFormat.BitsPerSample / 8;
                samplesRead = audio.Read(buffer, offset, count);
                copy = new float[samplesRead];

                // Create replacement buffer.
                float[] rBuffer = new float[samplesRead];
                int rSamplesRead = toReplace.Read(rBuffer, 0, samplesRead);

                long rStartPos = toReplace.Position - (bpsToReplace * samplesRead);
                for (int i = 0; i < samplesRead; i++)
                {
                    long rSamplePos = rStartPos + (i * bpsToReplace);
                    float transitionVolume = 1f;

                    if (toReplace.StartPosition > 0 || toReplace.TransitionLength > 0)
                        transitionVolume = Math.Max(0, Math.Min(1, rSamplePos / (float)(toReplace.StartPosition + toReplace.TransitionLength)));
                    buffer[i + offset] = ((rBuffer[i] * transitionVolume) + (buffer[i + offset] * (1 - transitionVolume))) * Volume;

                    // This is just to dispatch waveform info to any external visual component.
                    copy[i] = buffer[i + offset];
                }

                if (toReplace.Position >= toReplace.StartPosition + toReplace.TransitionLength)
                {
                    audio = toReplace;
                    toReplace = null;
                }
            }

            Task.Run(() => OnBufferProcessed?.Invoke(this, copy));
            return samplesRead;
        }

        public void Dispose()
        {
            audio.Dispose();
            toReplace?.Dispose();
        }
    }
}
