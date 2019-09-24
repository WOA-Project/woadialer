using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WoADialer.Systems;
using WoADialer.UI.Dialogs;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace WoADialer.UI.Pages
{
    public sealed partial class MainPage : Page
    {
        public UISystem UISystem => App.Current.UISystem;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            switch (e.Parameter)
            {
                case string number:
                    NavView_Navigate(UISystem.DIAL_PAGE, new EntranceNavigationTransitionInfo(), number);
                    break;
            }
        }

        private async void NavView_ItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs args)
        {
            //if (args.IsSettingsInvoked == true)
            //{
            //    SettingsDialog dialog = new SettingsDialog();
            //    await dialog.ShowAsync();
            //}
            //else if (args.InvokedItemContainer != null)
            //{
            //    var navItemTag = args.InvokedItemContainer.Tag.ToString();
            //    NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            //}
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for ContentFrame navigation.
            ContentFrame.Navigated += On_Navigated;

            // Add keyboard accelerators for backwards navigation.
            var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);

            // ALT routes here
            var altLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(altLeft);
        }

        private async void NavView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                SettingsDialog dialog = new SettingsDialog();
                await dialog.ShowAsync();
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                ContentFrame.Navigate(UISystem.PageNameToType(args.SelectedItem.ToString()));
            }
        }

        private async void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                SettingsDialog dialog = new SettingsDialog();
                await dialog.ShowAsync();
            }
            else if (navItemTag == "About")
            {
                AboutDialog dialog = new AboutDialog();
                await dialog.ShowAsync();
            }
            else
            {
                _page = UISystem.PageNameToType(navItemTag);
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private async void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo, object parameter)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                SettingsDialog dialog = new SettingsDialog();
                await dialog.ShowAsync();
            }
            else if (navItemTag == "About")
            {
                AboutDialog dialog = new AboutDialog();
                await dialog.ShowAsync();
            }
            else
            {
                _page = UISystem.PageNameToType(navItemTag);
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, parameter, transitionInfo);
            }
        }

        private void NavView_BackRequested(MUXC.NavigationView sender, MUXC.NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (nv_PagePresenter.IsPaneOpen &&
                (nv_PagePresenter.DisplayMode == MUXC.NavigationViewDisplayMode.Compact ||
                 nv_PagePresenter.DisplayMode == MUXC.NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            nv_PagePresenter.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType != null)
            {
                //var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                //NavView.SelectedItem = NavView.MenuItems
                //    .OfType<MUXC.NavigationViewItem>()
                //    .First(n => n.Tag.Equals(item.Tag));
            }
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            await dialog.ShowAsync();
        }
    }
}
