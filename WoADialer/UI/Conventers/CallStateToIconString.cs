using Internal.Windows.Calls;
using System;
using Windows.UI.Xaml.Data;
using WoADialer.UI.ViewModel;

namespace WoADialer.UI.Conventers
{
    public sealed class CallStateToIconString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case CallState.ActiveTalking:
                    return "\uE717";
                case CallState.Dialing:
                    return "\uF715";
                case CallState.Disconnected:
                    switch (parameter)
                    {
                        case CallStateReason.Busy:
                        case CallStateReason.CallBarred:
                        case CallStateReason.CallUpgradeInitiated:
                        case CallStateReason.Dropped:
                        case CallStateReason.Ended:
                        case CallStateReason.FDNRestricted:
                        case CallStateReason.NetworkCongestion:
                        case CallStateReason.Other:
                        case CallStateReason.RoamRestricted:
                        case CallStateReason.ServiceOff:
                        case CallStateReason.VideoCallingOff:
                            return "\uE778";
                        default:
                            return "";
                    }
                case CallState.Incoming:
                    return "\uE77E";
                case CallState.OnHold:
                    return "\uE769";
                case CallState.Transferring:
                    return "\uE7F2";
                case CallState.Count:
                    return "\uE80B";
                case CallState.Indeterminate:
                    return "?";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
