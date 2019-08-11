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
        private string AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ? ApplicationView.GetForCurrentView().Title + " - " : "") + Package.Current.DisplayName;

        public TitlebarControl()
        {
            this.InitializeComponent();
            WindowTitle.Text = AppTitle;
            Window.Current.SetTitleBar(this);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Height = CoreApplication.GetCurrentView().TitleBar.Height;
            var margin = CustomTitleBar.Margin;
            margin.Right = CoreApplication.GetCurrentView().TitleBar.SystemOverlayRightInset;
            CustomTitleBar.Margin = margin;
            CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;


            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var transparentColorBrush = new SolidColorBrush { Opacity = 0 };
            var transparentColor = transparentColorBrush.Color;
            titlebar.BackgroundColor = transparentColor;
            titlebar.ButtonBackgroundColor = transparentColor;
            titlebar.ButtonInactiveBackgroundColor = transparentColor;
            var solidColorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;

            if (solidColorBrush != null)
            {
                titlebar.ButtonForegroundColor = solidColorBrush.Color;
                titlebar.ButtonInactiveForegroundColor = solidColorBrush.Color;
            }

            var colorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;

            if (colorBrush != null)
            {
                titlebar.ForegroundColor = colorBrush.Color;
            }

            var hovercolor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 32;
            titlebar.ButtonHoverBackgroundColor = hovercolor;
            titlebar.ButtonHoverForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 64;
            titlebar.ButtonPressedBackgroundColor = hovercolor;
            titlebar.ButtonPressedForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
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
            AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ? ApplicationView.GetForCurrentView().Title + " - " : "") + Package.Current.DisplayName;
            WindowTitle.Text = AppTitle;
        }

        public string Title
        {
            get => ApplicationView.GetForCurrentView().Title;
            set
            {
                ApplicationView.GetForCurrentView().Title = value;
                AppTitle = (!string.IsNullOrEmpty(ApplicationView.GetForCurrentView().Title) ? ApplicationView.GetForCurrentView().Title + " - " : "") + Package.Current.DisplayName;
                WindowTitle.Text = AppTitle;
            }
        }
    }
}
