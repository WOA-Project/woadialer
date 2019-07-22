using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
using WoADialer.Model;
using Windows.Devices.Sensors;
using Windows.Devices.Enumeration;
using Internal.Windows.Calls;
using WoADialer.UI.Controls;

namespace WoADialer.UI.Pages
{
    public sealed partial class InCallUI : Page
    {
        private ProximitySensor _ProximitySensor;
        private ProximitySensorDisplayOnOffController _DisplayController;

        private Call _CurrentCall;

        public InCallUI()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            MainEntities.CallManager.CallAppeared += CallManager_CallAppeared;
        }

        private void CallManager_CallAppeared(CallManager sender, Call args)
        {
            throw new NotImplementedException();
        }

        private async void TimerCallback(object state)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {

                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.ToString()).ShowAsync();
                }
            });
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                switch (e.Parameter)
                {
                    case MovingCallInfo info:
                        MainEntities.DefaultLine?.Dial(info.Number.ToString(), "test");
                        callerNumberText.Text = info.Number.ToString("nice");
                        break;
                }
                getHistory();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
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
            try
            {
                PhoneCallStore phoneCallStore = await PhoneCallManager.RequestStoreAsync();
                Guid lineId = await phoneCallStore.GetDefaultLineAsync();
                return await PhoneLine.FromIdAsync(lineId);
            }
            catch
            {
                return null;
            }
        }


        private async void CloseCallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _CurrentCall?.End();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
            //create consoleapp helper and restart data service
            //string closeCallCommand = "woadialerhelper:closecall";
            //Uri uri = new Uri(closeCallCommand);
            //var result = await Windows.System.Launcher.LaunchUriAsync(uri);
            //go back to the previous page or close the app if the call was received
            Frame.Navigate(typeof(MainPage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceInformationCollection sensors = await DeviceInformation.FindAllAsync(ProximitySensor.GetDeviceSelector());
                _ProximitySensor = sensors.Count > 0 ? ProximitySensor.FromId(sensors.First().Id) : null;
                _DisplayController = _ProximitySensor?.CreateDisplayOnOffController();
                if (!MainEntities.Initialized)
                {
                    await MainEntities.Initialize();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
        }
    }
}