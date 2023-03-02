using Dialer.Helpers;
using Internal.Windows.Calls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading;
using Windows.ApplicationModel.Calls;
using Windows.Graphics;
using WinUIEx;

namespace Dialer.UI.Pages
{
    public sealed partial class CallUIPage : Page
    {
        public CallManager CallManager => App.Current.CallSystem.CallManager;

        private Timer Timer;
        private Call PresentedCall;

        public CallUIPage()
        {
            InitializeComponent();

            // Retrieve the window handle (HWND) of the current (XAML) WinUI 3 window.
            nint hWnd = HwndExtensions.GetActiveWindow();

            // Retrieve the WindowId that corresponds to hWnd.
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // You now have an AppWindow object, and you can call its methods to manipulate the window.
                //appWindow.SetPreferredMinSize(new Size { Width = 400, Height = 100 });
                //appWindow.Resize(new Size { Width = 400, Height = 100 });
            }

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

        private void TimerCallback(object state)
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, Bindings.Update);
        }

        private void ResizeView(SizeInt32 size)
        {
            // Retrieve the window handle (HWND) of the current (XAML) WinUI 3 window.
            nint hWnd = HwndExtensions.GetActiveWindow();

            // Retrieve the WindowId that corresponds to hWnd.
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            // You now have an AppWindow object, and you can call its methods to manipulate the window.
            appWindow?.Resize(size);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the window handle (HWND) of the current (XAML) WinUI 3 window.
            nint hWnd = HwndExtensions.GetActiveWindow();

            // Retrieve the WindowId that corresponds to hWnd.
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            // You now have an AppWindow object, and you can call its methods to manipulate the window.
            if (appWindow == App.Window.GetAppWindow())
            {
                CompactUIGrid.Visibility = Visibility.Collapsed;
                ExtendedUIGrid.Visibility = Visibility.Visible;
                Keypad.Visibility = Visibility.Collapsed;
            }
            else
            {
                appWindow?.Resize(new SizeInt32 { Width = 400, Height = 100 });
            }

            if (SettingsManager.getProximitySensorOn())
            {
                App.Current.DeviceSystem.IsDisplayControlledByProximitySensor = true;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (SettingsManager.getProximitySensorOn())
            {
                App.Current.DeviceSystem.IsDisplayControlledByProximitySensor = false;
            }

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
                _ = await new ContentDialog() { Title = "Unhandled Exception", Content = ex.ToString(), XamlRoot = XamlRoot, IsPrimaryButtonEnabled = true, PrimaryButtonText = "Ok" }.ShowAsync();
            }
        }
        private void HideExtendedUIButton_Click(object sender, RoutedEventArgs e)
        {
            ExtendedUIGrid.Visibility = Visibility.Collapsed;
            CompactUIGrid.Visibility = Visibility.Visible;

            ResizeView(new SizeInt32 { Width = 400, Height = 100 });
        }

        private void ShowExtendedUIButton_Click(object sender, RoutedEventArgs e)
        {
            CompactUIGrid.Visibility = Visibility.Collapsed;
            ExtendedUIGrid.Visibility = Visibility.Visible;

            if (KeypadToggleButton.IsChecked.GetValueOrDefault())
            {
                ResizeView(new SizeInt32 { Width = 400, Height = 730 });
            }
            else
            {
                ResizeView(new SizeInt32 { Width = 400, Height = 530 });
            }
        }

        private void KeypadToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Keypad.Visibility = Visibility.Visible;

            ResizeView(new SizeInt32 { Width = 400, Height = 730 });
        }

        private void KeypadToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Keypad.Visibility = Visibility.Collapsed;

            ResizeView(new SizeInt32 { Width = 400, Height = 530 });
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