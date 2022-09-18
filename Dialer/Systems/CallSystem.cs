using Internal.Windows.Calls;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.Haptics;
using Windows.Networking.NetworkOperators;
using Windows.System;

namespace Dialer.Systems
{
    public class DisplayableLine : INotifyPropertyChanged
    {
        public PhoneLine Line { get; }
        public string DisplayName { get; private set; }
        public string Glyph { get; }
        public MobileBroadbandModem Modem { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public async static Task<MobileBroadbandModem> LoadMobileBroadbandModemAsync(PhoneLine line)
        {
            MobileBroadbandModem modem = null;

            if (line.CellularDetails != null)
            {
                try
                {
                    string selectorStr = MobileBroadbandModem.GetDeviceSelector();
                    DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selectorStr);

                    if (devices.Count > line.CellularDetails.SimSlotIndex)
                    {
                        DeviceInformation mdevice = devices[line.CellularDetails.SimSlotIndex];
                        modem = MobileBroadbandModem.FromId(mdevice.Id);
                    }
                }
                catch
                {
                }
            }

            return modem;
        }

        public static string LoadExtendedDisplayNameInformation(PhoneLine line, MobileBroadbandModem modem)
        {
            string displayName = line == null ? "Unknown" : (string.IsNullOrWhiteSpace(line.DisplayName) ? line.NetworkName : line.DisplayName);

            if (string.IsNullOrWhiteSpace(displayName) && line.CellularDetails != null && modem != null)
            {
                displayName = modem.CurrentNetwork.RegisteredProviderName;
                displayName += " (SIM " + (line.CellularDetails.SimSlotIndex + 1) + ")";
            }

            if (line != null && line.CellularDetails != null && string.IsNullOrWhiteSpace(displayName))
            {
                foreach (PhoneLineNetworkOperatorDisplayTextLocation location in Enum.GetValues(typeof(PhoneLineNetworkOperatorDisplayTextLocation)))
                {
                    displayName = line.CellularDetails.GetNetworkOperatorDisplayText(location);
                    if (!string.IsNullOrWhiteSpace(displayName))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "Unknown";
            }

            return displayName;
        }

        public DisplayableLine(PhoneLine line)
        {
            Line = line;

            DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            Task.Run(async () =>
            {
                MobileBroadbandModem newModem = await LoadMobileBroadbandModemAsync(line);
                await dispatcherQueue.EnqueueAsync(() =>
                {
                    Modem = newModem;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Modem)));

                    DisplayName = LoadExtendedDisplayNameInformation(line, newModem);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                });
            });

