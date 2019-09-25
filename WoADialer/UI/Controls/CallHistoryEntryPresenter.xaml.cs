using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Calls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoADialer.UI.Controls
{
    public sealed partial class CallHistoryEntryPresenter : UserControl
    {
        public static readonly DependencyProperty PresentedEntryProperty = DependencyProperty.Register(nameof(PresentedEntry), typeof(PhoneCallHistoryEntry), typeof(CallHistoryEntryPresenter), new PropertyMetadata(null));

        public PhoneCallHistoryEntry PresentedEntry
        {
            get => (PhoneCallHistoryEntry)GetValue(PresentedEntryProperty);
            set => SetValue(PresentedEntryProperty, value);
        }

        public bool ContactPresented => !string.IsNullOrEmpty(PresentedEntry?.Address.ContactId);

        public CallHistoryEntryPresenter()
        {
            this.InitializeComponent();
        }
    }
}
