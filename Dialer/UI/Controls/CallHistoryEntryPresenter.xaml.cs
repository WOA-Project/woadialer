using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            this.InitializeComponent();
        }
    }
}