            if (line != null)
            {
                switch (line.NetworkState)
                {
                    //
                    // Summary:
                    //     The registration status of the phone line is unknown.
                    case PhoneNetworkState.Unknown:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     Could not detect a signal on the phone line, or the phone line is limited to
                    //     emergency calls only.
                    case PhoneNetworkState.NoSignal:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     The phone line has been de-registered.
                    case PhoneNetworkState.Deregistered:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     Could not register the phone line with any available network.
                    case PhoneNetworkState.Denied:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     Searching for a network for the phone line.
                    case PhoneNetworkState.Searching:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     The phone line is registered and is on the carrier's home network.
                    case PhoneNetworkState.Home:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     The phone line is registered and is roaming internationally on another carrier's
                    //     network.
                    case PhoneNetworkState.RoamingInternational:
                        {
                            break;
                        }
                    //
                    // Summary:
                    //     The phone line is registered and is roaming domestically on another carrier's
                    //     network.
                    case PhoneNetworkState.RoamingDomestic:
                        {
                            break;
                        }
                }
            }
        }
    }

    public sealed class CallSystem
    {
        private readonly ObservableCollection<PhoneCallHistoryEntry> _CallHistoryEntries = new();
        private PhoneLineWatcher LineWatcher;
        private readonly CoreApplicationView CoreApplicationView;

        public CallManager CallManager { get; private set; }
        public PhoneCallHistoryStore CallHistoryStore { get; private set; }
        public PhoneCallStore CallStore { get; private set; }
        public ContactStore ContactStore { get; private set; }
        public PhoneLine DefaultLine { get; private set; }

        public ReadOnlyObservableCollection<PhoneCallHistoryEntry> CallHistoryEntries { get; }
        public readonly ObservableCollection<PhoneLine> Lines = new();
        public readonly ObservableCollection<DisplayableLine> DisplayableLines = new();

        public CallSystem()
        {
            CallHistoryEntries = new ReadOnlyObservableCollection<PhoneCallHistoryEntry>(_CallHistoryEntries);
            CoreApplicationView = CoreApplication.GetCurrentView();
            Lines.CollectionChanged += Lines_CollectionChanged;
        }

        private async Task SaveCallIntoHistory(Call call, CallStateChangedEventArgs args)
        {
            bool UseAlternativeField = (DateTimeOffset.Now - call.CallArrivalTime)?.TotalSeconds >= 1;

            PhoneCallHistoryEntry historyEntry = new()
            {
                IsIncoming = call.Direction == CallDirection.Incoming,
                IsMissed = args.OldState == CallState.Incoming,
                IsSeen = args.OldState != CallState.Incoming,
                OtherAppReadAccess = PhoneCallHistoryEntryOtherAppReadAccess.SystemOnly,
                StartTime = (UseAlternativeField ? call.CallArrivalTime : call.StartTime) ?? DateTimeOffset.Now,
                Duration = UseAlternativeField ? call.EndTime - call.CallArrivalTime : call.EndTime - call.StartTime,
                Media = PhoneCallHistoryEntryMedia.Audio,
                IsCallerIdBlocked = false,
                IsEmergency = false,
                IsRinging = false,
                IsSuppressed = false,
                IsVoicemail = call.field_BF0.HasFlag(CallFlags.VoicemailCall),
                RemoteId = call.Line?.Transport + "|" + call.Line?.Transport switch
                {
                    PhoneLineTransport.Cellular => call.Line.Id.ToString(),
                    PhoneLineTransport.VoipApp => call.OwningApplication.PackageFamilyName,
                    PhoneLineTransport.Bluetooth => call.Line.Id.ToString(),
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
            UpdateCallHistoryEntries();
        }

        private async void UpdateCallHistoryEntries()
        {
            IReadOnlyList<PhoneCallHistoryEntry> entries = await CallHistoryStore.GetEntryReader().ReadBatchAsync();
            List<PhoneCallHistoryEntry> @new = entries.Except(_CallHistoryEntries).ToList();
            List<PhoneCallHistoryEntry> removed = _CallHistoryEntries.Except(entries).ToList();
            foreach (PhoneCallHistoryEntry entry in @removed)
            {
                await CoreApplicationView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => _CallHistoryEntries.Remove(entry));
            }
            foreach (PhoneCallHistoryEntry entry in @new)
            {
                await CoreApplicationView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => _CallHistoryEntries.Add(entry));
            }
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
            switch (args.NewState)
            {
                case CallState.Disconnected:
                    sender.StateChanged -= Call_StateChanged;
                    await SaveCallIntoHistory(sender, args);
                    switch (args.OldState)
                    {
                        case CallState.Incoming:
                            App.Current.NotificationSystem.ToastNotifier?.Show(App.Current.NotificationSystem.CreateMissedCallToastNotification(sender));
                            break;
                    }
                    break;
                case CallState.ActiveTalking:
                    if (App.Current.PermissionSystem.Vibration == VibrationAccessStatus.Allowed && App.Current.DeviceSystem.VibrationDevice != null)
                    {
                        SimpleHapticsControllerFeedback feedback = App.Current.DeviceSystem.VibrationDevice.SimpleHapticsController.SupportedFeedback[0];
                        App.Current.DeviceSystem.VibrationDevice.SimpleHapticsController.SendHapticFeedback(feedback);
                    }
                    break;
                case CallState.Dialing:

                    break;
                case CallState.Incoming:
                    App.Current.NotificationSystem.RefreshCallNotification(CallManager.CurrentCalls);
                    break;
                case CallState.Transferring:

                    break;
            }
        }

        public async Task Initialize()
        {
            try
            {
                CallStore = await PhoneCallManager.RequestStoreAsync();

                try
                {
                    Guid phoneLine = await CallStore.GetDefaultLineAsync();
                    DefaultLine = await PhoneLine.FromIdAsync(phoneLine);
                }
                catch
                {
                }

                LineWatcher = CallStore.RequestLineWatcher();
                LineWatcher.LineAdded += LineWatcher_LineAdded;
                LineWatcher.LineRemoved += LineWatcher_LineRemoved;
                LineWatcher.LineUpdated += LineWatcher_LineUpdated;
                LineWatcher.Start();

                try
                {
                    CallManager = await CallManager.GetCallManagerAsync();
                    CallManager.CallAppeared += CallManager_CallAppeared;
                }
                catch
                {
                }
            }
            catch
            {
            }

            try
            {
                CallHistoryStore = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite);
                UpdateCallHistoryEntries();
            }
            catch
            {
            }

            try
            {
                ContactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);
            }
            catch
            {
            }
        }

        private async void LineWatcher_LineUpdated(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            Debug.WriteLine("Updating " + args.LineId);
            int index = Lines.IndexOf(Lines.First(x => x.Id == args.LineId));
            if (index != -1)
            {
                PhoneLine line = await PhoneLine.FromIdAsync(args.LineId);
                if (line != null)
                {
                    try
                    {
                        Lines[index] = line;
                    }
                    catch (InvalidOperationException) { }
                }
            }
        }

        private void LineWatcher_LineRemoved(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            Debug.WriteLine("Removing " + args.LineId);
            Lines.Remove(Lines.First(x => x.Id == args.LineId));
        }

        private async void LineWatcher_LineAdded(PhoneLineWatcher sender, PhoneLineWatcherEventArgs args)
        {
            Debug.WriteLine("Adding " + args.LineId);
            PhoneLine line = await PhoneLine.FromIdAsync(args.LineId);
            if (line != null)
            {
                Lines.Add(line);
            }
        }

        private async void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplicationView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        {
                            DisplayableLine itemToAdd = new(e.NewItems[0] as PhoneLine);
                            DisplayableLines.Add(itemToAdd);
                            break;
                        }
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        {
                            DisplayableLine itemToRemove = DisplayableLines.First(x => x.Line.Id == (e.OldItems[0] as PhoneLine)?.Id);
                            DisplayableLines.Remove(itemToRemove);
                            break;
                        }
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        {
                            DisplayableLine itemToReplace = DisplayableLines.First(x => x.Line.Id == (e.OldItems[0] as PhoneLine)?.Id);
                            DisplayableLine itemToAdd = new(e.NewItems[0] as PhoneLine);
                            DisplayableLines[DisplayableLines.IndexOf(itemToReplace)] = itemToAdd;
                            break;
                        }
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        {
                            DisplayableLines.Move(e.OldStartingIndex, e.NewStartingIndex);
                            break;
                        }
                }
            });
        }
    }
}
