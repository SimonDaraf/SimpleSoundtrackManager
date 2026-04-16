using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public class TrackPositionUpdatedEventArgs : EventArgs
    {
        public Track? Track { get; set; }
        public long Position { get; set; }
    }

    public enum FadeRegion
    {
        None,
        Start,
        End,
    }

    /// <summary>
    /// A loopable audio stream holding a cached audio source.
    /// </summary>
    public class LoopableCachedAudio : ISampleProvider, IDisposable
    {
        public event EventHandler<TrackPositionUpdatedEventArgs>? OnPositionUpdated;
        private bool isFading;
        private FadeRegion lastFadeRegion = FadeRegion.None;
        private CachedAudio audio;
        private CachedAudio copy;
        private Track track;

        public long StartPosition { get => track.StartPoint; }
        public long LoopPosition { get => track.LoopPoint; }
        public long TransitionLength { get => track.TransitionLength; }
        public long TrackLength { get; private set; }
        public float Volume { get => track.TrackVolume; }

        public LoopableCachedAudio(Track track, CachedAudio cachedAudio)
        {
            audio = cachedAudio;
            this.track = track;
            TrackLength = track.TrackLength;
            audio.Position = StartPosition;
            copy = audio.CloneCachedAudio();
        }

        public WaveFormat WaveFormat => audio.WaveFormat;
        public long Position
        {
            get => audio.Position;
            set 
            {
                audio.Position = value;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int bytesPerSample = audio.WaveFormat.BitsPerSample / 8;

            // Check if this read will advance past the  tracks length. To stop it from removing itself from existens,
            // use this conditional to perform a reset.
            bool willReachEnd = audio.Position + (count * bytesPerSample) >= TrackLength;

            if (TransitionLength == 0)
            {
                if (willReachEnd)
                    audio.Position = StartPosition;

                // If no transition, just check whenever we are passed loop and read from the beginning.
                int samplesRead = audio.Read(buffer, offset, count);
                for (int i = 0; i < samplesRead; i++)
                {
                    buffer[i + offset] *= Volume;
                }

                if (Position >= LoopPosition || samplesRead < count)
                {
                    Position = StartPosition;
                }

                DispatchPositionUpdatedEvent();
                return samplesRead;
            }
            else
            {
                HandleFadeInState(audio.Position);
                if (isFading && !IsInStartFadeRegion(audio.Position))
                {
                    if (willReachEnd)
                    {
                        isFading = false;
                        (audio, copy) = (copy, audio);
                    }

                    float[] copyBuffer = new float[count];
                    FillSampleDuringFade(copyBuffer, count);
                    int samplesRead = audio.Read(buffer, offset, count);

                    long byteStartPosition = audio.Position - (bytesPerSample * samplesRead);
                    for (int i = 0; i < samplesRead; i++)
                    {
                        // If we have read past the loop point. We can just continue to read from the copy, next
                        // read phase will read from the copy anyways so we might as well continue to do so.
                        long samplePosition = byteStartPosition + (i * bytesPerSample);
                        if (samplePosition >= LoopPosition || samplesRead < count)
                        {
                            buffer[i + offset] = copyBuffer[i] * Volume;
                        }
                        else
                        {
                            float fadeVolume = GetFadeVolumeFactor(samplePosition);
                            buffer[i + offset] = ((buffer[i + offset] * fadeVolume) + (copyBuffer[i] * (1f - fadeVolume))) * Volume;
                        }
                    }

                    DispatchPositionUpdatedEvent();
                    return samplesRead;
                }
                else
                {
                    int samplesRead = audio.Read(buffer, offset, count);
                    long byteStartPosition = audio.Position - (bytesPerSample * samplesRead);
                    for (int i = 0; i < samplesRead; i++)
                    {
                        long samplePosition = byteStartPosition + (i * bytesPerSample);
                        float fadeVolume = GetFadeVolumeFactor(samplePosition);
                        buffer[i + offset] *= Volume * fadeVolume;
                    }

                    if (willReachEnd)
                    {
                        audio.Position = StartPosition;
                    }

                    DispatchPositionUpdatedEvent();
                    return samplesRead;
                }
            }
        }

        private void HandleFadeInState(long position)
        {
            if (IsInEndFadeRegion(position))
            {
                if (!isFading || lastFadeRegion != FadeRegion.End)
                {
                    long posOffset = position - (LoopPosition - TransitionLength);
                    copy.Position = StartPosition + posOffset;
                }
                isFading = true;
                lastFadeRegion = FadeRegion.End;
            }
            else if (IsInStartFadeRegion(position))
            {
                if (!isFading || lastFadeRegion != FadeRegion.Start)
                {
                    copy.Position = StartPosition;
                }
                isFading = true;
                lastFadeRegion = FadeRegion.Start;
            }
            else
            {
                if (isFading && lastFadeRegion == FadeRegion.End)
                {
                    (audio, copy) = (copy, audio);
                }
                isFading = false;
                lastFadeRegion = FadeRegion.None;
            }
        }

        private bool IsInStartFadeRegion(long position)
        {
            return position <= StartPosition + TransitionLength && position >= StartPosition;
        }

        private bool IsInEndFadeRegion(long position)
        {
            return position >= LoopPosition - TransitionLength && position <= LoopPosition;
        }

        private float GetFadeVolumeFactor(long position)
        {
            if (TransitionLength == 0) return 1f;

            // Start region.
            if (IsInStartFadeRegion(position))
            {
                float t = (float) (position - StartPosition) / TransitionLength;
                return MathF.Max(0, MathF.Min(1, t));
            }
            // End region.
            else if (IsInEndFadeRegion(position))
            {
                float t = (float) (LoopPosition - position) / TransitionLength;
                return MathF.Max(0, MathF.Min(1, t));
            }

            // Default.
            return 1f;
        }

        private void FillSampleDuringFade(float[] buffer, int count)
        {
            copy.Read(buffer, 0, count);
        }

        private void DispatchPositionUpdatedEvent()
        {
            OnPositionUpdated?.Invoke(this, new TrackPositionUpdatedEventArgs
            {
                Position = Position,
                Track = track
            });
        }

        public void Dispose()
        {
            audio.Dispose();
            copy.Dispose();
        }
    }
}
