using Dialer.Systems;
using Microsoft.UI.Xaml.Controls;

namespace Dialer.UI.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public UISystem UISystem => App.Current.UISystem;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (UISystem.PageNameToType(args.SelectedItem.ToString()) != null)
            {
                _ = ContentFrame.Navigate(UISystem.PageNameToType(args.SelectedItem.ToString()));
            }
        }
    }
}
