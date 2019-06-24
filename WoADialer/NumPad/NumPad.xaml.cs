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

namespace WoADialer.NumPad
{
    public sealed partial class NumPad : UserControl
    {
        public event EventHandler<char> DigitTapped;

        public NumPad()
        {
            this.InitializeComponent();
        }

        private void DigitButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is DigitButton button)
            {
                switch(button.Digit)
                {
                    case "0":
                        DigitTapped?.Invoke(this, '+');
                        break;
                }
            }
        }

        private void DigitButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is DigitButton button)
            {
                DigitTapped?.Invoke(this, button.Digit[0]);
            }
        }
    }
}
