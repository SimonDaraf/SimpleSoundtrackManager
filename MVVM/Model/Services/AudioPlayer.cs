using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class AudioPlayer
    {
        public event EventHandler<Track>? OnTrackChanged;

        private WaveOutEvent? outputDevice;
        private LoopStream? loopStream;

        public bool IsPlaying { get; private set; }
        public Track? ActiveTrack { get; private set; }

        public void Play(Track track)
        {
            outputDevice?.Dispose();
            loopStream?.Dispose();

            ActiveTrack = track;
            outputDevice ??= new WaveOutEvent();
            loopStream = new LoopStream(new AudioFileReader(track.FilePath))
            {
                StartPosition = track.StartPoint,
                LoopPosition = track.LoopPoint
            };
            outputDevice.Init(loopStream);
            outputDevice.Play();
            IsPlaying = true;

            OnTrackChanged?.Invoke(this, track);
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;
            loopStream?.Dispose();
            loopStream = null;
            IsPlaying = false;
            ActiveTrack = null;
        }
    }
}
