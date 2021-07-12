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

namespace Dialer.UI.Controls
{
    public sealed partial class VerticalIndexScrollbar : UserControl
    {
        public VerticalIndexScrollbar()
        {
            InitializeComponent();
        }

        public void FixSpacing()
        {
            LettersStackPanel.Spacing = (LettersStackPanel.ActualHeight / 26d) - 21d;
        }
    }
}
