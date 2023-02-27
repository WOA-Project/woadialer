using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Calls;

namespace Dialer.UI.Controls
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
            InitializeComponent();
        }
    }
}
