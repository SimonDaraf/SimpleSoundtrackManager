using CommunityToolkit.Mvvm.ComponentModel;
using SimpleSoundtrackManager.MVVM.Model.Data;
using System.Windows;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class TrackSessionViewModel : ObservableObject
    {
        [ObservableProperty]
        private Track? track;

        [ObservableProperty]
        private LinearGradientBrush trackGradientBrush = new LinearGradientBrush();

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
    }
}
