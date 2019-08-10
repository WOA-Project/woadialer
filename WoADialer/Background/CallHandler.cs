using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;

namespace WoADialer.Background
{
    public sealed class CallHandler
    {
        public CallHandler()
        {

        }

        private void CallManager_CallAppeared(CallManager sender, Call call)
        {
            call.StateChanged += Call_StateChanged;
        }

        private async void Call_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            switch(args.NewState)
            {
                case CallState.Disconnected:
                    switch (args.OldState)
                    {
                        case CallState.Incoming:
                            App.Current.ToastNotifier.Show(App.Current.CreateMissedCallToastNotification(sender));
                            PhoneCallHistoryEntry missedCall = new PhoneCallHistoryEntry()
                            {
                                Address = new PhoneCallHistoryEntryAddress()
                                {
                                    DisplayName = sender.Name,
                                    RawAddress = sender.Number,
                                    RawAddressKind = PhoneCallHistoryEntryRawAddressKind.PhoneNumber
                                },
                                IsIncoming = true,
                                IsMissed = true,
                                Media = PhoneCallHistoryEntryMedia.Audio,
                                OtherAppReadAccess = PhoneCallHistoryEntryOtherAppReadAccess.Full,
                                //SourceId = sender.Line.Id.ToString(),
                                //SourceIdKind = PhoneCallHistorySourceIdKind.CellularPhoneLineId,
                                StartTime = sender.EndTime ?? DateTimeOffset.Now,
                            };
                            await App.Current.CallHistoryStore.SaveEntryAsync(missedCall);
                            goto default;
                        case CallState.Dialing:
                        case CallState.OnHold:
                        case CallState.ActiveTalking:
                            PhoneCallHistoryEntry call = new PhoneCallHistoryEntry()
                            {
                                Address = new PhoneCallHistoryEntryAddress()
                                {
                                    DisplayName = sender.Name,
                                    RawAddress = sender.Number,
                                    RawAddressKind = PhoneCallHistoryEntryRawAddressKind.PhoneNumber
                                },
                                IsSeen = true,
                                IsIncoming = sender.Direction == CallDirection.Incoming,
                                Duration = sender.EndTime - sender.StartTime,
                                Media = PhoneCallHistoryEntryMedia.Audio,
                                OtherAppReadAccess = PhoneCallHistoryEntryOtherAppReadAccess.Full,
                                //SourceId = sender.Line.Id.ToString(),
                                //SourceIdKind = PhoneCallHistorySourceIdKind.CellularPhoneLineId,
                                StartTime = sender.StartTime ?? DateTimeOffset.Now,
                            };
                            await App.Current.CallHistoryStore.SaveEntryAsync(call);
                            goto default;
                        default:
                            switch (args.StateReason)
                            {
                                default:
                                    sender.StateChanged -= Call_StateChanged;
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        public void Start()
        {
            App.Current.CallManager.CallAppeared += CallManager_CallAppeared;
        }

        public void Stop()
        {
            App.Current.CallManager.CallAppeared -= CallManager_CallAppeared;
        }
    }
}
