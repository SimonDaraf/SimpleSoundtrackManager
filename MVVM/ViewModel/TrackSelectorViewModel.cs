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
        private readonly SessionTracker sessionTracker;

        [ObservableProperty]
        private Track? track;

        [ObservableProperty]
        private SolidColorBrush brushColor = new SolidColorBrush();

        [ObservableProperty]
        private LinearGradientBrush trackGradientBrush = new LinearGradientBrush();

        public TrackSelectorViewModel(SessionTracker sessionTracker)
        {
            this.sessionTracker = sessionTracker;
        }

        partial void OnTrackChanged(Track? value)
        {
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (Track is null) return;
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
        private void OpenColorPicker()
        {
            ColorPickerWindow pickerWindow = new ColorPickerWindow(Track?.TrackColor.Color) { Owner = App.Current.MainWindow };
            if (pickerWindow.ShowDialog() == true && Track is not null)
            {
                SKColor color = pickerWindow.SelectedColor;
                Track.SetColor(Color.FromRgb(color.Red, color.Green, color.Blue));
                UpdateColors();
            }
        }
    }
}
