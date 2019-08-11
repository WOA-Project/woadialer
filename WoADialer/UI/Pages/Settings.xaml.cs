using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WoADialer.UI.Pages
{
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
