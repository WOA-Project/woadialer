using Internal.Windows.Calls;
using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml.Data;
using WoADialer.UI.ViewModel;

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
                    return Glyphs.SHIELD;
                }
                else if (entry.IsMissed)
                {
                    return Glyphs.HANG_UP;
                }
                else
                {
                    return Glyphs.INCOMING_CALL;
                }
            }
            else
            {
                return Glyphs.PHONE;
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
