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

namespace WoADialer
{
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
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;
            titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;

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
                //Task<Dictionary<Guid, PhoneLine>> getPhoneLinesTask = GetPhoneLinesAsync();
                //allPhoneLines = await getPhoneLinesTask;
                //noOfLines = allPhoneLines.Count;
                Task<PhoneLine> getDefaultLineTask = GetDefaultPhoneLineAsync();
                currentPhoneLine = await getDefaultLineTask;
                //updateCellularInformation();
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
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

        private async Task<PhoneLine> GetDefaultPhoneLineAsync()
        {
            PhoneCallStore phoneCallStore = await PhoneCallManager.RequestStoreAsync();
            // Added this so the user can give permissions on beforehand, and so the InCallUI will also display the info correctly the first time you dial
            PhoneCallHistoryStore a = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite);
            Guid lineId = await phoneCallStore.GetDefaultLineAsync();
            return await PhoneLine.FromIdAsync(lineId);
        }

        private void updateCellularInformation()
        {
            PhoneLine line = currentPhoneLine;
            PhoneLineCellularDetails cellularDetails = line.CellularDetails;
            currentSIMSlotIndex = cellularDetails.SimSlotIndex;
            currentDisplayName = line.DisplayName;
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

        public PhoneLine CurrentPhoneLine
        {
            get
            {
                return currentPhoneLine;
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

        private void ComposeNumber(object sender, RoutedEventArgs e)
        {
            numberToDialBox.Text += ((Button)sender).Content.ToString();
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

        private void DeleteLastNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (numberToDialBox.Text.Length >= 1)
            {
                numberToDialBox.Text = numberToDialBox.Text.Remove(numberToDialBox.Text.Length - 1);

            }
        }
    }
}
