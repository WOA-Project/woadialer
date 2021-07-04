using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WoADialer.UI.ViewModel;

namespace WoADialer.UI.Pages
{
    public sealed partial class AboutSettings : Page
    {
        public AboutViewModel ViewModel { get; }

        public AboutSettings()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            this.InitializeComponent();
            ViewModel = new AboutViewModel(Dispatcher);
        }
    }
}
