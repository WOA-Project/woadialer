using Internal.Windows.Calls;
using System;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Converters
{
    public sealed class CallToCallStateTextString : IValueConverter
    {
        public static string Convert(Call call)
        {
            switch (call.State)
            {
                case CallState.ActiveTalking:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.ActiveTalking));
                case CallState.Dialing:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Dialing));
                case CallState.Disconnected:
                    switch (call.StateReason)
                    {
                        case CallStateReason.Busy:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Busy));
                        case CallStateReason.CallBarred:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.CallBarred));
                        case CallStateReason.CallUpgradeInitiated:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.CallUpgradeInitiated));
                        case CallStateReason.Dropped:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Dropped));
                        case CallStateReason.Ended:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Ended));
                        case CallStateReason.FDNRestricted:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.FDNRestricted));
                        case CallStateReason.NetworkCongestion:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.NetworkCongestion));
                        case CallStateReason.RoamRestricted:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.RoamRestricted));
                        case CallStateReason.ServiceOff:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.ServiceOff));
                        case CallStateReason.VideoCallingOff:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Busy));
                        case CallStateReason.Other:
                        default:
                            return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Disconnected));
                    }
                case CallState.Incoming:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Incoming));
                case CallState.Indeterminate:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Indeterminate));
                case CallState.OnHold:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.OnHold));
                case CallState.Transferring:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Transferring));
                case CallState.Count:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Count));
                default:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + "_Unknown");
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case Call call:
                    return Convert(call);
                case null:
                default:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + "_Unknown");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
