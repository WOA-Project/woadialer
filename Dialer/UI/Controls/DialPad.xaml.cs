using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace Dialer.UI.Controls
{
    public sealed partial class DialPad : UserControl
    {
        public event EventHandler<char> DigitTapped;

        public DialPad()
        {
            InitializeComponent();
            DigitButton[] digitbuttons = { NumPad_0, NumPad_1, NumPad_2, NumPad_3, NumPad_4, NumPad_5, NumPad_6, NumPad_7, NumPad_8, NumPad_9, NumPad_A, NumPad_S };
            foreach (DigitButton digitbutton in digitbuttons)
            {
                digitbutton.Click += Digitbutton_Click;
            }
            NumPad_0.RightTapped += DigitButton_RightTapped;
        }

        private void Digitbutton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is DigitButton button)
            {
                DigitTapped?.Invoke(this, button.Digit[0]);
            }
        }

        private void DigitButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is DigitButton button)
            {
                switch (button.Digit)
                {
                    case "0":
                        DigitTapped?.Invoke(this, '+');
                        break;
                }
            }
        }
    }
}
