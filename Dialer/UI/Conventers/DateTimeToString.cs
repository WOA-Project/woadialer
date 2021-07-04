using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Conventers
{
    public sealed class DateTimeToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
            {
                try
                {
                    return date.ToString("U", CultureInfo.CurrentCulture);
                }
                catch
                {
                    return string.Empty;
                }
            }
            if (value is DateTimeOffset dateOffset)
            {
                try
                {
                    return dateOffset.ToString("F", CultureInfo.CurrentCulture);
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
