using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

namespace WoADialer.UI.ViewModel
{
    public abstract class ViewModelCore : INotifyPropertyChanged
    {
        protected CoreDispatcher Dispatcher { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelCore(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        protected virtual void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected async void OnPropertyChanged(string propertyName)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
