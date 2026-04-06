using MessagePack;

namespace AutomatedSoundtrackSystem.MVVM.Model.Data
{
    [MessagePackObject]
    public class Session
    {
        [Key(0)]
        public string Name { get; set; } = string.Empty;

        [Key(1)]
        public string Path { get; set; } = string.Empty;

        [Key(2)]
        public List<Track> Tracks { get; set; } = [];

        [IgnoreMember]
        public string LastModified { get; set; } = string.Empty;
    }
}
