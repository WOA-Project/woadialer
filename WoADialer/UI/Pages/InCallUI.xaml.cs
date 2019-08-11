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

namespace WoADialer.UI.Pages
{
    public sealed partial class InCallUI : Page
    {
        private Call _CurrentCall;
        private Call CurrentCall
        {
            get => _CurrentCall;
            set
            {
                if (_CurrentCall != null)
                {
                    _CurrentCall.StateChanged -= CurrentCall_StateChanged;
                }
                _CurrentCall = value;
                if (_CurrentCall != null)
                {
                    _CurrentCall.StateChanged += CurrentCall_StateChanged;
                }
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Bindings.Update());
            }
        }

        public InCallUI()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        }

        private async void CallManager_CurrentCallsChanged(CallManager sender, CallCounts args)
        {
            if (args.DialingCalls == 0 && args.OnHoldCalls == 0 && args.ActiveTalkingCalls == 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(MainPage)));
            }
            else
            {
                if (_CurrentCall == null)
                {
                    //At this point we know that we have calls, see above
                    CurrentCall = App.Current.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Dialing || x.State == CallState.ActiveTalking || x.State == CallState.OnHold);
                }
            }
        }

        private async void CurrentCall_StateChanged(Call sender, CallStateChangedEventArgs args)
        {
            try
            {
                switch (args.NewState)
                {
                    case CallState.ActiveTalking:
                        //vibrate
                        break;
                    case CallState.Disconnected:
                        CurrentCall = App.Current.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Dialing || x.State == CallState.ActiveTalking || x.State == CallState.OnHold);
                        if (CurrentCall == null)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(MainPage)));
                        }
                        break;
                    case CallState.OnHold:
                        Call notHeld = App.Current.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Dialing || x.State == CallState.ActiveTalking);
                        if (notHeld != null)
                        {
                            CurrentCall = notHeld;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await new MessageDialog(ex.ToString()).ShowAsync());
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CurrentCall = App.Current.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Dialing || x.State == CallState.ActiveTalking || x.State == CallState.OnHold);
            App.Current.CallManager.CurrentCallsChanged += CallManager_CurrentCallsChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            App.Current.CallManager.CurrentCallsChanged -= CallManager_CurrentCallsChanged;
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
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}