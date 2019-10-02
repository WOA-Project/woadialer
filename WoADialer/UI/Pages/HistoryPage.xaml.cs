using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Calls;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using WoADialer.Systems;
using WoADialer.UI.Controls;
using WoADialer.UI.Conventers;

namespace WoADialer.UI.Pages
{
    public sealed partial class HistoryPage : Page
    {
        private CallSystem CallSystem => App.Current.CallSystem;

        public HistoryPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PhoneCallHistoryEntry entry)
            {
                switch (entry.Address.RawAddressKind)
                {
                    case PhoneCallHistoryEntryRawAddressKind.PhoneNumber:
                        ((Window.Current.Content as Frame).Content as MainPage).Navigate(UISystem.DIAL_PAGE, new EntranceNavigationTransitionInfo(), entry.Address.RawAddress);
                        break;
                }
            }
        }
    }
}
