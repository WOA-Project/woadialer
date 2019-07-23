using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WoADialer.Helpers;

namespace WoADialer.UI.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            InitializeComponent();

            Debug.WriteLine(SettingsManager.getDialPadSize() + " - " + SettingsManager.getNumberFormatting());

            if (SettingsManager.getDialPadSize() == "Tall") tallDialPadRadio.IsChecked = true;
            else if (SettingsManager.getDialPadSize() == "Medium") mediumDialPadRadio.IsChecked = true;
            else if (SettingsManager.getDialPadSize() == "Short") shortDialPadRadio.IsChecked = true;

            if (SettingsManager.getNumberFormatting() == "None") numberFormattingCombobox.SelectedIndex = 0;
            else if (SettingsManager.getNumberFormatting() == "Italian") numberFormattingCombobox.SelectedIndex = 1;
            else if (SettingsManager.getNumberFormatting() == "Russian") numberFormattingCombobox.SelectedIndex = 2;
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
                            if (numberFormattingCombobox.SelectedIndex == 0) SettingsManager.setNumberFormatting("None");
                            else if (numberFormattingCombobox.SelectedIndex == 1) SettingsManager.setNumberFormatting("Italian");
                            else if (numberFormattingCombobox.SelectedIndex == 2) SettingsManager.setNumberFormatting("Russian");

                            if (shortDialPadRadio.IsChecked == true) SettingsManager.setDialPadSize("Short");
                            else if (mediumDialPadRadio.IsChecked == true) SettingsManager.setDialPadSize("Medium");
                            else if (tallDialPadRadio.IsChecked == true) SettingsManager.setDialPadSize("Tall");

                            Hide();
                        });
                }
                return _closeAndSaveDialogCommand;
            }
        }

    }
}
