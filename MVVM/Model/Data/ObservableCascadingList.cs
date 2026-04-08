using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SimpleSoundtrackManager.MVVM.Model.Data
{
    public class ObservableCascadingList<T>
    {
        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

        private ObservableCollection<T> values;

        public ObservableCascadingList()
        {
            values = new ObservableCollection<T>();
            values.CollectionChanged += Values_CollectionChanged;
        }

        public ObservableCascadingList(List<T> values)
        {
            this.values = new ObservableCollection<T>(values);
            this.values.CollectionChanged += Values_CollectionChanged;
        }

        private void Values_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
