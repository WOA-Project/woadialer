using CommunityToolkit.WinUI.Notifications;
using Dialer.Helpers;
using Dialer.Systems;
using Internal.Windows.Calls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;

namespace Dialer
{
    public partial class App : Application, IBackgroundTask
    {
        #region Call system constants
        private static readonly TimeSpan WAIT_CALL_DURATION = new(0, 0, 3);
        #endregion

        public static new App Current => Application.Current as App;

        private Task<bool> ObtainingAccess;
        private Task Initializating;

        #region UI system objects

        public ResourceLoader ResourceLoader
        {
            get; private set;
        }
        public int CompactOverlayId
        {
            get; set;
        }
        #endregion

        public bool IsForeground
        {
            get; private set;
        }
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

            if (SettingsManager.isFirstTimeRun())
            {
                SettingsManager.setDefaultSettings();
            }

            ObtainingAccess = PermissionSystem.RequestAllPermissions();
        }

        private async void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.CreateFileAsync("sample.txt", CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(sampleFile, DateTime.Now + " " + e.Exception.ToString() + '\n');
        }

        #region Application state managment
        private async Task InitializeSystems()
        {
            await BackgroundSystem.Initialize();
            await DeviceSystem.Initialize();
            ResourceLoader = new ResourceLoader();
            NotificationSystem.Initialize();
            await CallSystem.Initialize();
            ContactSystem.LoadContacts();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            if (await ObtainingAccess && PermissionSystem.IsAllPermissionsObtained)
            {
                Initializating ??= InitializeSystems();
                await Initializating;
                BackgroundSystem?.OnBackgroundActivated(taskInstance);
            }
            deferral.Complete();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
            if (await ObtainingAccess && PermissionSystem.IsAllPermissionsObtained)
            {
                Initializating ??= InitializeSystems();
                await Initializating;
                BackgroundSystem?.OnBackgroundActivated(args.TaskInstance);
            }
            deferral.Complete();
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Deferral deferral = e.GetDeferral();
            IsForeground = false;
            if (CallSystem.CallManager != null)
            {
                NotificationSystem.RefreshCallNotification(CallSystem.CallManager.CurrentCalls);
            }

            deferral.Complete();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
        {
            // TODO This code defaults the app to a single instance app. If you need multi instance app, remove this part.
            // Read: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle#single-instancing-in-applicationonlaunched
            // If this is the first instance launched, then register it as the "main" instance.
            // If this isn't the first instance launched, then "main" will already be registered,
            // so retrieve it.
            Microsoft.Windows.AppLifecycle.AppInstance mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");
            AppActivationArguments activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

            // If the instance that's executing the OnLaunched handler right now
            // isn't the "main" instance.
            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            if (await ObtainingAccess && PermissionSystem.IsAllPermissionsObtained)
            {
                Initializating ??= InitializeSystems();
                await Initializating;
            }

            // Initialize MainWindow here
            Window = new MainWindow();
            Window.Activate();

            OnLaunchedOrActivated(activatedEventArgs);
        }

        private async void OnLaunchedOrActivated(AppActivationArguments args)
        {
            IsForeground = true;
            if (!PermissionSystem.IsAllPermissionsObtained && !await ObtainingAccess)
            {
                _ = Window.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ObtainingAccess = PermissionSystem.RequestAllPermissions());
                _ = await ObtainingAccess;
            }
            Initializating ??= InitializeSystems();
            await Initializating;
            NotificationSystem.RemoveCallToastNotifications();
            UISystem.OnLaunchedOrActivated(args);
        }

        private async void OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            IsForeground = true;
            if (!PermissionSystem.IsAllPermissionsObtained && !await ObtainingAccess)
            {
                _ = Window.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ObtainingAccess = PermissionSystem.RequestAllPermissions());
                _ = await ObtainingAccess;
            }
            Initializating ??= InitializeSystems();
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
            WwwFormUrlDecoder decoder = new(args);
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
                    //frame = Window.Content as Frame;
                    //frame.Navigate(typeof(InCallUI));
                    break;
                case NotificationSystem.SHOW_INCOMING_CALL_UI:
                    frame = Window.Content as Frame;
                    //frame.Navigate(typeof(IncomingCallUI));
                    break;
            }
        }
        #endregion

        public static Window Window
        {
            get; set;
        }
    }
}