﻿using Dialer.Helpers;
using Dialer.UI.Pages;
using Internal.Windows.Calls;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.LockScreen;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            switch (name)
            {
                case CALL_HISTORY_PAGE:
                    return typeof(HistoryPage);
                case CALL_UI_PAGE:
                    return typeof(CallUIPage);
                case CONTACTS_PAGE:
                    return typeof(ContactsPage);
                case DIAL_PAGE:
                    return typeof(DialPage);
                case SETTINGS_PAGE:
                    return typeof(SettingsPage);
                case APPLICATIONS_SETTINGS_PAGE:
                    return typeof(ApplicationsSettings);
                case PHONE_LINES_SETTINGS_PAGE:
                    return typeof(PhoneLinesSettings);
                case PERSONALIZATION_SETTINGS_PAGE:
                    return typeof(PersonalizationSettings);
                case NOTIFICATIONS_SETTINGS_PAGE:
                    return typeof(NotificationsSettings);
                case ABOUT_SETTINGS_PAGE:
                    return typeof(AboutSettings);
                case SOUND_SETTINGS_PAGE:
                default:
                    return null;
            }
        }

        private readonly ObservableCollection<string> _MainPagePages;
        private readonly ObservableCollection<string> _SettingsPages;
        private readonly ObservableCollection<string> _SettingsFooterPages;
        private Window MainWindow;
        private Window CallUIWindow;

        public SystemNavigationManager NavigationManager { get; private set; }
        public LockApplicationHost LockApplicationHost { get; private set; }
        public ReadOnlyObservableCollection2<string> MainPagePages { get; }
        public ReadOnlyObservableCollection<string> SettingsPages { get; }
        public ReadOnlyObservableCollection<string> SettingsFooterPages { get; }

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

        private async void CallManager_ActiveCallChanged(CallManager sender, Call args)
        {
            if (args != null)
            {
                if (!_MainPagePages.Contains(CALL_UI_PAGE))
                {
                    await MainWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _MainPagePages.Add(CALL_UI_PAGE));
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
                await MainWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _MainPagePages.Remove(CALL_UI_PAGE));
            }
        }

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


        private void LockApplicationHost_Unlocking(LockApplicationHost sender, LockScreenUnlockingEventArgs args)
        {
            LockScreenUnlockingDeferral deferral = args.GetDeferral();

            deferral.Complete();
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

        public void OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            Frame frame = ConstructUI();
            App.Current.CallSystem.CallManager.ActiveCallChanged += CallManager_ActiveCallChanged;
            MainWindow = Window.Current;

            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    LaunchActivatedEventArgs launchActivationArgs = args as LaunchActivatedEventArgs;
                    if (launchActivationArgs.PrelaunchActivated == false)
                    {
                        if (frame.Content == null)
                        {
                            if (PhoneCallManager.IsCallActive)
                            {
                                ShowCallUIWindow();
                            }
                            frame.Navigate(typeof(MainPage), launchActivationArgs.Arguments);
                        }
                    }
                    break;
                case ActivationKind.LockScreen:
                    LockApplicationHost = LockApplicationHost.GetForCurrentView();
                    LockApplicationHost.Unlocking += LockApplicationHost_Unlocking;
                    frame.Navigate(typeof(MainPage));
                    break;
                case ActivationKind.Protocol:
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
                    ToastNotificationActivatedEventArgs toastActivationArgs = args as ToastNotificationActivatedEventArgs;
                    App.Current.OnToastNotificationActivated(ToastActivationType.Foreground, toastActivationArgs.Argument);
                    break;
                default:
                    throw new NotSupportedException();
            }
            Window.Current.Activate();
        }

        public void CloseCallUIWindow()
        {
            CallUIWindow?.Close();
            CallUIWindow = null;
        }

        public async void ShowCallUIWindow()
        {
            int compactViewId = 0;
            Size previoussize = new Size(0, 0);

            // Workaround for window spawn bug
            await MainWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var view = ApplicationView.GetForCurrentView();
                var frame = (Window.Current.Content as Frame);

                previoussize = new Size(frame.ActualWidth, frame.ActualHeight);
                view.SetPreferredMinSize(new Size { Width = 400, Height = 100 });
            });

            ViewModePreferences preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            preferences.CustomSize = new Size { Width = 400, Height = 100 };

            CoreApplicationView view = CoreApplication.CreateNewView();

            await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CallUIWindow = Window.Current;
                Frame frame = new Frame();
                Window.Current.Content = frame;
                frame.Navigate(typeof(CallUIPage));
                Window.Current.Activate();

                ApplicationView view = ApplicationView.GetForCurrentView();
                view.Title = App.Current.ResourceLoader.GetString(CALL_UI_PAGE);
                compactViewId = view.Id;

                Window.Current.Closed += (object sender, CoreWindowEventArgs e) =>
                {
                    var view = ApplicationView.GetForCurrentView();

                    view.SetPreferredMinSize(new Size(0, 0));
                    view.TryResizeView(previoussize);
                };
            });

            bool viewShown = await ApplicationViewSwitcher.TryShowAsViewModeAsync(compactViewId, ApplicationViewMode.CompactOverlay, preferences);

            // Workaround for window spawn bug
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var view = ApplicationView.GetForCurrentView();

                view.SetPreferredMinSize(new Size(0, 0));
                view.TryResizeView(previoussize);
            });
        }
    }
}
