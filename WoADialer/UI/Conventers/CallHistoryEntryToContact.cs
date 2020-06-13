using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml.Data;

namespace WoADialer.UI.Conventers
{
    public sealed class CallHistoryEntryToContact : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PhoneCallHistoryEntry entry && !string.IsNullOrEmpty(entry.Address.ContactId))
            {
                var task0 = App.Current.CallSystem.ContactStore.GetContactAsync(entry.Address.ContactId);
                task0.AsTask().Wait();
                return task0.GetResults();
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
