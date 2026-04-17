using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class SessionMixer : IDisposable
    {
        private Dictionary<Track, LoopableCachedAudio> tracks;
        private WaveOutEvent? waveOut;
        private SessionTrack? sessionTrack;

        public bool IsPlaying { get; private set; } = false;

        public SessionMixer(IEnumerable<Track> tracks)
        {
            this.tracks = new Dictionary<Track, LoopableCachedAudio>();
            foreach (Track track in tracks)
            {
                LoopableCachedAudio audio = new LoopableCachedAudio(track, new CachedAudio(track.FilePath, 44100));
                track.Channels = audio.WaveFormat.Channels;
                track.SampleRate = audio.WaveFormat.SampleRate;
                this.tracks.Add(track, audio);
            }
        }

        public void SetVolume(float volume)
        {
            if (sessionTrack is null)
                return;
            sessionTrack.Volume = volume;
        }

        public void Init(Track track)
        {
            if (tracks.TryGetValue(track, out LoopableCachedAudio? audio))
            {
                if (sessionTrack is not null)
                    throw new Exception("Cannot initialize session mixer, already initialized.");

                sessionTrack = new SessionTrack(audio);
                sessionTrack.OnBufferProcessed += SessionTrack_OnBufferProcessed;
                waveOut = new WaveOutEvent();
                waveOut.Init(sessionTrack);
                waveOut.Play();
                IsPlaying = true;
            }
            else
            {
                throw new Exception("Track not within initialized tracks.");
            }
        }

        public void RequestChange(Track track)
        {
            if (sessionTrack is null)
                return;

            if (tracks.TryGetValue(track, out LoopableCachedAudio? audio))
            {
                sessionTrack.RequestReplacement(audio);
            }
        }

        public void Stop()
        {
            waveOut?.Stop();
            waveOut = null;
            if (sessionTrack is not null)
            {
                sessionTrack.OnBufferProcessed -= SessionTrack_OnBufferProcessed;
                sessionTrack.Dispose();
                sessionTrack = null;
            }
        }

        private void SessionTrack_OnBufferProcessed(object? sender, float[] e)
        {
            WeakReferenceMessenger.Default.Send(e);
        }

        public void Dispose()
        {
            foreach (LoopableCachedAudio cachedAudio in tracks.Values)
            {
                cachedAudio.Dispose();
            }
        }
    }
}
