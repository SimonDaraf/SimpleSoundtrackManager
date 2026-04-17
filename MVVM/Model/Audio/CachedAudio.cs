using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    /// <summary>
    /// A class containing a audio stream stored in memory.
    /// Provides full control over the underlying stream position.
    /// </summary>
    public class CachedAudio : ISampleProvider, IDisposable
    {
        private readonly object _lock = new object();
        private MemoryStream stream;
        public WaveFormat WaveFormat { get; private set; }
        public long Length { get => stream.Length; }

        public long Position
        {
            get => stream.Position;
            set
            {
                stream.Position = AlignBytes(value);
            }
        }

        public CachedAudio(string filePath)
        {
            using AudioFileReader fileReader = new AudioFileReader(filePath);
            WaveFormat = fileReader.WaveFormat;
            byte[] buffer = new byte[fileReader.Length];
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = fileReader.Read(buffer, totalRead, buffer.Length - totalRead);
                if (read == 0) break;
                totalRead += read;
            }
            stream = new MemoryStream(buffer, 0, totalRead);
        }

        public CachedAudio(string filePath, int targetSampleRate)
        {
            using AudioFileReader fileReader = new AudioFileReader(filePath);

            ISampleProvider provider = fileReader.WaveFormat.SampleRate != targetSampleRate
                ? new WdlResamplingSampleProvider(fileReader, targetSampleRate)
                : fileReader;

            if (provider.WaveFormat.Channels == 1)
                provider = new MonoToStereoSampleProvider(provider);

            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(targetSampleRate, provider.WaveFormat.Channels);

            List<byte> bytes = new List<byte>();
            float[] readBuffer = new float[4096];
            int samplesRead;
            while ((samplesRead = provider.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    bytes.AddRange(BitConverter.GetBytes(readBuffer[i]));
                }
            }

            stream = new MemoryStream([..bytes]);
        }

        public CachedAudio CloneCachedAudio()
        {
            lock (_lock)
            {
                long pos = stream.Position;
                stream.Position = 0;
                MemoryStream newStream = new MemoryStream(stream.ToArray());
                stream.Position = pos;
                return new CachedAudio(newStream, WaveFormat);
            }
        }

        private CachedAudio(MemoryStream stream, WaveFormat waveFormat)
        {
            this.stream = stream;
            WaveFormat = waveFormat;
        }

        public long AlignBytes(long value)
        {
            int bytesPerSample = WaveFormat.BitsPerSample / 8;
            int frameSize = bytesPerSample * WaveFormat.Channels;
            return (value / frameSize) * frameSize;
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int bytesPerSample = WaveFormat.BitsPerSample / 8;
            byte[] bytesRead = new byte[count * bytesPerSample];

            int read = stream.Read(bytesRead, 0, count * bytesPerSample);
            int samplesRead = read / bytesPerSample;

            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i + offset] = BitConverter.ToSingle(bytesRead, i * bytesPerSample);
            }

            return samplesRead;
        }
    }
}
