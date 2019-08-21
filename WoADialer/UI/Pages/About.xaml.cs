using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WoADialer.UI.Pages
{
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
