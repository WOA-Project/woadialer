using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml.Data;

namespace WoADialer.UI.Conventers
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
