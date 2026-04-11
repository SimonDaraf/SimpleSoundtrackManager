using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class AudioPlayer
    {
        public event EventHandler<Track>? OnTrackChanged;

        private WaveOutEvent? outputDevice;
        private TrackSignalParent? source;

        public bool IsPlaying { get; private set; }
        public Track? ActiveTrack { get; private set; }

        private void CreateSignalChain(Track track)
        {
            LoopStream loopStream = new LoopStream(new AudioFileReader(track.FilePath))
            {
                StartPosition = track.StartPoint,
                LoopPosition = track.LoopPoint,
                TransitionLength = track.TransitionLength,
            };

            source = new TrackSignalParent(loopStream);
            source.Volume = track.TrackVolume;
        }

        public void Play(Track track)
        {
            outputDevice?.Dispose();
            source?.Dispose();

            ActiveTrack = track;
            ActiveTrack.OnTrackPlayPositionUpdateRequested += ActiveTrack_OnTrackPlayPositionUpdateRequested;
            outputDevice ??= new WaveOutEvent();

            CreateSignalChain(track);
            source!.PositionUpdated += Source_PositionUpdated;

            outputDevice.Init(source);
            outputDevice.Play();
            IsPlaying = true;

            OnTrackChanged?.Invoke(this, track);
        }

        private void ActiveTrack_OnTrackPlayPositionUpdateRequested(object? sender, long e)
        {
            source.ForcePositionChange(e);
        }

        private void Source_PositionUpdated(object? sender, long e)
        {
            if (ActiveTrack is null) return;
            ActiveTrack.PlayPosition = e;
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;
            if (source is not null) source.PositionUpdated -= Source_PositionUpdated;
            source?.Dispose();
            source = null;
            IsPlaying = false;
            ActiveTrack.OnTrackPlayPositionUpdateRequested -= ActiveTrack_OnTrackPlayPositionUpdateRequested;
            ActiveTrack = null;
        }

        public void UpdateVolume(float volume)
        {
            if (source is null) return;
            source.Volume = volume;
        }
    }
}
