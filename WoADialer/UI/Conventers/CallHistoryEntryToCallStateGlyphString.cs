using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml.Data;

namespace WoADialer.UI.Conventers
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
                else if (entry.IsMissed)
                {
                    return Glyphs.CALL_HISTORY_MISSED;
                }
                else
                {
                    return Glyphs.CALL_HISTORY_INCOMING;
                }
            }
            else
            {
                return Glyphs.CALL_HISTORY_OUTGOING;
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
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
