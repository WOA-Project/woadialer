using Internal.Windows.Calls;
using System;
using Windows.UI.Xaml.Data;

namespace WoADialer.UI.Conventers
{
    public sealed class CallToCallLengthTextString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case Call call:
                    string length = "";
                    /*if ((DateTimeOffset.Now - call.Field_BB4)?.TotalSeconds >= 1)
                    {
                        length = (DateTimeOffset.Now - call.Field_BB4)?.ToString("mm\\:ss") ?? string.Empty;
                    }
                    else*/
                    {
                        length = (DateTimeOffset.Now - call.StartTime)?.ToString("mm\\:ss") ?? string.Empty;
                    }
                    return length;
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
