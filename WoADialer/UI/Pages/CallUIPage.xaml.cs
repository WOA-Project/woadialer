using Internal.Windows.Calls;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WoADialer.UI.Controls;
using WoADialer.UI.ViewModel;

namespace WoADialer.UI.Pages
{
    public sealed partial class CallUIPage : Page
    {
        public CallManager CallManager => App.Current.CallSystem.CallManager;

        public CallUIPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.Current.DeviceSystem.IsDisplayControlledByProximitySensor = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            App.Current.DeviceSystem.IsDisplayControlledByProximitySensor = false;
            base.OnNavigatingFrom(e);
        }

        private async void CloseCallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CallManager.ActiveCall?.End();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}