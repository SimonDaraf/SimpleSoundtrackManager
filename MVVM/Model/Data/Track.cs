using MessagePack;

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
    }
}
