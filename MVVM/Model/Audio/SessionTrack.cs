using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class SessionTrack : ISampleProvider, IDisposable
    {
        public event EventHandler<float[]>? OnBufferProcessed;

        private LoopableCachedAudio audio;
        private LoopableCachedAudio? toReplace;

        public WaveFormat WaveFormat => audio.WaveFormat;

        public float Volume { get; set; }

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
                // Do stuff later
                samplesRead = audio.Read(buffer, offset, count);
                copy = new float[samplesRead];
                for (int i = 0; i < samplesRead; i++)
                {
                    buffer[i + offset] *= Volume;
                    copy[i] = buffer[i + offset];
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
