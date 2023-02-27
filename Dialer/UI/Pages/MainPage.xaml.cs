using Dialer.Systems;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Specialized;
using Windows.System;

namespace Dialer.UI.Pages
{
    public sealed partial class MainPage : Page
    {
        public UISystem UISystem => App.Current.UISystem;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            switch (e.Parameter)
            {
                case string number:
                    Navigate(UISystem.DIAL_PAGE, new EntranceNavigationTransitionInfo(), number);
                    break;
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for ContentFrame navigation.
            ContentFrame.Navigated += On_Navigated;

            // Add keyboard accelerators for backwards navigation.
            KeyboardAccelerator goBack = new()
            {
                Key = VirtualKey.GoBack
            };
            goBack.Invoked += BackInvoked;
            KeyboardAccelerators.Add(goBack);

            // ALT routes here
            KeyboardAccelerator altLeft = new()
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            KeyboardAccelerators.Add(altLeft);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                Navigate("SettingsPage", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                string navItemTag = args.SelectedItemContainer.Tag.ToString();
                Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                _ = ContentFrame.Navigate(UISystem.PageNameToType(args.SelectedItem.ToString()));
            }
        }

        public void Navigate(string navItemTag, NavigationTransitionInfo transitionInfo, object parameter = null)
        {
            Type _page = UISystem.PageNameToType(navItemTag);

            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page is not null && !Equals(preNavPageType, _page))
            {
                nv_PagePresenter.SelectedItem = navItemTag;
                _ = ContentFrame.Navigate(_page, parameter, transitionInfo);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            _ = On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            _ = On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
            {
                return false;
            }

            // Don't go back if the nav pane is overlayed.
            if (nv_PagePresenter.IsPaneOpen &&
                (nv_PagePresenter.DisplayMode == NavigationViewDisplayMode.Compact ||
                 nv_PagePresenter.DisplayMode == NavigationViewDisplayMode.Minimal))
            {
                return false;
            }

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            nv_PagePresenter.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UISystem.MainPagePages.CollectionChanged += MainPagePages_CollectionChanged;
        }

        private void MainPagePages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Contains(UISystem.CALL_UI_PAGE) ?? false)
            {
                Navigate(UISystem.CALL_UI_PAGE, new EntranceNavigationTransitionInfo());
            }
            else if (e.OldItems?.Contains(UISystem.CALL_UI_PAGE) ?? false)
            {
                Navigate(UISystem.CALL_HISTORY_PAGE, new EntranceNavigationTransitionInfo());
                ContentFrame.BackStack.Clear();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UISystem.MainPagePages.CollectionChanged -= MainPagePages_CollectionChanged;
        }
    }
}
