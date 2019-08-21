using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Haptics;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WoADialer.UI.Pages;

namespace WoADialer.Background
{
    public sealed class CallSystem
    {
        public CallManager CallManager { get; private set; }
        public PhoneCallHistoryStore CallHistoryStore { get; private set; }
        public PhoneCallStore CallStore { get; private set; }
        public ContactStore ContactStore { get; private set; }

        public CallSystem()
        {

        }

        private async Task SaveCallIntoHistory(Call call, CallStateChangedEventArgs args)
        {
            PhoneCallHistoryEntry historyEntry = new PhoneCallHistoryEntry()
            {
                IsIncoming = call.Direction == CallDirection.Incoming,
                IsMissed = args.OldState == CallState.Incoming,
                IsSeen = args.OldState != CallState.Incoming,
                OtherAppReadAccess = PhoneCallHistoryEntryOtherAppReadAccess.Full,
                StartTime = call.StartTime ?? DateTimeOffset.Now,
                Duration = call.EndTime - call.StartTime,
                Media = PhoneCallHistoryEntryMedia.Audio,
                IsCallerIdBlocked = false,
                IsEmergency = false,
                IsRinging = false,
                IsSuppressed = false,
                IsVoicemail = call.field_BF0.HasFlag(PH_CALL_INFO_field_BF0.VoicemailCall),
                RemoteId = call.Line?.Transport switch
                {
                    PhoneLineTransport.Cellular => call.Line.Id.ToString(),
                    PhoneLineTransport.VoipApp => call.OwningApplication.PackageFamilyName,
                    PhoneLineTransport.Bluetooth => call.Line.TransportDeviceId,
                    _ => "Unknown"
                },
                Address = new PhoneCallHistoryEntryAddress()
                {
                    ContactId = call.Contact?.Id ?? string.Empty,
                    DisplayName = call.Contact?.DisplayName ?? string.Empty,
                    RawAddress = call.Phone?.Number ?? "<null>",
                    RawAddressKind = call.Phone == null ? PhoneCallHistoryEntryRawAddressKind.Custom : PhoneCallHistoryEntryRawAddressKind.PhoneNumber,
                }
            };
            await CallHistoryStore.SaveEntryAsync(historyEntry);
        }

        private void CallManager_CallAppeared(CallManager sender, Call call)
        {
            switch (call.State)
            {
                case CallState.Disconnected:
                case CallState.Indeterminate:
                    break;
                case CallState.Dialing:
                case CallState.Incoming:
                    Call_StateChanged(call, new CallStateChangedEventArgs(call.State, call.State, call.StateReason));
                    goto default;
                default:
                    call.StateChanged += Call_StateChanged;
                    break;
            }
        }

        private async void Call_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            if (App.Current.IsForeground)
            {
                await App.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Frame frame = Window.Current.Content as Frame;
                    if (frame != null)
                    {
                        if (frame.SourcePageType == typeof(InCallUI) && CallManager.CallCounts.ActiveTalkingCalls == 0 && CallManager.CallCounts.ConferenceCalls == 0 && CallManager.CallCounts.DialingCalls == 0 && CallManager.CallCounts.IncomingCalls == 0 && CallManager.CallCounts.OnHoldCalls == 0 && CallManager.CallCounts.TransferingCalls == 0)
                        {
                            if (frame.CanGoBack)
                            {
                                frame.GoBack();
                            }
                            else
                            {
                                frame.BackStack.Clear();
                                frame.Navigate(typeof(MainPage));
                            }
                        }
                        else
                        {
                            switch (args.NewState)
                            {
                                case CallState.Dialing:
                                case CallState.Incoming:
                                    frame.Navigate(typeof(InCallUI));
                                    break;
                            }
                        }
                    }
                });
            }
            else
            {
                App.Current.NotificationSystem.RefreshCallNotification(CallManager.CurrentCalls);
            }
            switch (args.NewState)
            {
                case CallState.Disconnected:
                    sender.StateChanged -= Call_StateChanged;
                    await SaveCallIntoHistory(sender, args);
                    switch (args.OldState)
                    {
                        case CallState.Incoming:
                            App.Current.NotificationSystem.ToastNotifier.Show(App.Current.NotificationSystem.CreateMissedCallToastNotification(sender));
                            break;
                    }
                    break;
                case CallState.ActiveTalking:
                    SimpleHapticsControllerFeedback feedback = App.Current.DeviceSystem.VibrationDevice?.SimpleHapticsController.SupportedFeedback.First(x => x.Waveform == KnownSimpleHapticsControllerWaveforms.Release);
                    App.Current.DeviceSystem.VibrationDevice?.SimpleHapticsController.SendHapticFeedback(feedback);
                    break;
                case CallState.Dialing:
                case CallState.Incoming:

                    break;
                case CallState.Transfering:
                    
                    break;
            }
        }

        public async Task Initializate()
        {
            CallStore = await PhoneCallManager.RequestStoreAsync();
            CallHistoryStore = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite);
            ContactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);
            CallManager = await CallManager.GetCallManagerAsync();
            CallManager.CallAppeared += CallManager_CallAppeared;
        }
    }
}
