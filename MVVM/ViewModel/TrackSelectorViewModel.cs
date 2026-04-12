using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using SimpleSoundtrackManager.MVVM.View.Components;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class TrackSelectorViewModel : ObservableObject
    {
        private readonly SessionManager sessionManager;
        private readonly SessionTracker sessionTracker;
        private readonly AudioPlayer audioPlayer;

        [ObservableProperty]
        private Track? track;

        [ObservableProperty]
        private SolidColorBrush brushColor = new SolidColorBrush();

        [ObservableProperty]
        private string playbackState = "Play";

        [ObservableProperty]
        private LinearGradientBrush trackGradientBrush = new LinearGradientBrush();

        [ObservableProperty]
        private Color trackColor;

        [ObservableProperty]
        private bool isSource = false;

        public TrackSelectorViewModel(SessionManager sessionManager, SessionTracker sessionTracker, AudioPlayer audioPlayer)
        {
            this.sessionTracker = sessionTracker;
            this.sessionManager = sessionManager;
            this.audioPlayer = audioPlayer;
            this.audioPlayer.OnTrackChanged += AudioPlayer_OnTrackChanged;
        }

        private void AudioPlayer_OnTrackChanged(object? sender, Track e)
        {
            if (Track is not null && !e.Equals(Track))
            {
                PlaybackState = "Play";
                IsSource = false;
            }
        }

        partial void OnTrackChanged(Track? value)
        {
            if (value == null) return;
            value.ForceUpdateMsView();
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (Track is null) return;
            TrackColor = Track.TrackColor.Color;
            BrushColor = new SolidColorBrush(Track.TrackColor.Color);
            Color transparent = Color.FromArgb(0, Track.TrackColor.Color.R, Track.TrackColor.Color.G, Track.TrackColor.Color.B);
            Color subOpacity = Color.FromArgb(5, Track.TrackColor.Color.R, Track.TrackColor.Color.G, Track.TrackColor.Color.B);
            TrackGradientBrush = new LinearGradientBrush(
                [
                    new GradientStop(transparent, 0),
                    new GradientStop(subOpacity, 1)
                ],
                new Point(0, 0), new Point(0, 1));
        }

        [RelayCommand]
        private void Delete()
        {
            if (Track is null) return;
            sessionTracker.RemoveTrackFromActiveSesion(Track);
        }

        [RelayCommand]
        private void Browse()
        {
            if (Track is null) return;
            sessionManager.ReplaceAudioFileInTrack(Track);
        }

        [RelayCommand]
        private void OpenColorPicker()
        {
            ColorPickerWindow pickerWindow = new ColorPickerWindow(Track?.TrackColor.Color) { Owner = App.Current.MainWindow };
            if (pickerWindow.ShowDialog() == true && Track is not null)
            {
                SKColor color = pickerWindow.SelectedColor;
                Track.SetColor(Color.FromRgb(color.Red, color.Green, color.Blue));
                TrackColor = Track.TrackColor.Color;
                UpdateColors();
            }
        }

        [RelayCommand]
        private void TogglePlayback()
        {
            if (Track is null) return;
            if (audioPlayer.IsPlaying)
            {
                if (!IsSource)
                {
                    audioPlayer.Stop();
                    audioPlayer.Play(Track);
                    PlaybackState = "Stop";
                    IsSource = true;
                    return;
                }

                audioPlayer.Stop();
                PlaybackState = "Play";
                IsSource = false;
            } 
            else
            {
                audioPlayer.Play(Track);
                PlaybackState = "Stop";
                IsSource = true;
            }
        }
    }
}
