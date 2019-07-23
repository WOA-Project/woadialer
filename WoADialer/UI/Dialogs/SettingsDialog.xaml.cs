using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WoADialer.Helpers;

namespace WoADialer.UI.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            InitializeComponent();

            if (SettingsManager.getDialPadSize() == "Tall") tallDialPadRadio.IsChecked = true;
            else if (SettingsManager.getDialPadSize() == "Medium") mediumDialPadRadio.IsChecked = true;
            else if (SettingsManager.getDialPadSize() == "Short") shortDialPadRadio.IsChecked = true;

            if (SettingsManager.getNumberFormatting() == "None") formattingNoneComboOption.IsSelected = true;
            else if (SettingsManager.getNumberFormatting() == "Italian") formattingItalianComboOption.IsSelected = true;
            else if (SettingsManager.getNumberFormatting() == "American") formattingAmericanComboOption.IsSelected = true;
        }

        private ICommand _closeDialogCommand;
        public ICommand CloseDialogCommand
        {
            get
            {
                if (_closeDialogCommand == null)
                {
                    _closeDialogCommand = new RelayCommand(
                        () =>
                        {
                            Hide();
                        });
                }
                return _closeDialogCommand;
            }
        }

        private ICommand _closeAndSaveDialogCommand;
        public ICommand CloseAndSaveDialogCommand
        {
            get
            {
                if (_closeAndSaveDialogCommand == null)
                {
                    _closeAndSaveDialogCommand = new RelayCommand(
                        () =>
                        {
                            //save settings
                            Hide();
                        });
                }
                return _closeAndSaveDialogCommand;
            }
        }

    }
}
