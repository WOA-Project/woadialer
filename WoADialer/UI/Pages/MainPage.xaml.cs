using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Internal.Windows.Calls;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoADialer.Model;

namespace WoADialer.UI.Pages
{
    public sealed partial class MainPage : Page
    {
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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            PhoneCallManager.CallStateChanged -= UpdateState;
        }

        private void UpdateCurrentNumber()
        {
            callButton.IsEnabled = !string.IsNullOrWhiteSpace(currentNumber.ToString());
            numberToDialBox.Text = currentNumber.ToString("nice");
        }

        private async void UpdateState(object sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (PhoneCallManager.IsCallActive) callStateIndicatorText.Text = "Status: Call Active";
                else if (PhoneCallManager.IsCallIncoming) callStateIndicatorText.Text = "Status: Call Incoming";
                else callStateIndicatorText.Text = "Status: Phone Idle";
                if (PhoneCallManager.IsCallActive)
                {
                    try
                    {
                        Frame.Navigate(typeof(InCallUI));
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.ToString()).ShowAsync();
                    }
                }
            });
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(InCallUI), new MovingCallInfo() { Number = currentNumber });
            }
            catch (Exception ee)
            {
                handleException(ee);
            }
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
            MainEntities.VibrationDevice?.SimpleHapticsController.SendHapticFeedback(MainEntities.VibrationDevice.SimpleHapticsController.SupportedFeedback.First());
            currentNumber.RemoveLastChar();
            UpdateCurrentNumber();
        }

        private void NumPad_DigitTapped(object sender, char e)
        {
            MainEntities.VibrationDevice?.SimpleHapticsController.SendHapticFeedback(MainEntities.VibrationDevice.SimpleHapticsController.SupportedFeedback.First());
            currentNumber.AddLastChar(e);
            UpdateCurrentNumber();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MainEntities.Initialized)
                {
                    await MainEntities.Initialize();
                }
                PhoneCallManager.CallStateChanged += UpdateState;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"{ex}").ShowAsync();
            }
        }
    }
}
