using System;
using Windows.ApplicationModel.Calls;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WoADialer.Helpers;
using WoADialer.Model;

namespace WoADialer.UI.Pages
{
    public sealed partial class DialPage : Page
    {
        private PhoneNumber currentNumber;
        private PhoneLine _CurrentPhoneLine;

        public DialPage()
        {
            this.InitializeComponent();
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
            numberToDialBox.Text = currentNumber.ToString(SettingsManager.getNumberFormatting());
        }

        private async void CallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _CurrentPhoneLine = await PhoneLine.FromIdAsync(await App.Current.CallSystem.CallStore.GetDefaultLineAsync());
                _CurrentPhoneLine?.DialWithOptions(new PhoneDialOptions() { Number = currentNumber.ToString() });
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
    }
}
