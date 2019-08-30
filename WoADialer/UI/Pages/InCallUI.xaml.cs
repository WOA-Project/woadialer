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
    public sealed partial class InCallUI : Page
    {
        public static readonly DependencyProperty CurrentCallProperty = DependencyProperty.RegisterAttached("PresentedCall", typeof(CallViewModel), typeof(CallStatePresenter), new PropertyMetadata(null));

        public CallViewModel CurrentCall
        {
            get => (CallViewModel)GetValue(CurrentCallProperty);
            set => SetValue(CurrentCallProperty, value);
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
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(MainPage)));
            }
            else
            {
                if (CurrentCall == null)
                {
                    //At this point we know that we have calls, see above
                    Call call = App.Current.CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Dialing || x.State == CallState.ActiveTalking || x.State == CallState.OnHold);
                    CurrentCall = call == null ? null : new CallViewModel(Dispatcher, call);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Call call = App.Current.CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Dialing || x.State == CallState.ActiveTalking || x.State == CallState.OnHold);
            CurrentCall = call == null ? null : new CallViewModel(Dispatcher, call);
            App.Current.CallSystem.CallManager.CurrentCallsChanged += CallManager_CurrentCallsChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            App.Current.CallSystem.CallManager.CurrentCallsChanged -= CallManager_CurrentCallsChanged;
        }

        private async void CloseCallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentCall?.Call.End();
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