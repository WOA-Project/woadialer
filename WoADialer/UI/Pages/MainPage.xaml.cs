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
        private PhoneLine _CurrentPhoneLine;

        public MainPage()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;
            titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;

            //if calls missed // callHistoryButton.Content = "&#xF739;";
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
                _CurrentPhoneLine = await PhoneLine.FromIdAsync(await App.Current.CallStore.GetDefaultLineAsync());
                _CurrentPhoneLine.DialWithOptions(new PhoneDialOptions() { Number = currentNumber.ToString() });
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

        private void CallHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(History));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //_CurrentPhoneLine = await PhoneLine.FromIdAsync(await App.Current.CallStore.GetDefaultLineAsync());
            }
            catch
            {

            }
            lv_CallHistory.Items.Clear();
            IReadOnlyList<PhoneCallHistoryEntry> _entries = await App.Current.CallHistoryStore.GetEntryReader().ReadBatchAsync();
            List<PhoneCallHistoryEntry> entries = _entries.ToList();
            entries.Sort((x, y) => y.StartTime.CompareTo(x.StartTime));
            foreach(PhoneCallHistoryEntry entry in entries)
            {
                lv_CallHistory.Items.Add(new TextBlock() { Text = $"{entry.StartTime}: {entry.Address.DisplayName} {entry.Address.RawAddress} {(entry.IsMissed ? "Missed" : "")}" });
            }
        }
    }
}
