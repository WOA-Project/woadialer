using Internal.Windows.Calls;
using Windows.ApplicationModel.Calls;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WoADialer.UI.ViewModel;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoADialer.UI.Controls
{
    public sealed partial class LinePresenter : UserControl
    {
        public static readonly DependencyProperty PresentedLineProperty = DependencyProperty.RegisterAttached("PresentedLine", typeof(PhoneLine), typeof(LinePresenter), new PropertyMetadata(null));

        public PhoneLine PresentedLine
        {
            get => (PhoneLine)GetValue(PresentedLineProperty);
            set => SetValue(PresentedLineProperty, value);
        }

        public LinePresenter()
        {
            this.InitializeComponent();
        }
    }
}
