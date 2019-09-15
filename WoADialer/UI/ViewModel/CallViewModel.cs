#nullable enable

using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace WoADialer.UI.ViewModel
{
    public sealed class CallViewModel : ViewModelCore
    {
        public static string CallStateToFontIconString(CallState state, CallStateReason reason)
        {
            switch (state)
            {
                case CallState.ActiveTalking:
                    return "\uE717";
                case CallState.Dialing:
                    return "\uF715";
                case CallState.Disconnected:
                    switch (reason)
                    {
                        case CallStateReason.NetworkCongestion:
                            return "\uEC05";
                        case CallStateReason.CallBarred:
                            return "\uF619";
                        case CallStateReason.FDNRestricted:
                            return "\uF61A";
                        case CallStateReason.RoamRestricted:
                            return "\uE878";
                        case CallStateReason.ServiceOff:
                            return "\uF384";
                        case CallStateReason.Other:
                        case CallStateReason.Ended:
                        case CallStateReason.Dropped:
                        case CallStateReason.CallUpgradeInitiated:
                        case CallStateReason.Busy:
                        case CallStateReason.VideoCallingOff:
                            return "\uE778";
                        default:
                            return string.Empty;
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
                default:
                    return "";
            }
        }

        public static string CallStateToTextString(CallState state, CallStateReason reason)
        {
            switch (state)
            {
                case CallState.ActiveTalking:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.ActiveTalking));
                case CallState.Dialing:
                    return App.Current.ResourceLoader.GetString(nameof(CallState) + '_' + nameof(CallState.Dialing));
                case CallState.Disconnected:
                    switch (reason)
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

        private Timer? Timer;

        public Call Call { get; }
        public string StateFontIcon => CallStateToFontIconString(Call.State, Call.StateReason);
        public string StateText => CallStateToTextString(Call.State, Call.StateReason);
        public TimeSpan? Length => (Call.EndTime ?? DateTimeOffset.Now) - Call.StartTime;
        public string DisplayableLength => Length.HasValue ? (Length.Value.Hours == 0 ? Length.Value.ToString(@"mm\:ss", CultureInfo.InvariantCulture) : Length.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)) : "";

        public CallViewModel(CoreDispatcher dispatcher, Call call) : base(dispatcher)
        {
            Call = call;
            Call.StateChanged += Call_StateChanged;
            Call.StartTimeChanged += Call_StartTimeChanged;
            Call.EndTimeChanged += Call_EndTimeChanged;
            if (Call.StartTime != null && Call.EndTime == null)
            {
                InitializateTimer();
            }
        }

        private void Call_EndTimeChanged(Call sender, CallTimeChangedEventArgs args)
        {
            Timer?.Dispose();
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(DisplayableLength));
        }

        private void Call_StartTimeChanged(Call sender, CallTimeChangedEventArgs args)
        {
            if (Call.StartTime != null && Call.EndTime == null)
            {
                InitializateTimer();
            }
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(DisplayableLength));
        }

        private void Call_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            OnPropertyChanged(nameof(StateFontIcon));
            OnPropertyChanged(nameof(StateText));
        }

        private void InitializateTimer()
        {
            Timer = new Timer(Timer_Callback, null, new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 1));
        }

        private void Timer_Callback(object state)
        {
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(DisplayableLength));
        }
    }
}
