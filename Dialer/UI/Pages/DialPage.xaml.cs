using Dialer.Systems;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Calls;
using Windows.Globalization.PhoneNumberFormatting;
using Windows.System;
using WinUIEx;

namespace Dialer.UI.Pages
{
    public sealed partial class DialPage : Page
    {
        private CallSystem CallSystem => App.Current.CallSystem;
        private StringBuilder Number;
        private DisplayableLine CurrentPhoneLine;

        private ObservableCollection<DisplayableLine> DisplayableLines => App.Current.CallSystem.DisplayableLines;

        private readonly AppWindow appWindow;

        public DialPage()
        {
            // Retrieve the window handle (HWND) of the current (XAML) WinUI 3 window.
            nint hWnd = HwndExtensions.GetActiveWindow();

            // Retrieve the WindowId that corresponds to hWnd.
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

            // Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
            appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            InitializeComponent();
            CallSystem.Lines.CollectionChanged += Lines_CollectionChanged;

            if (CallSystem.DefaultLine != null && DisplayableLines.Any(x => x.Line.Id == CallSystem.DefaultLine.Id))
            {
                CurrentPhoneLine = DisplayableLines.First(x => x.Line.Id == CallSystem.DefaultLine.Id);
            }

            PhoneLineSelector.SelectedItem = CurrentPhoneLine;
        }

        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _ = App.Window.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        {
                            bool SetAsDefault = DisplayableLines.Count == 0;
                            DisplayableLine itemToAdd = new(e.NewItems[0] as PhoneLine);
                            if (SetAsDefault)
                            {
                                CurrentPhoneLine = itemToAdd;
                            }

                            break;
                        }
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        {
                            DisplayableLine itemToRemove = DisplayableLines.First(x => x.Line.Id == (e.OldItems[0] as PhoneLine)?.Id);
                            if (CurrentPhoneLine == itemToRemove)
                            {
                                CurrentPhoneLine = DisplayableLines.Count > 0 ? DisplayableLines.First() : null;
                            }
                            break;
                        }
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        {
                            DisplayableLine itemToReplace = DisplayableLines.First(x => x.Line.Id == (e.OldItems[0] as PhoneLine)?.Id);
                            DisplayableLine itemToAdd = new(e.NewItems[0] as PhoneLine);
                            if (CurrentPhoneLine == itemToReplace)
                            {
                                CurrentPhoneLine = itemToAdd;
                            }
                            break;
                        }
                }
            });
        }

        private void DeleteLastNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (Number.Length > 0)
            {
                _ = Number.Remove(Number.Length - 1, 1);
            }
            UpdateCurrentNumber();
        }

        private void NumPad_DigitTapped(object sender, char e)
        {
            _ = Number.Append(e);
            UpdateCurrentNumber();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Number = e.Parameter switch
            {
                string number => new StringBuilder(number),
                _ => new StringBuilder(),
            };
            UpdateCurrentNumber();
        }

        private void UpdateCurrentNumber()
        {
            callButton.IsEnabled = !string.IsNullOrWhiteSpace(Number.ToString());
            PhoneNumberFormatter a = new();
            numberToDialBox.Text = a.FormatPartialString(Number.ToString());
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentPhoneLine?.Line?.DialWithOptions(new PhoneDialOptions() { Number = Number.ToString() });
            }
            catch (Exception ee)
            {
                handleException(ee);
            }
        }

        public async void handleException(Exception e)
        {
            _ = await new ContentDialog() { Title = "Unhandled Exception", Content = e.Message + "\n\n\n" + e.StackTrace, XamlRoot = XamlRoot, IsPrimaryButtonEnabled = true, PrimaryButtonText = "Ok" }.ShowAsync();
        }

        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Number0:
                case VirtualKey.NumberPad0:
                    NumPad_DigitTapped(this, '0');
                    break;
                case VirtualKey.Number1:
                case VirtualKey.NumberPad1:
                    NumPad_DigitTapped(this, '1');
                    break;
                case VirtualKey.Number2:
                case VirtualKey.NumberPad2:
                    NumPad_DigitTapped(this, '2');
                    break;
                case VirtualKey.Number3:
                case VirtualKey.NumberPad3:
                    NumPad_DigitTapped(this, '3');
                    break;
                case VirtualKey.Number4:
                case VirtualKey.NumberPad4:
                    NumPad_DigitTapped(this, '4');
                    break;
                case VirtualKey.Number5:
                case VirtualKey.NumberPad5:
                    NumPad_DigitTapped(this, '5');
                    break;
                case VirtualKey.Number6:
                case VirtualKey.NumberPad6:
                    NumPad_DigitTapped(this, '6');
                    break;
                case VirtualKey.Number7:
                case VirtualKey.NumberPad7:
                    NumPad_DigitTapped(this, '7');
                    break;
                case VirtualKey.Number8:
                case VirtualKey.NumberPad8:
                    NumPad_DigitTapped(this, '8');
                    break;
                case VirtualKey.Number9:
                case VirtualKey.NumberPad9:
                    NumPad_DigitTapped(this, '9');
                    break;
                case (VirtualKey)187:
                case VirtualKey.Add:
                    NumPad_DigitTapped(this, '+');
                    break;
                case VirtualKey.Back:
                    DeleteLastNumberButton_Click(this, null);
                    break;
            }
        }

        private void PhoneLineSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0 && e.AddedItems[0] != null)
            {
                CurrentPhoneLine = e.AddedItems[0] as DisplayableLine;
            }
            else if (DisplayableLines.Count > 0)
            {
                CurrentPhoneLine = DisplayableLines.First();
            }
            PhoneLineSelector.SelectedItem = CurrentPhoneLine;
        }
    }
}
