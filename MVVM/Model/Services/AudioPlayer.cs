using Microsoft.VisualBasic.Devices;
using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;
using System.Diagnostics;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class AudioPlayer
    {
        public event EventHandler<Track>? OnTrackChanged;

        private WaveOutEvent? outputDevice;
        private LoopableCachedAudio? source;

        public bool IsPlaying { get; private set; }
        public Track? ActiveTrack { get; private set; }

        private void CreateSignalChain(Track track)
        {
            CachedAudio cached = new CachedAudio(track.FilePath);
            source = new LoopableCachedAudio(track, cached);
        }

        public void Play(Track track)
        {
            outputDevice?.Dispose();
            source?.Dispose();

            if (ActiveTrack is not null)
                ActiveTrack.OnTrackPlayPositionUpdateRequested -= ActiveTrack_OnTrackPlayPositionUpdateRequested;

            ActiveTrack = track;
            ActiveTrack.OnTrackPlayPositionUpdateRequested += ActiveTrack_OnTrackPlayPositionUpdateRequested;
            outputDevice ??= new WaveOutEvent();

            CreateSignalChain(track);
            source!.OnPositionUpdated += Source_PositionUpdated;

            outputDevice.Init(source);
            outputDevice.Play();
            IsPlaying = true;

            OnTrackChanged?.Invoke(this, track);
        }

        private void ActiveTrack_OnTrackPlayPositionUpdateRequested(object? sender, long e)
        {
            if (source is null) return;
            source.Position = e;
        }

        private void Source_PositionUpdated(object? sender, TrackPositionUpdatedEventArgs e)
        {
            if (ActiveTrack is null) return;
            ActiveTrack.PlayPosition = e.Position;
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;
            if (source is not null) source.OnPositionUpdated -= Source_PositionUpdated;
            source?.Dispose();
            source = null;
            IsPlaying = false;
            if (ActiveTrack is not null)
            {
                ActiveTrack.OnTrackPlayPositionUpdateRequested -= ActiveTrack_OnTrackPlayPositionUpdateRequested;
            }
            ActiveTrack = null;
        }
    }
}
