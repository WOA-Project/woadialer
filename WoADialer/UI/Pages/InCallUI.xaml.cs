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

namespace WoADialer.UI.Pages
{
    public sealed partial class InCallUI : Page
    {
        private Timer callLengthCounter;
        private DateTime? callStartTime;
        private ProximitySensor _ProximitySensor;
        private ProximitySensorDisplayOnOffController _DisplayController;
        private PhoneCall FirstCall;
        private bool currentSpeakerState;

        public InCallUI()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            PhoneCallManager.CallStateChanged += PhoneCallManager_CallStateChanged;
        }

        private async void TimerCallback(object state)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    callerNameText.Text = FirstCall?.Name ?? "null";
                    callerNumberText.Text = FirstCall?.Number ?? "null";
                    callTimerText.Text = (DateTime.Now - callStartTime)?.ToString("mm\\:ss") ?? "null";
                    callStatusText.Text = $"{(int?)FirstCall?.ID ?? -1}, {(int?)FirstCall?.ConferenceID ?? -1}| is {FirstCall?.State.ToString() ?? "unknown"}";
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.ToString()).ShowAsync();
                }
            });
        }

        private void StartTimer()
        {
            callStartTime = FirstCall?.StartTime.DateTime;
            callLengthCounter = new Timer(TimerCallback, null, 0, 1000);
        }

        private void StopTimer()
        {
            callLengthCounter.Dispose();
            callStartTime = null;
        }

        private async void PhoneCallManager_CallStateChanged(object sender, object e)
        {
            try
            {
                FirstCall = MainEntities.API.CurrentCalls.FirstOrDefault();
            }
            catch (Exception ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await new MessageDialog(ex.ToString()).ShowAsync());
            }
            if (!callStartTime.HasValue && PhoneCallManager.IsCallActive)
            {
                StartTimer();
            }
            else if (callStartTime.HasValue && !PhoneCallManager.IsCallActive)
            {
                StopTimer();
            }
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
                Task.Delay(150).Wait();
                FirstCall = MainEntities.API.CurrentCalls.FirstOrDefault(x => x.State == CallState.ActiveTalking || x.State == CallState.Dialing || x.State == CallState.OnHold);
                if (!callStartTime.HasValue && PhoneCallManager.IsCallActive)
                {
                    StartTimer();
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
                FirstCall?.End();
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