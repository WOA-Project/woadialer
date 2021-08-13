using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Dialer.Helpers
{
    public sealed class ReadOnlyObservableCollection2<T> : ReadOnlyObservableCollection<T>
    {
        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => base.CollectionChanged += value;
            remove => base.CollectionChanged -= value;
        }

        public ReadOnlyObservableCollection2(ObservableCollection<T> list) : base(list)
        {
        }
    }
}
