using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Dialer.UI.Controls
{
    public sealed partial class LinePresenter : UserControl
    {
        public static readonly DependencyProperty PresentedLineNameProperty = DependencyProperty.RegisterAttached(nameof(PresentedLineName), typeof(string), typeof(LinePresenter), new PropertyMetadata(null));
        public static readonly DependencyProperty PresentedLineIconSourceProperty = DependencyProperty.RegisterAttached(nameof(PresentedLineIconSource), typeof(ImageSource), typeof(LinePresenter), new PropertyMetadata(null));

        public string PresentedLineName
        {
            get => (string)GetValue(PresentedLineNameProperty);
            set => SetValue(PresentedLineNameProperty, value);
        }

        public ImageSource PresentedLineIconSource
        {
            get => (ImageSource)GetValue(PresentedLineIconSourceProperty);
            set => SetValue(PresentedLineIconSourceProperty, value);
        }

        public LinePresenter()
        {
            this.InitializeComponent();
        }
    }
}
