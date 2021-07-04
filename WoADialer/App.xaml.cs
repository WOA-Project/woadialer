using Dialer.Helpers;
using Dialer.Systems;
using Internal.Windows.Calls;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Dialer
{
    public sealed partial class App : Application
    {
        #region Call system constants
        private static readonly TimeSpan WAIT_CALL_DURATION = new TimeSpan(0, 0, 3);
        #endregion

        public static new App Current => Application.Current as App;

        private Task<bool> ObtainingAccess;
        private Task Initializating;

        #region UI system objects

        public ResourceLoader ResourceLoader { get; private set; }
        public int CompactOverlayId { get; set; }
        #endregion

        public bool IsForeground { get; private set; }
        public BackgroundSystem BackgroundSystem { get; } = new BackgroundSystem();
        public CallSystem CallSystem { get; } = new CallSystem();
        public DeviceSystem DeviceSystem { get; } = new DeviceSystem();
        public NotificationSystem NotificationSystem { get; } = new NotificationSystem();
        public PermissionSystem PermissionSystem { get; } = new PermissionSystem();
        public UISystem UISystem { get; } = new UISystem();

        public App()
        {
            InitializeComponent();
            EnteredBackground += OnEnteredBackground;
            LeavingBackground += OnLeavingBackground;
            Resuming += OnResuming;
            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;

            if (SettingsManager.isFirstTimeRun()) SettingsManager.setDefaultSettings();
            ObtainingAccess = PermissionSystem.RequestAllPermissions();
        }

        private async void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.CreateFileAsync("sample.txt", CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(sampleFile, DateTime.Now + " " + e.Exception.ToString() + '\n');
        }

        #region Application state managment
        private async Task InitializateSystems()
        {
            await BackgroundSystem.Initializate();
            await DeviceSystem.Initializate();
            ResourceLoader = ResourceLoader.GetForViewIndependentUse();
            NotificationSystem.Initializate();
            await CallSystem.Initializate();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
            if (await ObtainingAccess && PermissionSystem.IsAllPermissionsObtained)
            {
                if (Initializating == null)
                {
                    Initializating = InitializateSystems();
                }
                await Initializating;
                BackgroundSystem.OnBackgroundActivated(args.TaskInstance);
            }
            deferral.Complete();
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Deferral deferral = e.GetDeferral();
            IsForeground = false;
            NotificationSystem.RefreshCallNotification(CallSystem.CallManager.CurrentCalls);
            deferral.Complete();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        private async void OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            IsForeground = true;
            if (!PermissionSystem.IsAllPermissionsObtained && !await ObtainingAccess)
            {
                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    ObtainingAccess = PermissionSystem.RequestAllPermissions();
                });
                await ObtainingAccess;
            }
            if (Initializating == null)
            {
                Initializating = InitializateSystems();
            }
            await Initializating;
            NotificationSystem.RemoveCallToastNotifications();
            UISystem.OnLaunchedOrActivated(args);
        }

        private void OnLeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Deferral deferral = e.GetDeferral();
            IsForeground = true;
            NotificationSystem.RemoveCallToastNotifications();
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {

        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            IsForeground = false;
            if (CallSystem.CallManager != null)
            {
                NotificationSystem.RefreshCallNotification(CallSystem.CallManager.CurrentCalls);
            }
            deferral.Complete();
        }

        public void OnToastNotificationActivated(ToastActivationType activationType, string args)
        {
            WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(args);
            uint activeCallID = 0;
            uint incomingCallID = 0;
            Call activeCall = null;
            Call incomingCall = null;
            Frame frame = null;
            switch (decoder.GetFirstValueByName(NotificationSystem.ACTION))
            {
                case NotificationSystem.END:
                    activeCallID = uint.Parse(decoder.GetFirstValueByName(NotificationSystem.ACTIVE_CALL_ID));
                    activeCall = CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == activeCallID);
                    //if (activeCall?.AvailableActions.EndCallAvailable ?? false)
                    //{
                    activeCall?.End();
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    break;
                case NotificationSystem.REJECT:
                    incomingCallID = uint.Parse(decoder.GetFirstValueByName(NotificationSystem.INCOMING_CALL_ID));
                    incomingCall = CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == incomingCallID);
                    incomingCall?.RejectIncoming();
                    break;
                case NotificationSystem.TEXT_REPLY:
                    incomingCallID = uint.Parse(decoder.GetFirstValueByName(NotificationSystem.INCOMING_CALL_ID));

                    break;
                case NotificationSystem.END_AND_ANSWER:
                    activeCallID = uint.Parse(decoder.GetFirstValueByName(NotificationSystem.ACTIVE_CALL_ID));
                    activeCall = CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == activeCallID);
                    //if (activeCall?.AvailableActions.EndCallAvailable ?? false)
                    //{
                    activeCall?.End();
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    goto case NotificationSystem.ANSWER;
                case NotificationSystem.HOLD_AND_ANSWER:
                    activeCallID = uint.Parse(decoder.GetFirstValueByName(NotificationSystem.ACTIVE_CALL_ID));
                    activeCall = CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == activeCallID);
                    //if (activeCall?.AvailableActions.HoldAvailable ?? false)
                    //{
                    activeCall?.SetHold(true);
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    goto case NotificationSystem.ANSWER;
                case NotificationSystem.ANSWER:
                    incomingCallID = uint.Parse(decoder.GetFirstValueByName(NotificationSystem.INCOMING_CALL_ID));
                    incomingCall = CallSystem.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == incomingCallID);
                    //if (incomingCall?.AvailableActions.AnswerAvailable ?? false)
                    //{
                    incomingCall?.AcceptIncomingEx();
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    if (activationType == ToastActivationType.Foreground)
                    {
                        goto case NotificationSystem.SHOW_CALL_UI;
                    }
                    else
                    {
                        break;
                    }
                case NotificationSystem.SHOW_CALL_UI:
                    UISystem.ShowCallUIWindow();
                    //frame = Window.Current.Content as Frame;
                    //frame.Navigate(typeof(InCallUI));
                    break;
                case NotificationSystem.SHOW_INCOMING_CALL_UI:
                    frame = Window.Current.Content as Frame;
                    //frame.Navigate(typeof(IncomingCallUI));
                    break;
            }
        }
        #endregion
    }
}
