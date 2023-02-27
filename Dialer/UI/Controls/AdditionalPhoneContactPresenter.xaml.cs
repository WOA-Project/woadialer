using Microsoft.UI.Xaml.Controls;


namespace Dialer.UI.Controls
{
    public sealed partial class AdditionalPhoneContactPresenter : UserControl
    {
        public string PhoneType
        {
            get => PhoneTypeTB.Text;
            set => PhoneTypeTB.Text = value;
        }

        public string PhoneNumber
        {
            get => PhoneNumberTB.Text;
            set => PhoneNumberTB.Text = value;
        }

        public AdditionalPhoneContactPresenter()
        {
            InitializeComponent();
        }
    }
}
