using Dialer.UI.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dialer.UI.Pages
{
    public sealed partial class AboutSettings : Page
    {
        public AboutViewModel ViewModel
        {
            get;
        }

        public AboutSettings()
        {
            RequestedTheme = (App.Window.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default;
            InitializeComponent();
            ViewModel = new AboutViewModel(DispatcherQueue);
        }
    }
}
