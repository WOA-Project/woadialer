using Internal.Windows.Calls;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoADialer.UI.Controls
{
    public sealed partial class CallStatePresenter : UserControl
    {
        public static readonly DependencyProperty CallStateProperty = DependencyProperty.RegisterAttached("CallState", typeof(CallState), typeof(CallStatePresenter), new PropertyMetadata(CallState.Indeterminate));

        public CallState CallState
        {
            get => (CallState)GetValue(CallStateProperty);
            set => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetValue(CallStateProperty, value));
        }

        public CallStatePresenter()
        {
            this.InitializeComponent();
        }
    }
}
