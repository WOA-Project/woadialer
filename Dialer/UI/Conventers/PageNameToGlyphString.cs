using Dialer.Systems;
using System;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Conventers
{
    public sealed class PageNameToGlyphString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string name)
            {
                switch (name)
                {
                    case UISystem.CALL_HISTORY_PAGE:
                        return Glyphs.CALL_PAGE_HISTORY;
                    case UISystem.CALL_UI_PAGE:
                        return Glyphs.CALL_PAGE_INCALL;
                    case UISystem.CONTACTS_PAGE:
                        return Glyphs.CALL_PAGE_CONTACTS;
                    case UISystem.DIAL_PAGE:
                        return Glyphs.CALL_PAGE_DIAL;

                    case UISystem.APPLICATIONS_SETTINGS_PAGE:
                        return Glyphs.CALL_SETTINGS_APPS;
                    case UISystem.NOTIFICATIONS_SETTINGS_PAGE:
                        return Glyphs.CALL_SETTINGS_NOTIFICATIONS;
                    case UISystem.PERSONALIZATION_SETTINGS_PAGE:
                        return Glyphs.CALL_SETTINGS_PERSONALIZATION;
                    case UISystem.PHONE_LINES_SETTINGS_PAGE:
                        return Glyphs.CALL_SETTINGS_LINES;
                    case UISystem.SOUND_SETTINGS_PAGE:
                        return Glyphs.CALL_SETTINGS_SOUND;
                    case UISystem.ABOUT_SETTINGS_PAGE:
                        return Glyphs.CALL_SETTINGS_ABOUT;
                    default:
                        return string.Empty;
                }
            }
            else throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
