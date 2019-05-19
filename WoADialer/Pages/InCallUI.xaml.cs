using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234238

namespace WoADialer.Pages
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class InCallUI : Page
    {
        private PhoneLine currentPhoneLine;

        public InCallUI()
        {
            this.InitializeComponent();
            prepareItems();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        }

        private async void prepareItems()
        {
            Task<PhoneLine> getDefaultLineTask = GetDefaultPhoneLineAsync();
            currentPhoneLine = await getDefaultLineTask;




        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                callerNumberText.Text = e.Parameter.ToString();
            }
            getHistory();
            base.OnNavigatedTo(e);
        }

        private async void getHistory()
        {
            try
            {
                PhoneCallHistoryStore a = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite);
                IReadOnlyList<PhoneCallHistoryEntry> list = await a.GetEntryReader().ReadBatchAsync();
                foreach (PhoneCallHistoryEntry entry in list)
                {
                    Debug.WriteLine("Entry ------");
                    Debug.WriteLine("Address: " + entry.Address);
                    Debug.WriteLine("Id: " + entry.Id);
                    Debug.WriteLine("SourceId: " + entry.SourceId);
                    Debug.WriteLine("Ringing: " + entry.IsRinging);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("!!!!!!!!");
                Debug.WriteLine(e.Message);
                Debug.WriteLine("!!!!!!!!");
            }
        }

        private async Task<PhoneLine> GetDefaultPhoneLineAsync()
        {
            PhoneCallStore phoneCallStore = await PhoneCallManager.RequestStoreAsync();
            Guid lineId = await phoneCallStore.GetDefaultLineAsync();
            return await PhoneLine.FromIdAsync(lineId);
        }


        private async void CloseCallButton_Click(object sender, RoutedEventArgs e)
        {
            //create consoleapp helper and restart data service
            string closeCallCommand = "woadialerhelper:closecall";
            Uri uri = new Uri(closeCallCommand);
            var result = await Windows.System.Launcher.LaunchUriAsync(uri);
            //go back to the previous page or close the app if the call was received
            Frame.Navigate(typeof(MainPage));
        }
    }
}
