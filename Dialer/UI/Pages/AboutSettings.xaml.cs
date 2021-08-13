using Dialer.UI.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Dialer.UI.Pages
{
    public sealed partial class AboutSettings : Page
    {
        public AboutViewModel ViewModel { get; }

        public AboutSettings()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default;
            this.InitializeComponent();
            ViewModel = new AboutViewModel(Dispatcher);
        }
    }
}
