using Internal.Windows.Calls;
using System;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Converters
{
    public sealed class CallToCallStateGlyphString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case Call call:
                    switch (call.State)
                    {
                        case CallState.ActiveTalking:
                            return Glyphs.CALL_INCALL_TALKING;
                        case CallState.Dialing:
                            return Glyphs.CALL_INCALL_DIALING;
                        case CallState.Disconnected:
                            switch (call.StateReason)
                            {
                                case CallStateReason.NetworkCongestion:
                                    return Glyphs.NETWORK_TOWER;
                                case CallStateReason.CallBarred:
                                    return Glyphs.SIM_MISSED;
                                case CallStateReason.FDNRestricted:
                                    return Glyphs.SIM_LOCK;
                                case CallStateReason.RoamRestricted:
                                    return Glyphs.ROAMING_INTERNATIONAL;
                                case CallStateReason.ServiceOff:
                                    return Glyphs.NETWORK_OFFLINE;
                                case CallStateReason.Other:
                                case CallStateReason.Ended:
                                    return Glyphs.CALL_ENDED;
                                case CallStateReason.Dropped:
                                case CallStateReason.CallUpgradeInitiated:
                                case CallStateReason.Busy:
                                case CallStateReason.VideoCallingOff:
                                    return Glyphs.HANG_UP;
                                default:
                                    return string.Empty;
                            }
                        case CallState.Incoming:
                            return Glyphs.CALL_INCALL_INCOMING;
                        case CallState.OnHold:
                            return Glyphs.CALL_INCALL_ONHOLD;
                        case CallState.Transferring:
                            return Glyphs.CALL_INCALL_TRANSFERRING;
                        case CallState.Count:
                            return Glyphs.CALL_INCALL_COUNT;
                        case CallState.Indeterminate:
                            return Glyphs.CALL_INCALL_INDETERMINATE;
                        default:
                            return string.Empty;
                    }
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
