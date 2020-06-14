using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WoADialer.UI.Controls
{
    public sealed partial class TitlebarControl : UserControl
    {
        private static string AppName = Package.Current.DisplayName;

        private string AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ?
            ApplicationView.GetForCurrentView().Title + " - " : "") +
            AppName;

        private CoreApplicationView coreApplicationView;

        public TitlebarControl()
        {
            this.InitializeComponent();
            Loaded += TitleBarControl_Loaded;

            Window.Current.SetTitleBar(this);
            coreApplicationView = CoreApplication.GetCurrentView();
            coreApplicationView.TitleBar.ExtendViewIntoTitleBar = true;
            Height = coreApplicationView.TitleBar.Height;

            var margin = CustomTitleBar.Margin;
            margin.Right = coreApplicationView.TitleBar.SystemOverlayRightInset;
            CustomTitleBar.Margin = margin;

            coreApplicationView.TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            coreApplicationView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

            /*if (App.appearanceManager != null)
                App.appearanceManager.ThemeChanged += AppearanceManager_ThemeChanged;*/

            RefreshColor();
        }

        private async void AppearanceManager_ThemeChanged(object sender, EventArgs e)
        {
            await coreApplicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RefreshColor();
            });
        }

        private void RefreshColor()
        {
            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var transparentColorBrush = new SolidColorBrush { Opacity = 0 };
            var transparentColor = transparentColorBrush.Color;
            titlebar.BackgroundColor = transparentColor;
            titlebar.ButtonBackgroundColor = transparentColor;
            titlebar.ButtonInactiveBackgroundColor = transparentColor;

            if (this.Resources["ApplicationForegroundThemeBrush"] is SolidColorBrush solidColorBrush)
            {
                titlebar.ButtonForegroundColor = solidColorBrush.Color;
                titlebar.ButtonInactiveForegroundColor = solidColorBrush.Color;
            }

            if (this.Resources["ApplicationForegroundThemeBrush"] is SolidColorBrush colorBrush)
            {
                titlebar.ForegroundColor = colorBrush.Color;
            }

            var hovercolor = (this.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 32;
            titlebar.ButtonHoverBackgroundColor = hovercolor;
            titlebar.ButtonHoverForegroundColor = (this.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 64;
            titlebar.ButtonPressedBackgroundColor = hovercolor;
            titlebar.ButtonPressedForegroundColor = (this.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
        }

        private async void TitleBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AppName = (await Package.Current.GetAppListEntriesAsync())[0].DisplayInfo.DisplayName;
            }
            catch { }

            AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ?
                ApplicationView.GetForCurrentView().Title + " - " : "") +
                AppName;
            WindowTitle.Text = AppTitle;

            /*if (App.appearanceManager != null)
                App.appearanceManager.ThemeChanged += AppearanceManager_ThemeChanged;*/
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            Height = sender.Height;
            var margin = CustomTitleBar.Margin;
            margin.Right = sender.SystemOverlayRightInset;
            CustomTitleBar.Margin = margin;
        }

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        }

        public AppViewBackButtonVisibility BackButtonVisibility
        {
            get => SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility;
            set
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value;
                switch (SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility)
                {
                    case AppViewBackButtonVisibility.Visible:
                        BackButtonBg.Visibility = Visibility.Visible;
                        Arrow.Visibility = Visibility.Collapsed;
                        break;
                    case AppViewBackButtonVisibility.Collapsed:
                        BackButtonBg.Visibility = Visibility.Collapsed;
                        Arrow.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        public void UpdateTitle()
        {
            AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ?
                ApplicationView.GetForCurrentView().Title + " - " : "") +
                AppName;
            WindowTitle.Text = AppTitle;
        }

        public string Title
        {
            get => ApplicationView.GetForCurrentView().Title;
            set
            {
                ApplicationView.GetForCurrentView().Title = value;
                AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ?
                    ApplicationView.GetForCurrentView().Title + " - " : "") +
                    AppName;
                WindowTitle.Text = AppTitle;
            }
        }
    }
}
