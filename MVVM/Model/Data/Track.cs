using MessagePack;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.Model.Data
{
    [MessagePackObject]
    public class Track
    {
        [Key(0)]
        public string Name { get; set; } = string.Empty;

        [Key(1)]
        public string FilePath { get; set; } = string.Empty;

        [Key(2)]
        public long StartPoint { get; set; }

        [Key(3)]
        public long EndPoint { get; set; }

        [Key(4)]
        public long TransitionLength { get; set; }

        [Key(5)]
        public SerializableColor TrackColor { get; set; } = new SerializableColor();

        public void SetColor(Color color)
        {
            TrackColor = new SerializableColor
            {
                Red = color.R,
                Green = color.G,
                Blue = color.B,
            };
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
