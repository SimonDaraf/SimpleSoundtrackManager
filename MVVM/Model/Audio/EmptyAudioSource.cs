using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    /// <summary>
    /// Should just pump out zeroes, so that the wave out event doesnt close itself when nothing is playing.
    /// </summary>
    public class EmptyAudioSource : ISampleProvider
    {
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public int Read(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[i + offset] = 0;
            }

            return count;
        }
    }
}
