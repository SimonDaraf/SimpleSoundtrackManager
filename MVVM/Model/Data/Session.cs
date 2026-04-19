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

        private float volume = -1;

        [Key(4)]
        public float Volume
        {
            get => volume;
            set
            {
                if (value != Volume && volume != -1)
                    MakeDirty();
                volume = value;
            }
        }

        [IgnoreMember]
        public string LastModified { get; set; } = string.Empty;

        [IgnoreMember]
        public bool IsDirty { get; private set; }

        public event EventHandler<bool>? OnDirtyStateChanged;

        public void Initialize()
        {
            Tracks.CollectionChanged += Tracks_CollectionChanged;
            foreach (Track t in Tracks)
            {
                t.OnTrackChanged += InternalTrackChanged;
            }
        }

        public void Invalidate()
        {
            Tracks.CollectionChanged -= Tracks_CollectionChanged;
        }

        private void Tracks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MakeDirty();
            if (e.OldItems is not null)
            {
                foreach (Track t in e.OldItems)
                {
                    t.OnTrackChanged -= InternalTrackChanged;
                }
            }
            if (e.NewItems is not null)
            {
                foreach (Track t in e.NewItems)
                {
                    t.OnTrackChanged += InternalTrackChanged;
                }
            }
        }

        private void InternalTrackChanged(object? sender, EventArgs e)
        {
            MakeDirty();
        }

        private void MakeDirty()
        {
            if (IsDirty) return;
            IsDirty = true;
            OnDirtyStateChanged?.Invoke(this, true);
        }

        public void MarkClean()
        {
            if (!IsDirty) return;
            IsDirty = false;
            OnDirtyStateChanged?.Invoke(this, false);
        }
    }
}
