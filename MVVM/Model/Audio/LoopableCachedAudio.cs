using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Data;
using System.Diagnostics;

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
        private readonly object _lock = new object();

        public event EventHandler<TrackPositionUpdatedEventArgs>? OnPositionUpdated;
        private bool isFading;
        private FadeRegion lastFadeRegion = FadeRegion.None;
        private CachedAudio audio;
        private CachedAudio copy;
        private Track track;

        public long StartPosition { get => track.StartPoint; }
        public long LoopPosition { get => track.LoopPoint; }
        public long TransitionLength { get => track.TransitionLength;}
        public float Volume { get => track.TrackVolume;}

        public LoopableCachedAudio(Track track, CachedAudio cachedAudio)
        {
            audio = cachedAudio;
            this.track = track;
            audio.Position = StartPosition;
            copy = audio.CloneCachedAudio();
        }

        public WaveFormat WaveFormat => audio.WaveFormat;
        public long Position
        {
            get => audio.Position;
            set 
            {
                lock (_lock)
                {
                    // We need to align the position incase position is not alignable.
                    int bytesPerSample = audio.WaveFormat.BitsPerSample / 8;
                    int frameSize = bytesPerSample * audio.WaveFormat.Channels;
                    long alignedValue = (value / frameSize) * frameSize;
                    audio.Position = alignedValue;
                    isFading = false;
                    lastFadeRegion = FadeRegion.None;
                    copy.Position = StartPosition;
                }
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                HandleFadeInState();
                int bytesPerSample = audio.WaveFormat.BitsPerSample / 8;
                if (isFading && !IsInStartFadeRegion())
                {
                    float[] copyBuffer = new float[count];
                    FillSampleDuringFade(copyBuffer, count);

                    int samplesRead = audio.Read(buffer, offset, count);

                    long byteStartPosition = audio.Position - (bytesPerSample * samplesRead);
                    for (int i = 0; i < samplesRead; i++)
                    {
                        float fadeVolume = GetFadeVolumeFactor(byteStartPosition + (i * bytesPerSample));
                        buffer[i + offset] = ((buffer[i + offset] * fadeVolume) + copyBuffer[i]) * Volume;
                    }

                    float maxSample = 0f;
                    for (int i = 0; i < samplesRead; i++)
                        maxSample = MathF.Max(maxSample, MathF.Abs(buffer[i + offset]));

                    DispatchPositionUpdatedEvent();
                    return samplesRead;
                }
                else
                {
                    int samplesRead = audio.Read(buffer, offset, count);
                    long byteStartPosition = audio.Position - (bytesPerSample * samplesRead);
                    for (int i = 0; i < samplesRead; i++)
                    {
                        float fadeVolume = GetFadeVolumeFactor(byteStartPosition + (i * bytesPerSample));
                        buffer[i + offset] *= Volume * fadeVolume;
                    }

                    float maxSample = 0f;
                    for (int i = 0; i < samplesRead; i++)
                        maxSample = MathF.Max(maxSample, MathF.Abs(buffer[i + offset]));

                    DispatchPositionUpdatedEvent();
                    return samplesRead;
                }
            }
        }

        private void HandleFadeInState()
        {
            if (isFading)
            {
                if (!IsInFadeRegions())
                {
                    if (lastFadeRegion == FadeRegion.End)
                    {
                        audio.Position = copy!.Position;
                        isFading = false;
                    }
                    else
                    {
                        isFading = false;
                    }
                    return;
                }
            }
            else
            {
                if (IsInFadeRegions())
                {
                    isFading = true;
                    lastFadeRegion = IsInStartFadeRegion() ? FadeRegion.Start : FadeRegion.End;
                    copy.Position = StartPosition;
                }
            }
        }

        private bool IsInStartFadeRegion()
        {
            return audio.Position >= StartPosition && audio.Position <= StartPosition + TransitionLength;
        }

        private bool IsInEndFadeRegion()
        {
            return audio.Position >= LoopPosition - TransitionLength && audio.Position <= LoopPosition;
        }

        private bool IsInFadeRegions()
        {
            return IsInStartFadeRegion() ||
                IsInEndFadeRegion();
        }

        private float GetFadeVolumeFactor(long position)
        {
            // Start region.
            if (position >= StartPosition && position <= StartPosition + TransitionLength)
            {
                float t = (float) (position - StartPosition) / TransitionLength;
                return MathF.Max(0, MathF.Min(1, t));
            }
            // End region.
            else if (position >= LoopPosition - TransitionLength && position <= LoopPosition)
            {
                float t = (float) (LoopPosition - position) / TransitionLength;
                return MathF.Max(0, MathF.Min(1, t));
            }

            // Default.
            return 1f;
        }

        private void FillSampleDuringFade(float[] buffer, int count)
        {
            int read = copy.Read(buffer, 0, count);
            // Convert to byte position.
            int bytesPerSample = copy.WaveFormat.BitsPerSample / 8;
            long byteStartPosition = copy.Position - (read * bytesPerSample);
            for (int i = 0; i < read; i++)
            {
                float transitionVolume = GetFadeVolumeFactor(byteStartPosition + (i * bytesPerSample));
                buffer[i] *= transitionVolume;
            }
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
