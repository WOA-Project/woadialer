using Internal.Windows.Calls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
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
            var view = ApplicationView.GetForCurrentView();

            //view.SetPreferredMinSize(new Size { Width = 400, Height = 100 });
            //view.TryResizeView(new Size { Width = 400, Height = 100 });
        }

        private void ResizeView(Size size)
        {
            var view = ApplicationView.GetForCurrentView();
            view.TryResizeView(size);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (CoreApplication.GetCurrentView().IsMain)
            {
                CompactUIGrid.Visibility = Visibility.Collapsed;
                ExtendedUIGrid.Visibility = Visibility.Visible;
                Keypad.Visibility = Visibility.Visible;
            }
            else
            {
                var view = ApplicationView.GetForCurrentView();
                view.TryResizeView(new Size { Width = 400, Height = 100 });
            }

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
        private void HideExtendedUIButton_Click(object sender, RoutedEventArgs e)
        {
            ExtendedUIGrid.Visibility = Visibility.Collapsed;
            CompactUIGrid.Visibility = Visibility.Visible;

            ResizeView(new Size { Width = 400, Height = 100 });
        }

        private void ShowExtendedUIButton_Click(object sender, RoutedEventArgs e)
        {
            CompactUIGrid.Visibility = Visibility.Collapsed;
            ExtendedUIGrid.Visibility = Visibility.Visible;

            if (KeypadToggleButton.IsChecked.GetValueOrDefault())
                ResizeView(new Size { Width = 400, Height = 730 });
            else
                ResizeView(new Size { Width = 400, Height = 530 });
        }

        private void KeypadToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Keypad.Visibility = Visibility.Visible;

            ResizeView(new Size { Width = 400, Height = 730 });
        }

        private void KeypadToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Keypad.Visibility = Visibility.Collapsed;

            ResizeView(new Size { Width = 400, Height = 530 });
        }

        private void SpeakerOutputButton_Click(object sender, RoutedEventArgs e)
        {
            switch (CallManager.AudioEndpoint)
            {
                case PhoneAudioRoutingEndpoint.Speakerphone:
                    CallManager.AudioEndpoint = PhoneAudioRoutingEndpoint.Default;
                    break;
                default:
                    CallManager.AudioEndpoint = PhoneAudioRoutingEndpoint.Speakerphone;
                    break;
            }
        }
    }
}