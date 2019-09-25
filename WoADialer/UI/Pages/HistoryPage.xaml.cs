using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Calls;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
    }
}
