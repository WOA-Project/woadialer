using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Converters
{
    public sealed class CallHistoryEntryToCallStateGlyphString : IValueConverter
    {
        public static string Convert(PhoneCallHistoryEntry entry)
        {
            if (entry.IsIncoming)
            {
                if (entry.IsCallerIdBlocked)
                {
                    return Glyphs.CALL_HISTORY_BLOCKED;
                }
                else
                {
                    return entry.IsMissed ? Glyphs.CALL_HISTORY_MISSED : Glyphs.CALL_HISTORY_INCOMING;
                }
            }
            else
            {
                return Glyphs.CALL_HISTORY_OUTGOING;
            }
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
