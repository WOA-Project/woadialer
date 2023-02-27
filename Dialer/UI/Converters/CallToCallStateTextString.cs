using Internal.Windows.Calls;
using Microsoft.UI.Xaml.Data;
using System;

namespace Dialer.UI.Converters
{
    public sealed class CallToCallStateTextString : IValueConverter
    {
        public static string Convert(Call call)
        {
            return call.State switch
            {
                CallState.ActiveTalking => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.ActiveTalking)),
                CallState.Dialing => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Dialing)),
                CallState.Disconnected => call.StateReason switch
                {
                    CallStateReason.Busy => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Busy)),
                    CallStateReason.CallBarred => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.CallBarred)),
                    CallStateReason.CallUpgradeInitiated => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.CallUpgradeInitiated)),
                    CallStateReason.Dropped => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Dropped)),
                    CallStateReason.Ended => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Ended)),
                    CallStateReason.FDNRestricted => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.FDNRestricted)),
                    CallStateReason.NetworkCongestion => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.NetworkCongestion)),
                    CallStateReason.RoamRestricted => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.RoamRestricted)),
                    CallStateReason.ServiceOff => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.ServiceOff)),
                    CallStateReason.VideoCallingOff => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallStateReason.Busy)),
                    _ => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Disconnected)),
                },
                CallState.Incoming => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Incoming)),
                CallState.Indeterminate => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Indeterminate)),
                CallState.OnHold => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.OnHold)),
                CallState.Transferring => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Transferring)),
                CallState.Count => App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Count)),
                _ => App.Current.ResourceLoader.GetString(nameof(CallState) + "_Unknown"),
            };
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                Call call => Convert(call),
                _ => App.Current.ResourceLoader.GetString(nameof(CallState) + "_Unknown"),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
