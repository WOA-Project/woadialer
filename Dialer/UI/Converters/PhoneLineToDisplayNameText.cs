using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Converters
{
    public sealed class PhoneLineToDisplayNameText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PhoneLine line)
            {
                try
                {
                    return line.DisplayName;
                }
                catch
                {
                    try
                    {
                        return line.NetworkName;
                    }
                    catch
                    {
                        return string.Empty;
                    }
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
