using Dialer.Systems;
using Microsoft.UI.Xaml.Data;
using System;

namespace Dialer.UI.Converters
{
    public sealed class PageNameToGlyphString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is string name
                ? (object)(name switch
                {
                    UISystem.CALL_HISTORY_PAGE => Glyphs.CALL_PAGE_HISTORY,
                    UISystem.CALL_UI_PAGE => Glyphs.CALL_PAGE_INCALL,
                    UISystem.CONTACTS_PAGE => Glyphs.CALL_PAGE_CONTACTS,
                    UISystem.DIAL_PAGE => Glyphs.CALL_PAGE_DIAL,
                    UISystem.APPLICATIONS_SETTINGS_PAGE => Glyphs.CALL_SETTINGS_APPS,
                    UISystem.NOTIFICATIONS_SETTINGS_PAGE => Glyphs.CALL_SETTINGS_NOTIFICATIONS,
                    UISystem.PERSONALIZATION_SETTINGS_PAGE => Glyphs.CALL_SETTINGS_PERSONALIZATION,
                    UISystem.PHONE_LINES_SETTINGS_PAGE => Glyphs.CALL_SETTINGS_LINES,
                    UISystem.SOUND_SETTINGS_PAGE => Glyphs.CALL_SETTINGS_SOUND,
                    UISystem.ABOUT_SETTINGS_PAGE => Glyphs.CALL_SETTINGS_ABOUT,
                    _ => string.Empty,
                })
                : throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
