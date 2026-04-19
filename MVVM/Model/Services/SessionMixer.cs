using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class SessionMixer : IDisposable
    {
        private readonly SettingsManager settingsManager;
        private Dictionary<Track, LoopableCachedAudio> tracks;
        private WaveOutEvent? waveOut;
        private SessionTrack? sessionTrack;

        public bool IsPlaying { get; private set; } = false;

        public SessionMixer(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            tracks = new Dictionary<Track, LoopableCachedAudio>();
        }

        public void CacheAudioData(IEnumerable<Track> tracks)
        {
            this.tracks = new Dictionary<Track, LoopableCachedAudio>();
            foreach (Track track in tracks)
            {
                LoopableCachedAudio audio = new LoopableCachedAudio(track, new CachedAudio(track.Name, track.FilePath, 44100));
                audio.OnBufferRead += Audio_OnBufferRead;
                track.Channels = audio.WaveFormat.Channels;
                track.SampleRate = audio.WaveFormat.SampleRate;
                this.tracks.Add(track, audio);
            }
        }

        /// <summary>
        /// Sets the global session volume.
        /// </summary>
        public void SetVolume(float volume)
        {
            if (sessionTrack is null)
                return;
            sessionTrack.Volume = volume;
        }

        /// <summary>
        /// Initializes the session mixer with an initial track to play.
        /// </summary>
        public void Init(Track track)
        {
            if (IsPlaying)
                return;
            if (tracks.TryGetValue(track, out LoopableCachedAudio? audio))
            {
                if (sessionTrack is not null)
                    throw new Exception("Cannot initialize session mixer, already initialized.");

                sessionTrack = new SessionTrack(audio);
                waveOut = new WaveOutEvent();
                waveOut.DeviceNumber = settingsManager.GetSelectedDevice();
                waveOut.Init(sessionTrack);
                waveOut.Play();
                IsPlaying = true;
            }
            else
            {
                throw new Exception("Track not within initialized tracks.");
            }
        }

        /// <summary>
        /// Starts a session mixer with no specified track.
        /// </summary>
        public void InitEmpty()
        {
            sessionTrack = new SessionTrack();
            waveOut = new WaveOutEvent();
            waveOut.Init(sessionTrack);
            waveOut.DeviceNumber = settingsManager.GetSelectedDevice();
            waveOut.Play();
            IsPlaying = true;
        }

        /// <summary>
        /// Requests that a track is added as an overlay.
        /// If track is already an overlay track this removes it instead.
        /// </summary>
        public void AddTrackAsOverlay(Track track)
        {
            if (sessionTrack is null || !tracks.TryGetValue(track, out LoopableCachedAudio? audio) 
                || sessionTrack.IsOverlay(track.Name))
                return;

            sessionTrack.AddOverlayTrack(track.Name, audio);
        }

        /// <summary>
        /// Requests that an overlay track is removed.
        /// </summary>
        public void RemoveTrackAsOverlay(Track track)
        {
            if (sessionTrack is null || !tracks.ContainsKey(track) || !sessionTrack.IsOverlay(track.Name))
                return;

            sessionTrack.RemoveOverlayTrack(track.Name);
        }

        public bool IsOverlay(Track track)
        {
            if (sessionTrack is null)
                return false;

            return sessionTrack.IsOverlay(track.Name);
        }

        /// <summary>
        /// Requests a track change, if no track is playing this starts specified track.
        /// If the same track is playing, make it fade out.
        /// </summary>
        public void RequestChange(Track track)
        {
            if (sessionTrack is null)
                return;

            if (tracks.TryGetValue(track, out LoopableCachedAudio? audio))
            {
                LoopableCachedAudio? cTrack = sessionTrack.GetCurrentPlayingTrack();
                if (cTrack is null)
                {
                    sessionTrack.RequestStart(audio);
                    IsPlaying = true;
                }
                else if (cTrack is not null && cTrack.Equals(audio))
                {
                    sessionTrack.RequestFadeOut();
                    IsPlaying = false;
                }
                else
                {
                    sessionTrack.RequestReplacement(audio);
                }  
            }
        }

        /// <summary>
        /// Stops the session.
        /// </summary>
        public void Stop()
        {
            waveOut?.Stop();
            waveOut = null;
            if (sessionTrack is not null)
            {
                sessionTrack.Dispose();
                sessionTrack = null;
            }
        }

        private void Audio_OnBufferRead(object? sender, TrackBufferUpdatedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(e);
            return;
        }

        public void Dispose()
        {
            foreach (LoopableCachedAudio cachedAudio in tracks.Values)
            {
                cachedAudio.OnBufferRead -= Audio_OnBufferRead;
                cachedAudio.Dispose();
            }
        }
    }
}
