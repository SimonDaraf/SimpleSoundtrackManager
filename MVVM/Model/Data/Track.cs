using CommunityToolkit.Mvvm.ComponentModel;
using MessagePack;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.Model.Data
{
    [MessagePackObject(AllowPrivate = true)]
    public partial class Track : ObservableObject
    {
        public event EventHandler? OnTrackChanged;

        [Key(0)]
        [ObservableProperty]
        private string name = string.Empty;

        [Key(1)]
        [ObservableProperty]
        private string filePath = string.Empty;

        [Key(2)]
        [ObservableProperty]
        private long startPoint;

        [Key(3)]
        [ObservableProperty]
        private long loopPoint;

        [Key(4)]
        [ObservableProperty]
        private long trackLength;

        [Key(5)]
        [ObservableProperty]
        private long transitionLength;

        [Key(6)]
        [ObservableProperty]
        private int trackVolume;

        [Key(7)]
        [ObservableProperty]
        private SerializableColor trackColor = new SerializableColor();

        public void SetColor(Color color)
        {
            TrackColor = new SerializableColor
            {
                Red = color.R,
                Green = color.G,
                Blue = color.B,
            };
        }

        public void MarkDirty()
        {
            OnTrackChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnFilePathChanging(string? oldValue, string newValue)
        {
            if (oldValue is null || oldValue.Equals(newValue)) return;
            MarkDirty();
        }

        partial void OnNameChanging(string? oldValue, string newValue)
        {
            if (oldValue is null || oldValue.Equals(newValue)) return;
            MarkDirty();
        }

        partial void OnStartPointChanging(long oldValue, long newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
        }

        partial void OnLoopPointChanging(long oldValue, long newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
        }

        partial void OnTrackLengthChanging(long oldValue, long newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
        }

        partial void OnTransitionLengthChanging(long oldValue, long newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
        }

        partial void OnTrackVolumeChanging(int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
        }

        partial void OnTrackColorChanging(SerializableColor? oldValue, SerializableColor newValue)
        {
            if (oldValue is null || oldValue.Equals(newValue)) return;
            MarkDirty();
        }
    }

    [MessagePackObject]
    public class SerializableColor
    {
        [Key(0)]
        public byte Red { get; set; } = 255;

        [Key(1)]
        public byte Green { get; set; } = 255;

        [Key(2)]
        public byte Blue { get; set; } = 255;

        [IgnoreMember]
        public Color Color { get => Color.FromRgb(Red, Green, Blue); }

        [IgnoreMember]
        public SolidColorBrush BrushColor { get => new SolidColorBrush(Color); }
    }
}
