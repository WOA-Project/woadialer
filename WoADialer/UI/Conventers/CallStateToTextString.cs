using Internal.Windows.Calls;
using System;
using Windows.UI.Xaml.Data;

namespace WoADialer.UI.Conventers
{
    public sealed class CallStateToTextString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CallState)
            {
                CallState state = (CallState)value;
                return state.ToString();
                //switch (state)
                //{
                //    case CallState.ActiveTalking:
                //        return "\uE717";
                //    case CallState.Dialing:
                //        return "\uF715";
                //    case CallState.Disconnected:
                //        return "\uE778";
                //    case CallState.Incoming:
                //        return "\uE77E";
                //    case CallState.OnHold:
                //        return "\uE769";
                //    case CallState.Transfering:
                //        return "\uE7F2";
                //    case CallState.Count:
                //        return "\uE80B";
                //    case CallState.Indeterminate:
                //    default:
                //        return "";
                //}
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
