using Internal.Windows.Calls;
using Microsoft.UI.Xaml.Data;
using System;

namespace Dialer.UI.Converters
{
    public sealed class CallToCallLengthTextString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case Call call:
                    string length = (DateTimeOffset.Now - call.CallArrivalTime)?.TotalSeconds >= 1
    ? (DateTimeOffset.Now - call.CallArrivalTime)?.ToString("mm\\:ss") ?? string.Empty
    : (DateTimeOffset.Now - call.StartTime)?.ToString("mm\\:ss") ?? string.Empty;
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
