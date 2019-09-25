using Internal.Windows.Calls;
using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml.Data;
using WoADialer.UI.ViewModel;

namespace WoADialer.UI.Conventers
{
    public sealed class CallHistoryEntryToCallStateTextString : IValueConverter
    {
        public static string Convert(PhoneCallHistoryEntry entry)
        {
            if (entry.IsIncoming)
            {
                if (entry.IsCallerIdBlocked)
                {
                    return "Blocked";
                }
                else if (entry.IsMissed)
                {
                    return "Missed";
                }
                else
                {
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Incoming));
                }
            }
            else
            {
                return "Outgoing";
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch(value)
            {
                case PhoneCallHistoryEntry entry:
                    return Convert(entry);
                case null:
                default:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + "_Unknown");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
