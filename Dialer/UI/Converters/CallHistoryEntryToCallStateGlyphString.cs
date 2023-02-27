using Microsoft.UI.Xaml.Data;
using System;
using Windows.ApplicationModel.Calls;

namespace Dialer.UI.Converters
{
    public sealed class CallHistoryEntryToCallStateGlyphString : IValueConverter
    {
        public static string Convert(PhoneCallHistoryEntry entry)
        {
            return entry.IsIncoming
                ? entry.IsCallerIdBlocked
                    ? Glyphs.CALL_HISTORY_BLOCKED
                    : entry.IsMissed ? Glyphs.CALL_HISTORY_MISSED : Glyphs.CALL_HISTORY_INCOMING
                : Glyphs.CALL_HISTORY_OUTGOING;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                PhoneCallHistoryEntry entry => Convert(entry),
                _ => string.Empty,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
