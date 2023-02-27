using CommunityToolkit.WinUI.Notifications;
using Dialer.Helpers;
using Dialer.UI.Pages;
using Internal.Windows.Calls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.LockScreen;
using Windows.Graphics;
using WinUIEx;

namespace Dialer.Systems
{
    public sealed class UISystem
    {
        private const string TEL = "tel";

        public const string CALL_HISTORY_PAGE = "CallHistoryPage";
        public const string CALL_UI_PAGE = "CallUIPage";
        public const string CONTACTS_PAGE = "ContactsPage";
        public const string DIAL_PAGE = "DialPage";
        public const string SETTINGS_PAGE = "SettingsPage";
        public const string APPLICATIONS_SETTINGS_PAGE = "ApplicationsSettings";
        public const string PHONE_LINES_SETTINGS_PAGE = "PhoneLinesSettings";
        public const string PERSONALIZATION_SETTINGS_PAGE = "PersonalizationSettings";
        public const string NOTIFICATIONS_SETTINGS_PAGE = "NotificationsSettings";
        public const string SOUND_SETTINGS_PAGE = "SoundSettings";
        public const string ABOUT_SETTINGS_PAGE = "AboutSettings";

        public static Type PageNameToType(string name)
        {
            return name switch
            {
                CALL_HISTORY_PAGE => typeof(HistoryPage),
                CALL_UI_PAGE => typeof(CallUIPage),
                CONTACTS_PAGE => typeof(ContactsPage),
                DIAL_PAGE => typeof(DialPage),
                SETTINGS_PAGE => typeof(SettingsPage),
                APPLICATIONS_SETTINGS_PAGE => typeof(ApplicationsSettings),
                PHONE_LINES_SETTINGS_PAGE => typeof(PhoneLinesSettings),
                PERSONALIZATION_SETTINGS_PAGE => typeof(PersonalizationSettings),
                NOTIFICATIONS_SETTINGS_PAGE => typeof(NotificationsSettings),
                ABOUT_SETTINGS_PAGE => typeof(AboutSettings),
                _ => null,
            };
        }

        private readonly ObservableCollection<string> _MainPagePages;
        private readonly ObservableCollection<string> _SettingsPages;
        private readonly ObservableCollection<string> _SettingsFooterPages;
        private Window MainWindow;
        private Window CallUIWindow;

        public LockApplicationHost LockApplicationHost
        {
            get; private set;
        }
        public ReadOnlyObservableCollection2<string> MainPagePages
        {
            get;
        }
        public ReadOnlyObservableCollection<string> SettingsPages
        {
            get;
        }
        public ReadOnlyObservableCollection<string> SettingsFooterPages
        {
            get;
        }

        public UISystem()
        {
            _MainPagePages = new ObservableCollection<string>()
            {
                CALL_HISTORY_PAGE,
                CONTACTS_PAGE,
                DIAL_PAGE,
                SETTINGS_PAGE
            };
            MainPagePages = new ReadOnlyObservableCollection2<string>(_MainPagePages);
            _SettingsPages = new ObservableCollection<string>()
            {
                APPLICATIONS_SETTINGS_PAGE,
                PHONE_LINES_SETTINGS_PAGE,
                PERSONALIZATION_SETTINGS_PAGE,
                NOTIFICATIONS_SETTINGS_PAGE,
                SOUND_SETTINGS_PAGE
            };
            SettingsPages = new ReadOnlyObservableCollection<string>(_SettingsPages);
            _SettingsFooterPages = new ObservableCollection<string>()
            {
                ABOUT_SETTINGS_PAGE
            };
            SettingsFooterPages = new ReadOnlyObservableCollection<string>(_SettingsFooterPages);
        }

        private void CallManager_ActiveCallChanged(CallManager sender, Call args)
        {
            if (args != null)
            {
                if (!_MainPagePages.Contains(CALL_UI_PAGE))
                {
                    _ = MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => _MainPagePages.Add(CALL_UI_PAGE));
                }
                switch (args.State)
                {
                    case CallState.Incoming:
                    case CallState.Dialing:

                        break;
                }
            }
            else if (_MainPagePages.Contains(CALL_UI_PAGE))
            {
                //_ = MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => _MainPagePages.Remove(CALL_UI_PAGE));
            }
        }

        private Frame ConstructUI()
        {
            if (App.Window == null)
            {
                App.Window = new MainWindow();
            }

            if (App.Window.Content is not Frame frame)
            {
                frame = new Frame();
                App.Window.Content = frame;
            }
            return frame;
        }

        private void LockApplicationHost_Unlocking(LockApplicationHost sender, LockScreenUnlockingEventArgs args)
        {
            LockScreenUnlockingDeferral deferral = args.GetDeferral();

            deferral.Complete();
        }

