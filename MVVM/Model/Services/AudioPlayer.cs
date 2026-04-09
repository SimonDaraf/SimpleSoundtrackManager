using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class AudioPlayer
    {
        private WaveOutEvent? outputDevice;
        private LoopStream? loopStream;

        public bool IsPlaying { get; private set; }

        public void Play(Track track)
        {
            outputDevice?.Dispose();
            loopStream?.Dispose();

            outputDevice ??= new WaveOutEvent();
            loopStream = new LoopStream(new AudioFileReader(track.FilePath))
            {
                LoopPosition = track.LoopPoint
            };
            outputDevice.Init(loopStream);
            outputDevice.Play();
            IsPlaying = true;
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;
            loopStream?.Dispose();
            loopStream = null;
            IsPlaying = false;
        }
    }
}
