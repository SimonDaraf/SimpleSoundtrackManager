using NAudio.Gui;
using NAudio.Wave;

namespace SimpleSoundtrackManager.MVVM.Model.Audio
{
    public enum SessionState
    {
        Empty,
        Playing,
        Replacing,
        FadingOut,
    }

    /// <summary>
    /// Used to track when an overlay fade out was requested.
    /// Keeping sessionTrack more clean.
    /// </summary>
    public class OverlayFadeWrapper
    {
        public required LoopableCachedAudio OverlayAudio { get; set; }
        public bool FadeOut { get; set; } = false;
        public long BytesReadSinceFadeOutRequest { get; set; } = 0;
        public bool IsDone => BytesReadSinceFadeOutRequest >= OverlayAudio.TransitionLength;
    }

    public class SessionTrack : ISampleProvider, IDisposable
    {
        private readonly ISampleProvider placeholder;
        private LoopableCachedAudio? audio;
        private LoopableCachedAudio? toReplace;

        private Dictionary<string, OverlayFadeWrapper> overlays;

        private long fadeOutPos;
        private SessionState state;

        public WaveFormat WaveFormat => audio is not null ? audio.WaveFormat : placeholder.WaveFormat;

        public float Volume { get; set; } = 1f;

        public SessionTrack()
        {
            overlays = new Dictionary<string, OverlayFadeWrapper>();
            placeholder = new EmptyAudioSource();
            state = SessionState.Empty;
        }

        public SessionTrack(LoopableCachedAudio audio)
        {
            overlays = new Dictionary<string, OverlayFadeWrapper>();
            state = SessionState.Playing;
            this.audio = audio;
            placeholder = new EmptyAudioSource();
        }

        public LoopableCachedAudio? GetCurrentPlayingTrack()
        {
            return audio;
        }

        /// <summary>
        /// Returns whether a registered overlay with passed identifier exists.
        /// </summary>
        public bool IsOverlay(string identifier)
        {
            return overlays.ContainsKey(identifier);
        }

        /// <summary>
        /// Appends an overlay track to the active session.
        /// </summary>
        public void AddOverlayTrack(string identifier, LoopableCachedAudio audio)
        {
            if (overlays.ContainsKey(identifier))
                return;

            overlays.Add(identifier, new OverlayFadeWrapper { OverlayAudio = audio });
        }

        /// <summary>
        /// Removes a track as an overlay.
        /// </summary>
        public void RemoveOverlayTrack(string identifier)
        {
            if (!overlays.ContainsKey(identifier))
                return;

            if (!overlays.TryGetValue(identifier, out OverlayFadeWrapper? overlayTrack))
                return;

            overlayTrack.FadeOut = true;
        }

        /// <summary>
        /// Requests that the session is started with specified track.
        /// If session is already playing this does nothing, use replace instead.
        /// </summary>
        public void RequestStart(LoopableCachedAudio audio)
        {
            if (state is not SessionState.Empty)
                return;

            foreach (string key in overlays.Keys)
            {
                if (overlays.TryGetValue(key, out OverlayFadeWrapper? overlay) && overlay.OverlayAudio.Equals(audio))
                {
                    // Handle edge case where we want to transition from an overlay to base track.
                    overlays.Remove(key);
                    this.audio = audio;
                    state = SessionState.Playing;
                    return;
                }
            }

            audio.Position = audio.StartPosition;
            this.audio = audio;
            state = SessionState.Playing;
        }

        /// <summary>
        /// Request the session to remove the base track and fade towards an empty state.
        /// </summary>
        public void RequestFadeOut()
        {
            if (state == SessionState.Replacing || state == SessionState.FadingOut || audio is null)
                return;

            fadeOutPos = audio.Position;
            state = SessionState.FadingOut;
        }

