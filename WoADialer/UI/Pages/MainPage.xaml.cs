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
using WoADialer.UI.Dialogs;
using WoADialer.Helpers;

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

            UpdateState(null, null);
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
            PhoneCallManager.CallStateChanged += UpdateState;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            PhoneCallManager.CallStateChanged -= UpdateState;
        }

        private void UpdateCurrentNumber()
        {
            callButton.IsEnabled = !string.IsNullOrWhiteSpace(currentNumber.ToString());
            numberToDialBox.Text = currentNumber.ToString(SettingsManager.getNumberFormatting());
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
            currentNumber.RemoveLastChar();
            UpdateCurrentNumber();
        }

        private void NumPad_DigitTapped(object sender, char e)
        {
            currentNumber.AddLastChar(e);
            UpdateCurrentNumber();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog dialog = new SettingsDialog();
            await dialog.ShowAsync();
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            await dialog.ShowAsync();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MainEntities.Initialized)
                {
                    await MainEntities.Initialize();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"{ex}").ShowAsync();
            }
        }
    }
}
