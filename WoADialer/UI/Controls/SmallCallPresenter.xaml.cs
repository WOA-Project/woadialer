using Internal.Windows.Calls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WoADialer.UI.ViewModel;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoADialer.UI.Controls
{
    public sealed partial class SmallCallPresenter : UserControl
    {
        public static readonly DependencyProperty PresentedCallProperty = DependencyProperty.RegisterAttached("PresentedCall", typeof(CallViewModel), typeof(CallStatePresenter), new PropertyMetadata(null));

        public CallViewModel PresentedCall
        {
            get => (CallViewModel)GetValue(PresentedCallProperty);
            set => SetValue(PresentedCallProperty, value);
        }

        public SmallCallPresenter()
        {
            this.InitializeComponent();
        }
    }
}
