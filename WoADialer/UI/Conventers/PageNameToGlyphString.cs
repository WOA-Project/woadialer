using System;
using Windows.UI.Xaml.Data;
using WoADialer.Systems;

namespace WoADialer.UI.Conventers
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
                        return Glyphs.HISTORY;
                    case UISystem.CALL_UI_PAGE:
                        return Glyphs.PHONE;
                    case UISystem.CONTACTS_PAGE:
                        return Glyphs.PEOPLE;
                    case UISystem.DIAL_PAGE:
                        return Glyphs.DIALPAD;
                    case UISystem.APPLICATIONS_SETTINGS_PAGE:
                        return Glyphs.ALL_APPS;
                    case UISystem.NOTIFICATIONS_SETTINGS_PAGE:
                        return Glyphs.ACTION_CENTER;
                    case UISystem.PERSONALIZATION_SETTINGS_PAGE:
                        return Glyphs.PERSONALIZE;
                    case UISystem.PHONE_LINES_SETTINGS_PAGE:
                        return Glyphs.PHONE;
                    case UISystem.SOUND_SETTINGS_PAGE:
                        return Glyphs.VOLUME;
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
