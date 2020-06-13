using System;
using Windows.UI.Xaml.Data;

namespace WoADialer.UI.Conventers
{
    public sealed class PageNameToTextString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string name)
            {
                return App.Current.ResourceLoader.GetString(name);
            }
            else throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
