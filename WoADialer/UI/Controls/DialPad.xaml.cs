using System;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace WoADialer.UI.Controls
{
    public sealed partial class DialPad : UserControl
    {
        public event EventHandler<char> DigitTapped;

        public DialPad()
        {
            this.InitializeComponent();
            DigitButton[] digitbuttons = { NumPad_0, NumPad_1, NumPad_2, NumPad_3, NumPad_4, NumPad_5, NumPad_6, NumPad_7, NumPad_8, NumPad_9, NumPad_A, NumPad_S };
            foreach (DigitButton digitbutton in digitbuttons)
            {
                digitbutton.AddHandler(PointerPressedEvent, new PointerEventHandler(DigitButton_PointerPressed), true);
            }
            NumPad_0.AddHandler(RightTappedEvent, new RightTappedEventHandler(DigitButton_RightTapped), true);
        }

        private void DigitButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is DigitButton button)
            {
                switch (button.Digit)
                {
                    case "0":
                        if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                        {
                            DigitTapped?.Invoke(this, '+');
                        }
                        break;
                }
            }
        }

        private void DigitButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is DigitButton button)
            {
                PointerPoint pp = e.GetCurrentPoint(button);
                if (pp.Properties.IsRightButtonPressed == true && button.Digit == "0") DigitTapped?.Invoke(this, '+');
                else DigitTapped?.Invoke(this, button.Digit[0]);
            }
        }
    }
}
