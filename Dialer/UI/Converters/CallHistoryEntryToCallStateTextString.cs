using Internal.Windows.Calls;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.ApplicationModel.Calls;

namespace Dialer.UI.Converters
{
    public sealed class CallHistoryEntryToCallStateTextString : IValueConverter
    {
        public static string Convert(PhoneCallHistoryEntry entry)
        {
            return entry.IsIncoming
                ? entry.IsCallerIdBlocked
                    ? "Blocked"
                    : entry.IsMissed ? "Missed" : App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Incoming))
                : "Outgoing";
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                PhoneCallHistoryEntry entry => Convert(entry),
                _ => App.Current.ResourceLoader.GetString(nameof(CallState) + "_Unknown"),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
