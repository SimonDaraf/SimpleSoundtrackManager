using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public class OnTrackChangeRequestedEventArgs : EventArgs
    {
        public required Track Track { get; set; }
        public bool IsOverlay { get; set; }
    }

    public partial class TrackSessionViewModel : ObservableObject, IDisposable
    {
        public event EventHandler<OnTrackChangeRequestedEventArgs>? OnTrackChangeRequested;

        [ObservableProperty]
        private Track? track;

        [ObservableProperty]
        private LinearGradientBrush trackGradientBrush = new LinearGradientBrush();

        [ObservableProperty]
        private bool isActive = false;

        [ObservableProperty]
        private string state = "";

        bool isShiftDown = false;

        public TrackSessionViewModel()
        {
            StaticKeyManager.OnKeyDown += StaticKeyManager_OnKeyDown;
            StaticKeyManager.OnKeyUp += StaticKeyManager_OnKeyUp;
        }

        private void StaticKeyManager_OnKeyUp(object? sender, Key e)
        {
            if (e == Key.LeftShift)
                isShiftDown = false;
        }

        private void StaticKeyManager_OnKeyDown(object? sender, Key e)
        {
            if (e == Key.LeftShift)
                isShiftDown = true;
        }

        partial void OnTrackChanged(Track? value)
        {
            if (value == null) return;
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (Track is null) return;
            Color trackColor = Track.TrackColor.Color;
            SolidColorBrush brushColor = new SolidColorBrush(Track.TrackColor.Color);
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
        private void Click()
        {
            if (Track is null) return;
            OnTrackChangeRequested?.Invoke(this, new OnTrackChangeRequestedEventArgs { Track = Track, IsOverlay = isShiftDown });
        }

        public void Dispose()
        {
            StaticKeyManager.OnKeyDown -= StaticKeyManager_OnKeyDown;
            StaticKeyManager.OnKeyUp -= StaticKeyManager_OnKeyUp;
        }
    }
}
