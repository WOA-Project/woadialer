using Dialer.Systems;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Calls;

namespace Dialer.UI.Pages
{
    public sealed partial class HistoryPage : Page
    {
        private CallSystem CallSystem => App.Current.CallSystem;

        public HistoryPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PhoneCallHistoryEntry entry)
            {
                switch (entry.Address.RawAddressKind)
                {
                    case PhoneCallHistoryEntryRawAddressKind.PhoneNumber:
                        ((App.Window.Content as Frame)?.Content as MainPage)?.Navigate(UISystem.DIAL_PAGE, new EntranceNavigationTransitionInfo(), entry.Address.RawAddress);
                        break;
                }
            }
        }
    }
}
