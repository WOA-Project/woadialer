using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoADialer.Pages;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace WoADialer
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public delegate void CallingInfoDelegate();
        public event CallingInfoDelegate CellInfoUpdateCompleted;
        public event CallingInfoDelegate ActivePhoneCallStateChanged;

        private int noOfLines;
        private Dictionary<Guid, PhoneLine> allPhoneLines;
        private PhoneLine currentPhoneLine;
        private int currentSIMSlotIndex;
        private string currentSIMState;
        private string currentNetworkState;
        private string currentDisplayName;
        private string currentOperatorName;
        private Windows.UI.Color currentDisplayColor;
        private string currentVoicemailNumber;
        private int currentVoicemailCount;
        private bool doesPhoneCallExist;

        public MainPage()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //titleBar.ButtonBackgroundColor = Colors.Transparent;
            //titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (PhoneCallManager.IsCallActive) callStateIndicatorText.Text = "Status: Call Active";
            else if (PhoneCallManager.IsCallIncoming) callStateIndicatorText.Text = "Status: Call Incoming";
            else callStateIndicatorText.Text = "Status: Phone Idle";

            this.MonitorCallState();

            

            start();
        }

        private async void start()
        {
            try
            {
                Task<Dictionary<Guid, PhoneLine>> getPhoneLinesTask = GetPhoneLinesAsync();
                allPhoneLines = await getPhoneLinesTask;
                noOfLines = allPhoneLines.Count;
                Task<PhoneLine> getDefaultLineTask = GetDefaultPhoneLineAsync();
                currentPhoneLine = await getDefaultLineTask;
                updateCellularInformation();
            } catch (Exception ex)
            {
                var messageDialog = new MessageDialog(ex.Message);

                messageDialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(this.CommandInvokedHandler)));

                messageDialog.DefaultCommandIndex = 0;
                await messageDialog.ShowAsync();
            }
        }

        private void MonitorCallState()
        {
            PhoneCallManager.CallStateChanged += async (o, args) =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                     {
                         if (PhoneCallManager.IsCallActive) callStateIndicatorText.Text = "Status: Call Active";
                         else if (PhoneCallManager.IsCallIncoming) callStateIndicatorText.Text = "Status: Call Incoming";
                         else callStateIndicatorText.Text = "Status: Phone Idle";

                         if(PhoneCallManager.IsCallActive)
                         {
                             
                             Frame.Navigate(typeof(InCallUI));
                         }
                     }
                );

                doesPhoneCallExist = PhoneCallManager.IsCallActive || PhoneCallManager.IsCallIncoming;
                if (ActivePhoneCallStateChanged != null)
                {
                    ActivePhoneCallStateChanged();
                }
            };
        }

        private async Task<Dictionary<Guid, PhoneLine>> GetPhoneLinesAsync()
        {
            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
            var watcher = store.RequestLineWatcher();
            var phoneLines = new List<PhoneLine>();
            var lineEnumerationCompletion = new TaskCompletionSource<bool>();
            watcher.LineAdded += async (o, args) => { var line = await PhoneLine.FromIdAsync(args.LineId); phoneLines.Add(line); };
            watcher.Stopped += (o, args) => lineEnumerationCompletion.TrySetResult(false);
            watcher.EnumerationCompleted += (o, args) => lineEnumerationCompletion.TrySetResult(true);
            watcher.Start();
            if (!await lineEnumerationCompletion.Task)
            {
                throw new Exception("Phone Line Enumeration failed");
            }

            watcher.Stop();

            Dictionary<Guid, PhoneLine> returnedLines = new Dictionary<Guid, PhoneLine>();

            foreach (PhoneLine phoneLine in phoneLines)
            {
                if (phoneLine != null && phoneLine.Transport == PhoneLineTransport.Cellular)
                {
                    returnedLines.Add(phoneLine.Id, phoneLine);
                }
            }

            return returnedLines;
        }

        private async Task<PhoneLine> GetDefaultPhoneLineAsync()
        {
            PhoneCallStore phoneCallStore = await PhoneCallManager.RequestStoreAsync();
            Guid lineId = await phoneCallStore.GetDefaultLineAsync();
            return await PhoneLine.FromIdAsync(lineId);
        }

        private void updateCellularInformation()
        {
            PhoneLine line = currentPhoneLine;
            PhoneLineCellularDetails cellularDetails = line.CellularDetails;

            currentSIMSlotIndex = cellularDetails.SimSlotIndex;

            currentDisplayName = line.DisplayName;

            //currentDisplayColor = line.DisplayColor;

            currentVoicemailNumber = line.Voicemail.Number;

            currentVoicemailCount = line.Voicemail.MessageCount;

            currentOperatorName = "N/A";

            PhoneSimState simState = cellularDetails.SimState;
            switch (simState)
            {
                case PhoneSimState.Unknown:
                    currentSIMState = "Unknown";
                    break;
                case PhoneSimState.PinNotRequired:
                    currentSIMState = "Pin Not Required";
                    break;
                case PhoneSimState.PinUnlocked:
                    currentSIMState = "Pin Unlocked";
                    break;
                case PhoneSimState.PinLocked:
                    currentSIMState = "Pin Locked";
                    break;
                case PhoneSimState.PukLocked:
                    currentSIMState = "Puk Locked";
                    break;
                case PhoneSimState.NotInserted:
                    currentSIMState = "No SIM";
                    break;
                case PhoneSimState.Invalid:
                    currentSIMState = "Invalid";
                    break;
                case PhoneSimState.Disabled:
                    currentSIMState = "Disabled";
                    break;
                default:
                    currentSIMState = "Unknown";
                    break;
            }

            PhoneNetworkState networkState = line.NetworkState;
            switch (line.NetworkState)
            {
                case PhoneNetworkState.NoSignal:
                    if ((bool)line.LineConfiguration.ExtendedProperties["ShouldDisplayEmergencyCallState"])
                    {
                        currentNetworkState = "Emergency calls only";
                        break;
                    }
                    else
                    {
                        currentNetworkState = "No Service";
                        break;
                    }

                case PhoneNetworkState.Deregistered:
                    currentNetworkState = "Deregistered";
                    break;
                case PhoneNetworkState.Denied:
                    currentNetworkState = "Denied";
                    break;
                case PhoneNetworkState.Searching:
                    currentNetworkState = "Searching";
                    break;
                case PhoneNetworkState.Home:
                    currentNetworkState = "Connected";
                    currentOperatorName = line.NetworkName;
                    break;
                case PhoneNetworkState.RoamingInternational:
                    currentNetworkState = "Roaming International";
                    currentOperatorName = line.NetworkName;
                    break;
                case PhoneNetworkState.RoamingDomestic:
                    currentNetworkState = "Roaming Domestic";
                    currentOperatorName = line.NetworkName;
                    break;
                default:
                    currentNetworkState = "Unknown";
                    break;
            }

            if (CellInfoUpdateCompleted != null)
            {
                CellInfoUpdateCompleted();
            }
        }

        public int NoOfLines
        {
            get
            {
                return noOfLines;
            }
        }

        public Dictionary<Guid, PhoneLine> AllPhoneLines
        {
            get
            {
                if (allPhoneLines == null)
                {
                    allPhoneLines = new Dictionary<Guid, PhoneLine>();
                }
                return allPhoneLines;
            }
        }

        public PhoneLine CurrentPhoneLine
        {
            get
            {
                return currentPhoneLine;
            }
        }

        public int CurrentSIMSlotIndex
        {
            get
            {
                return currentSIMSlotIndex;
            }
        }

        public string CurrentSIMState
        {
            get
            {
                if (currentSIMState == null)
                {
                    currentSIMState = "Loading...";
                }
                return currentSIMState;
            }
        }

        public string CurrentNetworkState
        {
            get
            {
                if (currentNetworkState == null)
                {
                    currentNetworkState = "Loading...";
                }
                return currentNetworkState;
            }
        }

        public string CurrentDisplayName
        {
            get
            {
                if (currentDisplayName == null)
                {
                    currentDisplayName = "Loading...";
                }
                return currentDisplayName;
            }
        }

        public string CurrentOperatorName
        {
            get
            {
                if (currentOperatorName == null)
                {
                    currentOperatorName = "Loading...";
                }
                return currentOperatorName;
            }
        }

        public Windows.UI.Color CurrentDisplayColor
        {
            get
            {
                if (currentDisplayColor == null)
                {
                    currentDisplayColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                }
                return currentDisplayColor;
            }
        }

        public string CurrentVoicemailNumber
        {
            get
            {
                if (currentVoicemailNumber == null)
                {
                    currentVoicemailNumber = "Loading...";
                }
                return currentVoicemailNumber;
            }
        }

        public int CurrentVoicemailCount
        {
            get
            {
                return currentVoicemailCount;
            }
        }

        public bool DoesPhoneCallExist
        {
            get
            {
                return doesPhoneCallExist;
            }
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string numberToDial = numberToDialBox.Text.Replace(" ", "");
                numberToDial = numberToDial.Replace("+", "00");
                if (numberToDial != "")
                {
                    currentPhoneLine.Dial(numberToDial, "test");
                    Frame.Navigate(typeof(InCallUI), numberToDial);
                }
            } catch (Exception ee)
            {
                handleBug(ee);
            }
        }

        public async void handleBug(Exception e)
        {
            var messageDialog = new MessageDialog(e.Message + "\n\n\n" + e.StackTrace);

            messageDialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(this.CommandInvokedHandler)));

            messageDialog.DefaultCommandIndex = 0;
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            //CoreApplication.Exit();
        }
    }
}
