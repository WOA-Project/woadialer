using Microsoft.UI.Dispatching;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dialer.UI.ViewModel
{
    public abstract class ViewModelCore : INotifyPropertyChanged
    {
        protected DispatcherQueue DispatcherQueue
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected ViewModelCore(DispatcherQueue dispatcherQueue)
        {
            DispatcherQueue = dispatcherQueue;
        }

        protected virtual void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
