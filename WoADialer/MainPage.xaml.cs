using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoADialer.Numbers;
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
        private PhoneNumber currentNumber;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            switch (e.Parameter)
            {
                case string number:
                    currentNumber = PhoneNumber.Parse(number);
                    UpdateCurrentNumber();
                    break;
            }
        }

        private void UpdateCurrentNumber()
        {
            callButton.IsEnabled = !string.IsNullOrWhiteSpace(currentNumber.ToString());
            numberToDialBox.Text = currentNumber.ToString("nice");
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
            }
            catch (Exception ex)
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

                         if (PhoneCallManager.IsCallActive)
                         {
                             Frame.Navigate(typeof(InCallUI));
                         }
                     }
                );

                doesPhoneCallExist = PhoneCallManager.IsCallActive || PhoneCallManager.IsCallIncoming;
                ActivePhoneCallStateChanged?.Invoke();
            };
        }

        private async Task<PhoneLine> GetDefaultPhoneLineAsync()
        {
            PhoneCallStore phoneCallStore = await PhoneCallManager.RequestStoreAsync();
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

            CellInfoUpdateCompleted?.Invoke();
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
                currentPhoneLine.Dial(currentNumber.ToString(), "test");
                Frame.Navigate(typeof(InCallUI), currentNumber);
            }
            catch (Exception ee)
            {
                handleException(ee);
            }
        }

        private void ComposeNumber(object sender, RoutedEventArgs e)
        {
            currentNumber.AddLastChar(((Button)sender).Content.ToString()[0]);
            UpdateCurrentNumber();
        }

        public async void handleException(Exception e)
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
            currentNumber.RemoveLastChar();
            UpdateCurrentNumber();
        }

        private void Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            currentNumber.AddLastChar('+');
            UpdateCurrentNumber();
        }
    }
}
