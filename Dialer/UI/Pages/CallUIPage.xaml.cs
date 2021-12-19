using Dialer.Helpers;
using Internal.Windows.Calls;
using System;
using System.Threading;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Dialer.UI.Pages
{
    public sealed partial class CallUIPage : Page
    {
        public CallManager CallManager => App.Current.CallSystem.CallManager;

        private Timer Timer;
        private Call PresentedCall;

        public CallUIPage()
        {
            this.InitializeComponent();
            var view = ApplicationView.GetForCurrentView();

            //view.SetPreferredMinSize(new Size { Width = 400, Height = 100 });
            //view.TryResizeView(new Size { Width = 400, Height = 100 });

            if (CallManager.ActiveCall != null)
            {
                PresentedCall = CallManager.ActiveCall;
                PresentedCall.StateChanged += PresentedCall_StateChanged;
                PresentedCall_StateChanged(PresentedCall, new CallStateChangedEventArgs(PresentedCall.State, PresentedCall.State, PresentedCall.StateReason));
            }

            CallManager.ActiveCallChanged += CallManager_ActiveCallChanged;
        }

        private void CallManager_ActiveCallChanged(CallManager sender, Call args)
        {
            if (PresentedCall != null)
            {
                PresentedCall.StateChanged -= PresentedCall_StateChanged;
            }
            if (CallManager.ActiveCall != null)
            {
                PresentedCall = CallManager.ActiveCall;
                PresentedCall.StateChanged += PresentedCall_StateChanged;
                PresentedCall_StateChanged(PresentedCall, new CallStateChangedEventArgs(PresentedCall.State, PresentedCall.State, PresentedCall.StateReason));
            }
        }

        private void PresentedCall_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            switch (args.NewState)
            {
                case CallState.Disconnected:
                case CallState.Count:
                case CallState.Indeterminate:
                    Timer?.Dispose();
                    break;
                case CallState.ActiveTalking:
                    Timer = new Timer(TimerCallback, null, TimeSpan.Zero, new TimeSpan(0, 0, 1));
                    break;
            }
        }

        private async void TimerCallback(object state)
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => Bindings.Update());
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
                Keypad.Visibility = Visibility.Collapsed;
            }
            else
            {
                var view = ApplicationView.GetForCurrentView();
                view.TryResizeView(new Size { Width = 400, Height = 100 });
            }

            if(SettingsManager.getProximitySensorOn()) App.Current.DeviceSystem.IsDisplayControlledByProximitySensor = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (SettingsManager.getProximitySensorOn()) App.Current.DeviceSystem.IsDisplayControlledByProximitySensor = false;
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

        private void Abtb_Hold_Unchecked(object sender, RoutedEventArgs e)
        {
            CallManager.ActiveCall?.SetHold(false);
        }

        private void Abtb_Hold_Checked(object sender, RoutedEventArgs e)
        {
            CallManager.ActiveCall?.SetHold(true);
        }

        private void SpeakerOutputButton_Checked(object sender, RoutedEventArgs e)
        {
            CallManager.AudioEndpoint = PhoneAudioRoutingEndpoint.Speakerphone;
        }

        private void SpeakerOutputButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CallManager.AudioEndpoint = PhoneAudioRoutingEndpoint.Default;
        }
    }
}