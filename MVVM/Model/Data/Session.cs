using MessagePack;
using System.Collections.ObjectModel;

namespace SimpleSoundtrackManager.MVVM.Model.Data
{
    [MessagePackObject]
    public class Session
    {
        [Key(0)]
        public string Name { get; set; } = string.Empty;

        [Key(1)]
        public string FullPath { get; set; } = string.Empty;

        [Key(2)]
        public string DirectoryPath { get; set; } = string.Empty;

        [Key(3)]
        public ObservableCollection<Track> Tracks { get; set; } = [];

        [IgnoreMember]
        public string LastModified { get; set; } = string.Empty;

        [IgnoreMember]
        public bool IsDirty { get; private set; }

        public void Initialize()
        {
            Tracks.CollectionChanged += Tracks_CollectionChanged;
        }

        public void Invalidate()
        {
            Tracks.CollectionChanged -= Tracks_CollectionChanged;
        }

        private void Tracks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MakeDirty();
        }

        private void MakeDirty()
        {
            IsDirty = true;
        }

        public void MarkClean()
        {
            IsDirty = false;
        }
    }
}
