using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class SessionTrack : ISampleProvider, IDisposable
    {
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
            int samplesRead;
            if (toReplace is null)
            {
                samplesRead = audio.Read(buffer, offset, count);
                for (int i = 0; i < samplesRead; i++)
                {
                    buffer[i + offset] *= Volume;
                }
            }
            else
            {
                int bpsToReplace = toReplace.WaveFormat.BitsPerSample / 8;
                samplesRead = audio.Read(buffer, offset, count);

                // Create replacement buffer.
                float[] rBuffer = new float[samplesRead];
                toReplace.Read(rBuffer, 0, samplesRead);

                long rStartPos = toReplace.Position - (bpsToReplace * samplesRead);
                for (int i = 0; i < samplesRead; i++)
                {
                    long rSamplePos = rStartPos + (i * bpsToReplace);
                    float transitionVolume = 1f;

                    if (toReplace.StartPosition > 0 || toReplace.TransitionLength > 0)
                        transitionVolume = Math.Max(0, Math.Min(1, rSamplePos / (float)(toReplace.StartPosition + toReplace.TransitionLength)));
                    buffer[i + offset] = ((rBuffer[i] * transitionVolume) + (buffer[i + offset] * (1 - transitionVolume))) * Volume;
                }

                if (toReplace.Position >= toReplace.StartPosition + toReplace.TransitionLength)
                {
                    audio = toReplace;
                    toReplace = null;
                }
            }
            return samplesRead;
        }

        public void Dispose()
        {
            audio.Dispose();
            toReplace?.Dispose();
        }
    }
}