        /// <summary>
        /// Request the session to replace the base track.
        /// </summary>
        public void RequestReplacement(LoopableCachedAudio audio)
        {
            foreach (string key in overlays.Keys)
            {
                if (overlays.TryGetValue(key, out OverlayFadeWrapper? overlay) && overlay.OverlayAudio.Equals(audio))
                {
                    // Handle edge case where we want to transition from an overlay to base track.
                    overlays.Remove(key);
                    if (this.audio is null)
                    {
                        this.audio = audio;
                        state = SessionState.Playing;
                        return;
                    }
                }
            }
            audio.Position = audio.StartPosition;
            toReplace = audio;
            state = SessionState.Replacing;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return state switch
            {
                SessionState.Empty => Empty(buffer, offset, count),
                SessionState.Playing => Playing(buffer, offset, count),
                SessionState.Replacing => Replacing(buffer, offset, count),
                SessionState.FadingOut => FadeOut(buffer, offset, count),
                _ => Empty(buffer, offset, count),
            };
        }

        private int Empty(float[] buffer, int offset, int count)
        {
            int samplesRead = placeholder.Read(buffer, offset, count);
            ReadOverlay(buffer, offset, samplesRead);
            return samplesRead;
        }

        private int Playing(float[] buffer, int offset, int count)
        {
            // Must verify before, else this is an invalid state and we should stop.
            if (audio is null)
                return Empty(buffer, offset, count);
            int samplesRead = audio.Read(buffer, offset, count);
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[i + offset] *= Volume;
            }
            ReadOverlay(buffer, offset, samplesRead);
            return samplesRead;
        }

        private int Replacing(float[] buffer, int offset, int count)
        {
            // Invalid state.
            if (audio is null || toReplace is null)
                return Empty(buffer, offset, count);

            int bpsToReplace = toReplace.WaveFormat.BitsPerSample / 8;
            int samplesRead = audio.Read(buffer, offset, count);

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
                state = SessionState.Playing;
            }

            ReadOverlay(buffer, offset, samplesRead);
            return samplesRead;
        }

        private int FadeOut(float[] buffer, int offset, int count)
        {
            // Invalid state.
            if (audio is null)
                return Empty(buffer, offset, count);

            int bps = audio.WaveFormat.BitsPerSample / 8;
            int samplesRead = audio.Read(buffer, offset, count);
            long dest = fadeOutPos + audio.TransitionLength;
            long startPos = audio.Position - (bps * samplesRead);

            if (dest == 0)
                return Empty(buffer, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                long pos = startPos + (i * bps);
                float volumeFactor = 1 - (Math.Max(0, Math.Min(1, (float)(pos - fadeOutPos) / audio.TransitionLength)));
                buffer[i + offset] *= volumeFactor;
            }

            if (audio.Position >= dest)
            {
                state = SessionState.Empty;
                audio = null;
            }

            ReadOverlay(buffer, offset, samplesRead);
            return samplesRead;
        }

        private void ReadOverlay(float[] buffer, int offset, int samplesRead)
        {
            float[] overlayData;
            foreach (KeyValuePair<string, OverlayFadeWrapper> overlayItem in overlays)
            {
                OverlayFadeWrapper overlay = overlayItem.Value;
                overlayData = new float[samplesRead];
                overlay.OverlayAudio.Read(overlayData, 0, samplesRead);

                int bytesPerSample = overlay.OverlayAudio.WaveFormat.BitsPerSample / 8;
                long bytesRead = samplesRead * bytesPerSample;
                if (overlay.FadeOut)
                    overlay.BytesReadSinceFadeOutRequest += bytesRead;

                long startPos = overlay.BytesReadSinceFadeOutRequest - bytesRead;
                for (int i = 0; i < samplesRead; i++)
                {
                    if (offset + i >= buffer.Length && i >= overlayData.Length)
                        break;

                    float sample = overlayData[i];

                    if (overlay.FadeOut)
                    {
                        long relativePos = startPos + (i * bytesPerSample);
                        float volumeFactor;
                        if (overlay.OverlayAudio.TransitionLength == 0)
                            volumeFactor = 0f;
                        else
                            volumeFactor = 1 - (Math.Max(0, Math.Min(1, (float)(relativePos - fadeOutPos) / overlay.OverlayAudio.TransitionLength)));

                        sample *= volumeFactor;
                    }

                    buffer[offset + i] += sample;
                }

                if (overlay.IsDone)
                    overlays.Remove(overlayItem.Key);
            }
        }

        public void Dispose()
        {
            audio?.Dispose();
            toReplace?.Dispose();
        }
    }
}