        public void OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            Frame frame = ConstructUI();
            App.Current.CallSystem.CallManager.ActiveCallChanged += CallManager_ActiveCallChanged;
            MainWindow = App.Window;

            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    Windows.ApplicationModel.Activation.LaunchActivatedEventArgs launchActivationArgs = args as Windows.ApplicationModel.Activation.LaunchActivatedEventArgs;
                    if (!launchActivationArgs.PrelaunchActivated)
                    {
                        if (frame.Content == null)
                        {
                            if (PhoneCallManager.IsCallActive)
                            {
                                ShowCallUIWindow();
                            }
                            _ = frame.Navigate(typeof(MainPage), launchActivationArgs.Arguments);
                        }
                    }
                    break;
                case ActivationKind.LockScreen:
                    LockApplicationHost = LockApplicationHost.GetForCurrentView();
                    LockApplicationHost.Unlocking += LockApplicationHost_Unlocking;
                    _ = frame.Navigate(typeof(MainPage));
                    break;
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs protocolActivationArgs = args as ProtocolActivatedEventArgs;
                    _ = protocolActivationArgs.Uri.Scheme switch
                    {
                        TEL => frame.Navigate(typeof(MainPage), protocolActivationArgs.Uri.LocalPath),
                        _ => throw new NotSupportedException(),
                    };
                    break;
                case ActivationKind.ToastNotification:
                    ToastNotificationActivatedEventArgs toastActivationArgs = args as ToastNotificationActivatedEventArgs;
                    App.Current.OnToastNotificationActivated(ToastActivationType.Foreground, toastActivationArgs.Argument);
                    break;
                default:
                    throw new NotSupportedException();
            }
            App.Window.Activate();
        }

        public void OnLaunchedOrActivated(AppActivationArguments args)
        {
            Frame frame = ConstructUI();
            App.Current.CallSystem.CallManager.ActiveCallChanged += CallManager_ActiveCallChanged;
            MainWindow = App.Window;

            switch (args.Kind)
            {
                case ExtendedActivationKind.Launch:
                    Windows.ApplicationModel.Activation.LaunchActivatedEventArgs launchActivationArgs = args.Data as Windows.ApplicationModel.Activation.LaunchActivatedEventArgs;
                    if (!launchActivationArgs.PrelaunchActivated)
                    {
                        if (frame.Content == null)
                        {
                            if (PhoneCallManager.IsCallActive)
                            {
                                ShowCallUIWindow();
                            }
                            _ = frame.Navigate(typeof(MainPage), launchActivationArgs.Arguments);
                        }
                    }
                    break;
                case ExtendedActivationKind.LockScreen:
                    LockApplicationHost = LockApplicationHost.GetForCurrentView();
                    LockApplicationHost.Unlocking += LockApplicationHost_Unlocking;
                    _ = frame.Navigate(typeof(MainPage));
                    break;
                case ExtendedActivationKind.Protocol:
                    ProtocolActivatedEventArgs protocolActivationArgs = args.Data as ProtocolActivatedEventArgs;
                    _ = protocolActivationArgs.Uri.Scheme switch
                    {
                        TEL => frame.Navigate(typeof(MainPage), protocolActivationArgs.Uri.LocalPath),
                        _ => throw new NotSupportedException(),
                    };
                    break;
                case ExtendedActivationKind.ToastNotification:
                    ToastNotificationActivatedEventArgs toastActivationArgs = args.Data as ToastNotificationActivatedEventArgs;
                    App.Current.OnToastNotificationActivated(ToastActivationType.Foreground, toastActivationArgs.Argument);
                    break;
                default:
                    throw new NotSupportedException();
            }
            App.Window.Activate();
        }

        public void CloseCallUIWindow()
        {
            CallUIWindow?.Close();
            CallUIWindow = null;
        }

        public void ShowCallUIWindow()
        {
            Frame mainframe = MainWindow.Content as Frame;
            SizeInt32 previoussize = new((int)mainframe.ActualWidth, (int)mainframe.ActualHeight);

            // Workaround for window spawn bug
            WindowManager MainWindowManager = WindowManager.Get(MainWindow);
            MainWindowManager.MinWidth = 400;
            MainWindowManager.MinHeight = 100;

            CallUIWindow = new Window();
            Frame frame = new();
            CallUIWindow.Content = frame;
            _ = frame.Navigate(typeof(CallUIPage));
            CallUIWindow.Title = App.Current.ResourceLoader.GetString(CALL_UI_PAGE);
            CallUIWindow.Closed += (object sender, WindowEventArgs e) =>
            {
                WindowManager view = WindowManager.Get(sender as Window);
                view.MinWidth = 0;
                view.MinHeight = 0;
                view.Width = previoussize.Width;
                view.Height = previoussize.Height;
            };
            CallUIWindow.Activate();

            WindowManager CallUIWindowManager = WindowManager.Get(CallUIWindow);
            CallUIWindowManager.PresenterKind = AppWindowPresenterKind.CompactOverlay;
            CallUIWindowManager.Width = 400;
            CallUIWindowManager.Height = 100;

            // Workaround for window spawn bug
            MainWindowManager.MinWidth = 0;
            MainWindowManager.MinHeight = 0;
            MainWindowManager.Width = previoussize.Width;
            MainWindowManager.Height = previoussize.Height;
        }
    }
}