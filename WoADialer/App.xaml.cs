using Internal.Windows.Calls;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Calls.Background;
using Windows.ApplicationModel.Calls.Provider;
using Windows.ApplicationModel.LockScreen;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoADialer.Helpers;
using WoADialer.Model;
using WoADialer.UI.Pages;

namespace WoADialer
{ 
    public sealed partial class App : Application
    {
        private const string TEL = "tel";

        private const string WINDOWS_SYSTEM_TOAST_CALLING = "Windows.SystemToast.Calling";

        #region Background system constants
        private const string CALL_BLOCKED = "CallBlocked";
        private const string CALL_HISTORY_CHANGED = "CallHistoryChanged";
        private const string CALL_ORIGIN_DATA_REQUEST = "CallOriginDataRequest";
        private const string LINE_STATE_CHANGED = "LineStateChanged";
        private const string NEW_VOICEMAIL_MESSAGE = "NewVoicemailMessage";
        private const string TOAST_BACKGROUNG_ACTIVATED = "ToastBackgroungActivated";
        #endregion

        #region Notification system constants
        private const string ACTION = "Action";
        #region Available actions
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        private const string ANSWER = "Answer";
        /// <summary>
        /// Requires <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        private const string END = "End";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        private const string REJECT = "Reject";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/> and <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        private const string HOLD_AND_ANSWER = "Hold_Answer";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/> and <see cref="ACTIVE_CALL_ID"/>
        /// </summary>
        private const string END_AND_ANSWER = "End_Answer";
        /// <summary>
        /// Requires <see cref="INCOMING_CALL_ID"/>
        /// </summary>
        private const string TEXT_REPLY = "TextReply";
        private const string SHOW_CALL_UI = "ShowCallUI";
        private const string SHOW_INCOMING_CALL_UI = "ShowIncomingCallUI";
        #endregion
        #region Possible parameters
        private const string ACTIVE_CALL_ID = "ActiveCallID";
        private const string INCOMING_CALL_ID = "IncomingCallID";
        #endregion
        #endregion

        #region Call system constants
        private static readonly TimeSpan WAIT_CALL_DURATION = new TimeSpan(0, 0, 3);
        #endregion

        public static new App Current => Application.Current as App;

        private ManualResetEvent CallHandler;
        private bool InitInWindowThread;

        #region Notification system objects
        public UserNotificationListener NotificationListener { get; private set; }
        public TileUpdater TileUpdater { get; private set; }
        public ToastNotificationManagerForUser ToastNotificationManagerForUser { get; private set; }
        public ToastNotifier ToastNotifier { get; private set; }
        public ToastCollectionManager ToastCollectionManager { get; private set; }
        #endregion

        #region Call system objects
        public CallManager CallManager { get; private set; }
        public PhoneCallHistoryStore CallHistoryStore { get; private set; }
        public PhoneCallStore CallStore { get; private set; }
        #endregion

        #region UI system objects
        public SystemNavigationManager NavigationManager { get; private set; }
        public LockApplicationHost LockApplicationHost { get; private set; }
        public ResourceLoader ResourceLoader { get; private set; }
        #endregion

        public ProximitySensor ProximitySensor { get; private set; }


