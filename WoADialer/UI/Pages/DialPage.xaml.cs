using System;
using System.Text;
using Windows.ApplicationModel.Calls;
using Windows.Foundation;
using Windows.Globalization.PhoneNumberFormatting;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WoADialer.Helpers;
using WoADialer.Systems;

namespace WoADialer.UI.Pages
{
    public sealed partial class DialPage : Page
    {
        private CallSystem CallSystem => App.Current.CallSystem;
        private StringBuilder Number;
        private PhoneLine CurrentPhoneLine;

        public DialPage()
        {
            this.InitializeComponent();
            CurrentPhoneLine = CallSystem.DefaultLine;
        }

        private void DeleteLastNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (Number.Length > 0)
            {
                Number.Remove(Number.Length - 1, 1);
            }
            UpdateCurrentNumber();
        }

        private void NumPad_DigitTapped(object sender, char e)
        {
            Number.Append(e);
            UpdateCurrentNumber();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            switch (e.Parameter)
            {
                case string number:
                    Number = new StringBuilder(number);
                    break;
                default:
                    Number = new StringBuilder();
                    break;
            }
            UpdateCurrentNumber();
        }

        private void UpdateCurrentNumber()
        {
            callButton.IsEnabled = !string.IsNullOrWhiteSpace(Number.ToString());
            PhoneNumberFormatter a = new PhoneNumberFormatter();
            numberToDialBox.Text = a.FormatPartialString(Number.ToString());
        }

        private async void CallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentPhoneLine = await PhoneLine.FromIdAsync(await App.Current.CallSystem.CallStore.GetDefaultLineAsync());
                CurrentPhoneLine?.DialWithOptions(new PhoneDialOptions() { Number = Number.ToString() });
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

        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch(e.Key)
            {
                case VirtualKey.Number0:
                case VirtualKey.NumberPad0:
                    NumPad_DigitTapped(this, '0');
                    break;
                case VirtualKey.Number1:
                case VirtualKey.NumberPad1:
                    NumPad_DigitTapped(this, '1');
                    break;
                case VirtualKey.Number2:
                case VirtualKey.NumberPad2:
                    NumPad_DigitTapped(this, '2');
                    break;
                case VirtualKey.Number3:
                case VirtualKey.NumberPad3:
                    NumPad_DigitTapped(this, '3');
                    break;
                case VirtualKey.Number4:
                case VirtualKey.NumberPad4:
                    NumPad_DigitTapped(this, '4');
                    break;
                case VirtualKey.Number5:
                case VirtualKey.NumberPad5:
                    NumPad_DigitTapped(this, '5');
                    break;
                case VirtualKey.Number6:
                case VirtualKey.NumberPad6:
                    NumPad_DigitTapped(this, '6');
                    break;
                case VirtualKey.Number7:
                case VirtualKey.NumberPad7:
                    NumPad_DigitTapped(this, '7');
                    break;
                case VirtualKey.Number8:
                case VirtualKey.NumberPad8:
                    NumPad_DigitTapped(this, '8');
                    break;
                case VirtualKey.Number9:
                case VirtualKey.NumberPad9:
                    NumPad_DigitTapped(this, '9');
                    break;
                case (VirtualKey)187:
                case VirtualKey.Add:
                    NumPad_DigitTapped(this, '+');
                    break;
                case VirtualKey.Back:
                    DeleteLastNumberButton_Click(this, null);
                    break;
            }
        }
    }
}
