using Dialer.Helpers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Dialer.UI.Pages
{
    public sealed partial class ApplicationsSettings : Page
    {
        public ApplicationsSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProximitySensorSettingToggle.IsOn = SettingsManager.getProximitySensorOn();
        }

        private void ProximitySensorSettingToggle_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SettingsManager.setProximitySensorOn(((ToggleSwitch)e.OriginalSource).IsOn);
        }
    }
}
