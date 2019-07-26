using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Calls;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            if(SettingsManager.isFirstTimeRun()) SettingsManager.setDefaultSettings();
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

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
            switch (args.TaskInstance.Task.Name)
            {
                case TaskManager.USER_NOTIFICATION_CHANGED:
                    NotificationHelper.RemoveSystemCallNotification();
                    break;
                case TaskManager.LINE_STATE_CHANGED:

                    break;
            }
            deferral.Complete();
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