        public App()
        {
            InitializeComponent();
            EnteredBackground += OnEnteredBackground;
            LeavingBackground += OnLeavingBackground;
            Resuming += OnResuming;
            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;

            CallHandler = new ManualResetEvent(false);

            if (SettingsManager.isFirstTimeRun()) SettingsManager.setDefaultSettings();

            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            bool? firstRun = roamingSettings.Values["ftest"] as bool?;

            if (firstRun ?? false)
            {
                Task t = Task.Run(InitializateSystems);
                try
                {
                    t.Wait();
                }
                catch
                {
                    InitInWindowThread = true;
                    roamingSettings.Values["ftest"] = false;
                }
            }
            else
            {
                InitInWindowThread = true;
            }
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        private async void AccessSetup()
        {
            await UserNotificationListener.Current.RequestAccessAsync();
            //await PhoneCallOriginManager.RequestSetAsActiveCallOriginAppAsync();
        }

        #region Application state managment
        private async Task InitializateSystems()
        {
            await InitializateBackgroundSystem();
            await InitializateCallSystem();
            InitializateNotificationSystem();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
            switch (args.TaskInstance.Task.Name)
            {
                default:
                    deferral.Complete();
                    break;
                case CALL_ORIGIN_DATA_REQUEST:
                    //PhoneCallOriginDataRequestTriggerDetails originDataRequest = args.TaskInstance.TriggerDetails as PhoneCallOriginDataRequestTriggerDetails;
                    //PhoneCallOrigin data = new PhoneCallOrigin();
                    //data.Category = "Category";
                    //data.CategoryDescription = "CategoryDescription";
                    //data.DisplayName = "DisplayName";
                    //data.Location = "Location";
                    //PhoneCallOriginManager.SetCallOrigin(originDataRequest.RequestId, data);
                    goto default;
                case LINE_STATE_CHANGED:
                    PhoneLineChangedTriggerDetails lineChangedDetails = args.TaskInstance.TriggerDetails as PhoneLineChangedTriggerDetails;
                    await Task.Run(OnLateBackgroundActivation);
                    CallHandler.WaitOne(WAIT_CALL_DURATION);
                    CallHandler.Reset();
                    deferral.Complete();
                    break;
                case TOAST_BACKGROUNG_ACTIVATED:
                    ToastNotificationActionTriggerDetail toastDetails = args.TaskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    OnToastNotificationActivated(ToastActivationType.Background, toastDetails.Argument);
                    goto default;
            }
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Deferral deferral = e.GetDeferral();

            deferral.Complete();
        }

        private void OnLateBackgroundActivation()
        {
            RemoveSystemToastNotificationIfExist();
            RefreshCallNotification();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        private void OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            Frame frame;
            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    frame = ConstructUI();
                    LaunchActivatedEventArgs launchActivationArgs = args as LaunchActivatedEventArgs;
                    if (launchActivationArgs.PrelaunchActivated == false)
                    {
                        if (frame.Content == null)
                        {
                            if (PhoneCallManager.IsCallActive)
                            {
                                frame.Navigate(typeof(InCallUI));
                            }
                            else
                            {
                                frame.Navigate(typeof(MainPage), launchActivationArgs.Arguments);
                            }
                        }
                    }
                    break;
                case ActivationKind.LockScreen:
                    LockApplicationHost = LockApplicationHost.GetForCurrentView();
                    LockApplicationHost.Unlocking += LockApplicationHost_Unlocking;
                    frame = ConstructUI();
                    frame.Navigate(typeof(MainPage));
                    break;
                case ActivationKind.Protocol:
                    frame = ConstructUI();
                    ProtocolActivatedEventArgs protocolActivationArgs = args as ProtocolActivatedEventArgs;
                    switch (protocolActivationArgs.Uri.Scheme)
                    {
                        case TEL:
                            frame.Navigate(typeof(MainPage), protocolActivationArgs.Uri.LocalPath);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case ActivationKind.ToastNotification:
                    frame = ConstructUI();
                    ToastNotificationActivatedEventArgs toastActivationArgs = args as ToastNotificationActivatedEventArgs;
                    OnToastNotificationActivated(ToastActivationType.Foreground, toastActivationArgs.Argument);
                    break;
                default:
                    throw new NotSupportedException();
            }
            Window.Current.Activate();
            Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PhoneCallOriginManager.RequestSetAsActiveCallOriginAppAsync());
            if (InitInWindowThread)
            {
                ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["ftest"] = true;
                Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AccessSetup();
                    InitializateSystems();
                });
            }
        }

        private void OnLeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Deferral deferral = e.GetDeferral();

            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {

        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }

        private void OnToastNotificationActivated(ToastActivationType activationType, string args)
        {
            QueryString query = QueryString.Parse(args);
            uint activeCallID = 0;
            uint incomingCallID = 0;
            Call activeCall = null;
            Call incomingCall = null;
            Frame frame = null;
            switch (query[ACTION])
            {
                case END:
                    activeCallID = uint.Parse(query[ACTIVE_CALL_ID]);
                    activeCall = CallManager.CurrentCalls.FirstOrDefault(x => x.ID == activeCallID);
                    //if (activeCall?.AvailableActions.EndCallAvailable ?? false)
                    //{
                    activeCall?.End();
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    break;
                case REJECT:
                    incomingCallID = uint.Parse(query[INCOMING_CALL_ID]);
                    incomingCall = CallManager.CurrentCalls.FirstOrDefault(x => x.ID == incomingCallID);
                    incomingCall?.RejectIncoming();
                    break;
                case TEXT_REPLY:
                    incomingCallID = uint.Parse(query[INCOMING_CALL_ID]);

                    break;
                case END_AND_ANSWER:
                    activeCallID = uint.Parse(query[ACTIVE_CALL_ID]);
                    activeCall = CallManager.CurrentCalls.FirstOrDefault(x => x.ID == activeCallID);
                    //if (activeCall?.AvailableActions.EndCallAvailable ?? false)
                    //{
                    activeCall?.End();
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    goto case ANSWER;
                case HOLD_AND_ANSWER:
                    activeCallID = uint.Parse(query[ACTIVE_CALL_ID]);
                    activeCall = CallManager.CurrentCalls.FirstOrDefault(x => x.ID == activeCallID);
                    //if (activeCall?.AvailableActions.HoldAvailable ?? false)
                    //{
                    activeCall?.SetHold(true);
                    //}
                    //else
                    //{
                    //    //LOG
                    //}
                    goto case ANSWER;
                case ANSWER:
                    incomingCallID = uint.Parse(query[INCOMING_CALL_ID]);
                    incomingCall = CallManager.CurrentCalls.FirstOrDefault(x => x.ID == incomingCallID);
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
                        goto case SHOW_CALL_UI;
                    }
                    else
                    {
                        break;
                    }
                case SHOW_CALL_UI:
                    frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(InCallUI));
                    break;
                case SHOW_INCOMING_CALL_UI:
                    frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(IncomingCallUI));
                    break;
            }
        }

        private void LockApplicationHost_Unlocking(LockApplicationHost sender, LockScreenUnlockingEventArgs args)
        {
            LockScreenUnlockingDeferral deferral = args.GetDeferral();

            deferral.Complete();
        }
        #endregion

        #region Background system
        private async Task InitializateBackgroundSystem()
        {
            await ConfigureBackgroundTasks();
        }

        private async Task ConfigureBackgroundTasks(bool force = false)
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            Dictionary<string, bool> taskRegistration = new Dictionary<string, bool>()
            {
                { CALL_BLOCKED, false },
                { CALL_HISTORY_CHANGED, false },
                { CALL_ORIGIN_DATA_REQUEST, false },
                { NEW_VOICEMAIL_MESSAGE, false },
                { LINE_STATE_CHANGED, false },
                { TOAST_BACKGROUNG_ACTIVATED, false },
            };
            foreach (IBackgroundTaskRegistration registeredTask in BackgroundTaskRegistration.AllTasks.Select(x => x.Value))
            {
                switch (registeredTask.Name)
                {
                    case string taskName when taskRegistration.Keys.Contains(taskName):
                        if (force)
                        {
                            goto default;
                        }
                        else
                        {
                            taskRegistration[taskName] = true;
                        }
                        break;
                    default:
                        registeredTask.Unregister(false);
                        break;
                }
            }
            foreach (string taskName in taskRegistration.Keys)
            {
                if (!taskRegistration[taskName])
                {
                    BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                    taskBuilder.Name = taskName;
                    switch (taskName)
                    {
                        case CALL_BLOCKED:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallBlocked, false));
                            break;
                        case CALL_HISTORY_CHANGED:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallHistoryChanged, false));
                            break;
                        case CALL_ORIGIN_DATA_REQUEST:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallOriginDataRequest, false));
                            break;
                        case NEW_VOICEMAIL_MESSAGE:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.NewVoicemailMessage, false));
                            break;
                        case LINE_STATE_CHANGED:
                            taskBuilder.SetTrigger(new PhoneTrigger(PhoneTriggerType.LineChanged, false));
                            break;
                        case TOAST_BACKGROUNG_ACTIVATED:
                            taskBuilder.SetTrigger(new ToastNotificationActionTrigger());
                            break;
                        default:
                            throw new NotImplementedException($"Case for {taskName} task missed.");
                    }
                    taskBuilder.Register();
                }
            }
        }
        #endregion

        #region UI system
        private Frame ConstructUI()
        {
            if (NavigationManager == null)
            {
                NavigationManager = SystemNavigationManager.GetForCurrentView();
                NavigationManager.BackRequested += NavigationManager_BackRequested;
            }
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                frame = new Frame();
                frame.NavigationFailed += Frame_NavigationFailed;
                frame.Navigated += Frame_Navigated;
                Window.Current.Content = frame;
            }
            return frame;
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            Frame frame = sender as Frame;
            NavigationManager.AppViewBackButtonVisibility = frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void NavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Window.Current.Content is Frame frame && frame != null)
            {
                if (frame.CanGoBack)
                {
                    frame.GoBack();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Notification system
        private void InitializateNotificationSystem()
        {
            NotificationListener = UserNotificationListener.Current;
            TileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            ToastNotificationManagerForUser = ToastNotificationManager.GetDefault();
            ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            ToastCollectionManager = ToastNotificationManagerForUser.GetToastCollectionManager();
            NotificationListener.NotificationChanged += NotificationListener_NotificationChanged;
        }

        private void NotificationListener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            switch (args.ChangeKind)
            {
                case UserNotificationChangedKind.Added:
                    UserNotification notification = sender.GetNotification(args.UserNotificationId);
                    if (notification != null && notification.AppInfo.AppUserModelId == WINDOWS_SYSTEM_TOAST_CALLING)
                    {
                        sender.RemoveNotification(notification.Id);
                    }
                    break;
            }
        }

        private async void RemoveSystemToastNotificationIfExist()
        {
            IReadOnlyList<UserNotification> notifications = await NotificationListener.GetNotificationsAsync(NotificationKinds.Toast);
            UserNotification notification = notifications.FirstOrDefault(x =>
            {
                try
                {
                    return x.AppInfo.AppUserModelId == WINDOWS_SYSTEM_TOAST_CALLING;
                }
                //That may happens when notifications come from non-UWP app, like MobileShell
                catch
                {
                    return false;
                }
            });
            if (notification != null)
            {
                NotificationListener.RemoveNotification(notification.Id);
            }
        }

        private void RemoveIncomingOrActiveCallToastNotifications()
        {
            IReadOnlyList<ToastNotification> notifications = ToastNotificationManager.History.GetHistory();
            foreach (ToastNotification notification in notifications)
            {
                switch (notification.Tag)
                {
                    case "IncomingCall":
                    case "ParallelIncomingCall":
                    case "ActiveCall":
                        ToastNotificationManager.History.Remove(notification.Tag);
                        break;
                }
            }
        }

        private ToastNotification CreateIncomingCallToastNotification(Call call)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = call.Name },
                            new AdaptiveText() { Text = $"{call.Line?.NetworkName} - {call.State}" },
                            new AdaptiveText() { Text = call.Number }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Text reply", $"{ACTION}={TEXT_REPLY}&{INCOMING_CALL_ID}={call.ID}")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("Reject", $"{ACTION}={REJECT}&{INCOMING_CALL_ID}={call.ID}")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("Answer", $"{ACTION}={ANSWER}&{INCOMING_CALL_ID}={call.ID}")
                    }
                },
                Launch = $"{ACTION}={SHOW_INCOMING_CALL_UI}",
                Scenario = ToastScenario.IncomingCall
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = "IncomingCall"
            };
            return notification;
        }

        private ToastNotification CreateParallelIncomingCallToastNotification(Call incomingCall, Call activeCall)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = incomingCall.Name },
                            new AdaptiveText() { Text = $"{incomingCall.Line?.NetworkName} - {incomingCall.State}" },
                            new AdaptiveText() { Text = incomingCall.Number },
                            new AdaptiveText() { Text = activeCall.Name },
                            new AdaptiveText() { Text = $"{activeCall.Line?.NetworkName} - {incomingCall.State}" },
                            new AdaptiveText() { Text = activeCall.Number },
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Text reply", $"{ACTION}={TEXT_REPLY}&{INCOMING_CALL_ID}={incomingCall.ID}")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("End & Answer", $"{ACTION}={END_AND_ANSWER}&{INCOMING_CALL_ID}={incomingCall.ID}&{ACTIVE_CALL_ID}={activeCall.ID}"),
                        new ToastButton("Reject", $"{ACTION}={REJECT}&{INCOMING_CALL_ID}={incomingCall.ID}")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("Hold & Answer", $"{ACTION}={HOLD_AND_ANSWER}&{INCOMING_CALL_ID}={incomingCall.ID}&{ACTIVE_CALL_ID}={activeCall.ID}")
                    }
                },
                Audio = new ToastAudio() { Silent = true },
                Launch = $"{ACTION}={SHOW_INCOMING_CALL_UI}",
                Scenario = ToastScenario.IncomingCall
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = "ParallelIncomingCall"
            };
            return notification;
        }

        private ToastNotification CreateActiveCallToastNotification(Call call)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = call.Name },
                            new AdaptiveText() { Text = $"{call.Line?.NetworkName} - {call.State}" },
                            new AdaptiveText() { Text = call.Number }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Open app", $"{ACTION}={SHOW_CALL_UI}"),
                        new ToastButton("End call", $"{ACTION}={END}&{ACTIVE_CALL_ID}={call.ID}")
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                },
                Audio = new ToastAudio() { Silent = true },
                Launch = $"{ACTION}={SHOW_CALL_UI}",
                Scenario = ToastScenario.IncomingCall
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = "ActiveCall"
            };
            return notification;
        }

        private ToastNotification CreateMissedCallToastNotification(Call call)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = $"Missed call - {call.Name}" },
                            new AdaptiveText() { Text = call.Number }
                        }
                    }
                },
                Scenario = ToastScenario.Default
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml());
            notification.Group = "MissedCalls";
            return notification;
        }

        private TileNotification CreateMissedCallTileNotification()
        {
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "Mon",
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = "22",
                                    HintStyle = AdaptiveTextStyle.Body,
                                    HintAlign = AdaptiveTextAlign.Center
                                }
                            }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        DisplayName = "Monday 22",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "Snowboarding with Mark",
                                    HintWrap = true,
                                    HintMaxLines = 2
                                },
                                new AdaptiveText()
                                {
                                    Text = "Fri: 9:00 AM",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Monday 22",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText() { Text = "Snowboarding with Mark" },
                                new AdaptiveText()
                                {
                                    Text = "Mt. Baker",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                                new AdaptiveText()
                                {
                                    Text = "Tomorrow: 9:00 AM – 5:00 PM",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Monday 22",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = "Snowboarding with Mark",
                                                    HintWrap = true
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = "Mt. Baker",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = "Tomorrow: 9:00 AM – 5:00 PM",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                }
                                            }
                                        }
                                    }
                                },
                                new AdaptiveText() { Text = "" },
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = "Casper Baby Pants",
                                                    HintWrap = true
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = "Hollywood Bowl",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = "Tomorrow: 8:00 PM – 11:00 PM",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            TileNotification notification = new TileNotification(tileContent.GetXml());
            return notification;
        }
        #endregion

        #region Call system
        private async Task InitializateCallSystem()
        {
            CallManager = await CallManager.GetCallManagerAsync();
            CallStore = await PhoneCallManager.RequestStoreAsync();
            CallHistoryStore = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite);
            CallManager.CallAppeared += CallManager_CallAppeared;
        }

        private void CallManager_CallAppeared(CallManager sender, Call args)
        {
            RefreshCallNotification();
            CallHandler.Set();
        }

        private void Call_StateChanged(Call sender, CallState args)
        {
            switch (args)
            {
                case CallState.Disconnected:
                    RemoveIncomingOrActiveCallToastNotifications();
                    sender.StateChanged -= Call_StateChanged;
                    RefreshCallNotification();
                    break;
            }
        }

        private void RefreshCallNotification()
        {
            RemoveIncomingOrActiveCallToastNotifications();
            Call incomingCall = CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Incoming);
            Call activeCall = CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.ActiveTalking);
            ToastNotification notification = incomingCall != null && activeCall != null ? CreateParallelIncomingCallToastNotification(incomingCall, activeCall) :
                                             incomingCall != null ? CreateIncomingCallToastNotification(incomingCall) :
                                             activeCall != null ? CreateActiveCallToastNotification(activeCall) : null;
            if (incomingCall != null)
            {
                incomingCall.StateChanged += Call_StateChanged;
            }
            if (activeCall != null)
            {
                activeCall.StateChanged += Call_StateChanged;
            }
            if (notification != null)
            {
                ToastNotifier.Show(notification);
                CallHandler.Set();
            }
        }
        #endregion
    }
}
