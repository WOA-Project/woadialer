using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class SmallCallPresenter : UserControl
    {
        public static readonly DependencyProperty PhoneCallProperty = DependencyProperty.RegisterAttached("PhoneCall", typeof(Call), typeof(SmallCallPresenter), new PropertyMetadata(null));

        public Call PhoneCall
        {
            get => (Call)GetValue(PhoneCallProperty);
            set => SetValue(PhoneCallProperty, value);
        }

        public SmallCallPresenter()
        {
            this.InitializeComponent();
        }
    }
}
