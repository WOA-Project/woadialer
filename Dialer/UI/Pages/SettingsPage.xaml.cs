using Dialer.Systems;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace Dialer.UI.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public UISystem UISystem => App.Current.UISystem;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void NavigationView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (UISystem.PageNameToType(args.SelectedItem.ToString()) != null)
                ContentFrame.Navigate(UISystem.PageNameToType(args.SelectedItem.ToString()));
        }
    }
}
