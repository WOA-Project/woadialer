using Dialer.UI.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool TrackingPointer = false;

        public VerticalIndexScrollbar()
        {
            InitializeComponent();
        }

        public void FixSpacing()
        {
            LettersStackPanel.Spacing = (LettersStackPanel.ActualHeight / 26d) - 21d;
        }

        private void Letter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //ContactsPage.CurrentInstance?.NavigateToLetter((sender as TextBlock).Text);
        }

        private void LettersStackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("Pointer entered at " + e.GetCurrentPoint(sender as UIElement).Position);
            TrackingPointer = true;
            double h = e.GetCurrentPoint(sender as UIElement).Position.Y;
            //Calculate letter based on point position
            int index = (int)Math.Floor(h / (referenceTB.ActualHeight + LettersStackPanel.Spacing));
            //Debug.WriteLine("Pointer should be in letter " + Convert.ToChar(64 + index));
            ContactsPage.CurrentInstance?.NavigateToLetter(Convert.ToChar(64 + index).ToString());
        }

        private void LettersStackPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if(TrackingPointer)
            {
                //Debug.WriteLine("Pointer moved at " + e.GetCurrentPoint(sender as UIElement).Position);
                double h = e.GetCurrentPoint(sender as UIElement).Position.Y;
                //Calculate letter based on point position
                int index = (int)Math.Floor(h / (referenceTB.ActualHeight + LettersStackPanel.Spacing));
                //Debug.WriteLine("Pointer should be in letter " + Convert.ToChar(64 + index));
                ContactsPage.CurrentInstance?.NavigateToLetter(Convert.ToChar(64 + index).ToString());
            }
        }

        private void LettersStackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("Pointer exited at " + e.GetCurrentPoint(sender as UIElement).Position);
            TrackingPointer = false;
            double h = e.GetCurrentPoint(sender as UIElement).Position.Y;
            //Calculate letter based on point position
            int index = (int)Math.Floor(h / (referenceTB.ActualHeight + LettersStackPanel.Spacing));
            //Debug.WriteLine("Pointer should be in letter " + Convert.ToChar(64 + index));
            ContactsPage.CurrentInstance?.NavigateToLetter(Convert.ToChar(64 + index).ToString());
        }
    }
}
