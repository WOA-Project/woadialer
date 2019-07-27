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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoADialer.Background;
using WoADialer.Helpers;
using WoADialer.Model;
using WoADialer.UI.Pages;

namespace WoADialer
{ 
    sealed partial class App : Application
    {
        private const string TEL = "tel";

        private SystemNavigationManager _NavigationManager;

        private NotificationShredder Shredder;
        private CallWaiter Waiter;
        private ManualResetEvent Event;
        private Call Incoming;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            if (SettingsManager.isFirstTimeRun()) SettingsManager.setDefaultSettings();
            MainEntities.Initialize();
        }

        private Frame ConstructUI()
        {
            _NavigationManager = SystemNavigationManager.GetForCurrentView();
            _NavigationManager.BackRequested += NavigationManager_BackRequested;
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
            _NavigationManager.AppViewBackButtonVisibility = frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
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
                        Window.Current.Activate();
                    }
                    break;
                case ActivationKind.LockScreenCall:

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
                    Window.Current.Activate();
                    break;
                case ActivationKind.ToastNotification:
                    frame = ConstructUI();
                    ToastNotificationActivatedEventArgs toastActivationArgs = args as ToastNotificationActivatedEventArgs;
                    QueryString str = QueryString.Parse(toastActivationArgs.Argument);
                    switch (str["action"])
                    {
                        case "answer":
                            uint callID = uint.Parse(str["callId"]);
                            MainEntities.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == callID)?.AcceptIncomingEx();
                            frame.Navigate(typeof(InCallUI), callID);
                            break;
                    }
                    Window.Current.Activate();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
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
                case TaskManager.LINE_STATE_CHANGED:
                    PhoneLineChangedTriggerDetails details = args.TaskInstance.TriggerDetails as PhoneLineChangedTriggerDetails;
                    if (Shredder == null && Waiter == null && Incoming == null)
                    {
                        Event = new ManualResetEvent(false);
                        Shredder = new NotificationShredder();
                        Waiter = new CallWaiter();
                        Shredder.NotificationRemoved += Shredder_NotificationRemoved;
                        Waiter.CallAppeared += Waiter_CallAppeared;
                        Shredder.RegisterListener();
                        Waiter.RegisterListener();
                        await Task.Run(() => Event.WaitOne(5000));
                        //TaskManager.ShowToast("Registered");
                        Event = null;
                        Incoming = null;
                        Waiter = null;
                        Shredder = null;
                        goto default;
                    }
                    else
                    {
                        //TaskManager.ShowToast("Not registered");
                        goto default;
                    }
                    break;
            }
        }

        private void Waiter_CallAppeared(object sender, Call e)
        {
            Incoming = e;
            Waiter.UnregisterListener();
            Waiter.CallAppeared -= Waiter_CallAppeared;
            ToastNotification notification = TaskManager.ShowCallToast(e);
            e.StateChanged += E_StateChanged;
            notification.Activated += Notification_Activated;
        }

        private void E_StateChanged(Call sender, CallState args)
        {
            sender.StateChanged -= E_StateChanged;
            if (args == CallState.Disconnected)
            {
                TaskManager.ShowToast($"Missed call: {sender.Number}");
            }
            Event.Set();
        }

        private void Notification_Activated(ToastNotification sender, object args)
        {
            Incoming.StateChanged -= E_StateChanged;
            Event.Set();
        }

        private void Shredder_NotificationRemoved(object sender, EventArgs e)
        {
            Shredder.UnregisterListener();
            Shredder.NotificationRemoved -= Shredder_NotificationRemoved;
            //TaskManager.ShowToast($"{MainEntities.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Incoming)?.Number}");
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
