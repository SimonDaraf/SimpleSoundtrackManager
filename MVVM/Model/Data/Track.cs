using CommunityToolkit.Mvvm.ComponentModel;
using MessagePack;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.Model.Data
{
    [MessagePackObject(AllowPrivate = true)]
    public partial class Track : ObservableObject
    {
        public event EventHandler? OnTrackChanged;
        public event EventHandler<long>? OnTrackPlayPositionUpdated;
        public event EventHandler<long>? OnTrackPlayPositionUpdateRequested;
        public event EventHandler<float>? OnTrackVolumeUpdated;

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
        private float trackVolume;

        [Key(7)]
        [ObservableProperty]
        private SerializableColor trackColor = new SerializableColor();

        [Key(8)]
        [ObservableProperty]
        private long lengthInMs;

        [IgnoreMember]
        [ObservableProperty]
        private string startMsPoint = string.Empty;

        [IgnoreMember]
        [ObservableProperty]
        private string loopMsPoint = string.Empty;

        [IgnoreMember]
        [ObservableProperty]
        private string transitionLengthTime = string.Empty;

        [IgnoreMember]
        [ObservableProperty]
        private long playPosition;

        public void ForceUpdateMsView()
        {
            long msStart = BytePosToMs(StartPoint);
            StartMsPoint = FormatTime(msStart);
            long msLoop = BytePosToMs(LoopPoint);
            LoopMsPoint = FormatTime(msLoop);
            TransitionLengthTime = FormatTime(BytePosToMs(TransitionLength));
        }

        private long BytePosToMs(long pos)
        {
            return (long)(pos / (double)TrackLength * LengthInMs);
        }

        private string FormatTime(long ms)
        {
            long minutes = ms / 60000;
            long seconds = (ms % 60000) / 1000;
            long milliseconds = ms % 1000;
            return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
        }

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

        public void RequestPlayPositionChange(long pos)
        {
            OnTrackPlayPositionUpdateRequested?.Invoke(this, pos);
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

            long ms = BytePosToMs(newValue);
            StartMsPoint = FormatTime(ms);
        }

        partial void OnLoopPointChanging(long oldValue, long newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();

            long ms = BytePosToMs(newValue);
            LoopMsPoint = FormatTime(ms);
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
            TransitionLengthTime = FormatTime(BytePosToMs(newValue));
        }

        partial void OnTrackVolumeChanging(float oldValue, float newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
            OnTrackVolumeUpdated?.Invoke(this, newValue);
        }

        partial void OnTrackColorChanging(SerializableColor? oldValue, SerializableColor newValue)
        {
            if (oldValue is null || oldValue.Equals(newValue)) return;
            MarkDirty();
        }

        partial void OnLengthInMsChanging(long oldValue, long newValue)
        {
            if (oldValue == newValue) return;
            MarkDirty();
        }

        partial void OnPlayPositionChanged(long value)
        {
            OnTrackPlayPositionUpdated?.Invoke(this, value);
        }

        partial void OnFilePathChanged(string value)
        {
            OnTrackChanged?.Invoke(this, EventArgs.Empty);
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
