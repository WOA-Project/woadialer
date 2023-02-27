using Microsoft.UI.Xaml.Data;
using System;

namespace Dialer.UI.Converters
{
    public sealed class PageNameToTextString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is string name ? App.Current.ResourceLoader.GetString(name) : throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
