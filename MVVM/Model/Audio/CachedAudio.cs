using Microsoft.VisualBasic.Devices;
using NAudio.Wave;
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

        public long Position
        {
            get => stream.Position;
            set
            {
                int bytesPerSample = WaveFormat.BitsPerSample / 8;
                int frameSize = bytesPerSample * WaveFormat.Channels;
                stream.Position = (value / frameSize) * frameSize;
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

        public void Dispose()
        {
            stream.Dispose();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int bytesPerSample = WaveFormat.BitsPerSample / 8;
            byte[] bytesRead = new byte[count * bytesPerSample];

            long positionBefore = stream.Position;
            int read = stream.Read(bytesRead, offset, count * bytesPerSample);
            int samplesRead = read / bytesPerSample;

            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i + offset] = BitConverter.ToSingle(bytesRead, i * bytesPerSample);
            }

            return samplesRead;
        }
    }
}
