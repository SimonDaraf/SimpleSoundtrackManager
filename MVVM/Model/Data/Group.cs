using MessagePack;
using System.Windows.Media;

namespace AutomatedSoundtrackSystem.MVVM.Model.Data
{
    [MessagePackObject]
    public class Group
    {
        [Key(0)]
        public string Name { get; set; } = string.Empty;

        [Key(1)]
        public string Path { get; set; } = string.Empty;
    }
}
