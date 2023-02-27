using Internal.Windows.Calls;
using Microsoft.UI.Xaml.Data;
using System;

namespace Dialer.UI.Converters
{
    public sealed class CallToCallStateGlyphString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                Call call => call.State switch
                {
                    CallState.ActiveTalking => Glyphs.CALL_INCALL_TALKING,
                    CallState.Dialing => Glyphs.CALL_INCALL_DIALING,
                    CallState.Disconnected => call.StateReason switch
                    {
                        CallStateReason.NetworkCongestion => Glyphs.NETWORK_TOWER,
                        CallStateReason.CallBarred => Glyphs.SIM_MISSED,
                        CallStateReason.FDNRestricted => Glyphs.SIM_LOCK,
                        CallStateReason.RoamRestricted => Glyphs.ROAMING_INTERNATIONAL,
                        CallStateReason.ServiceOff => Glyphs.NETWORK_OFFLINE,
                        CallStateReason.Other or CallStateReason.Ended => Glyphs.CALL_ENDED,
                        CallStateReason.Dropped or CallStateReason.CallUpgradeInitiated or CallStateReason.Busy or CallStateReason.VideoCallingOff => Glyphs.HANG_UP,
                        _ => string.Empty,
                    },
                    CallState.Incoming => Glyphs.CALL_INCALL_INCOMING,
                    CallState.OnHold => Glyphs.CALL_INCALL_ONHOLD,
                    CallState.Transferring => Glyphs.CALL_INCALL_TRANSFERRING,
                    CallState.Count => Glyphs.CALL_INCALL_COUNT,
                    CallState.Indeterminate => Glyphs.CALL_INCALL_INDETERMINATE,
                    _ => string.Empty,
                },
                _ => string.Empty,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
