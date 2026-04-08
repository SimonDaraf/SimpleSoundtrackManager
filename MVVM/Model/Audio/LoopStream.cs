using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class LoopStream : WaveStream
    {
        WaveStream sourceStream;

        public long StartPosition { get; set; } = 0;
        public long LoopPosition { get; set; }

        public LoopStream(WaveStream stream)
        {
            sourceStream = stream;
            LoopPosition = stream.Length;
        }

        public override long Length
        {
            get { return sourceStream.Length; }
        }

        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        public override int Read(byte[] buffer, int offset, int count)
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
    }
}
