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

        protected override void OnActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootFrame;
            }

            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                    rootFrame.Navigate(typeof(MainPage), eventArgs.Uri.LocalPath);
                    break;
                case ActivationKind.LockScreen:

                    break;

                case ActivationKind.LockScreenCall:

                    break;
                case ActivationKind.ToastNotification:
                    var toastActivationArgs = args as ToastNotificationActivatedEventArgs;

                    QueryString str = QueryString.Parse(toastActivationArgs.Argument);

                    switch (str["action"])
                    {
                        case "answer":
                            uint callID = uint.Parse(str["callId"]);
                            MainEntities.CallManager.CurrentCalls.FirstOrDefault(x => x.ID == callID)?.AcceptIncomingEx();
                            break;
                    }
                    if (rootFrame.BackStack.Count == 0)
                    {
                        rootFrame.BackStack.Add(new PageStackEntry(typeof(MainPage), null, null));
                    }
                    break;
                default:
                    rootFrame.Navigate(typeof(InCallUI));
                    break;
            }
            Window.Current.Activate();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    if (PhoneCallManager.IsCallActive)
                    {
                        rootFrame.Navigate(typeof(InCallUI));
                    } else
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                }
                Window.Current.Activate();
            }
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

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
