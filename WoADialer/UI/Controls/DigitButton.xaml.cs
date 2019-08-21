using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace WoADialer.UI.Controls
{
    public sealed partial class DigitButton : Button
    {
        public string Chars
        {
            get => tb_Chars.Text;
            set => tb_Chars.Text = value;
        }
        public string AdditionalChars
        {
            get => tb_AdditionalChars.Text;
            set => tb_AdditionalChars.Text = value;
        }
        public string Digit
        {
            get => tb_Digit.Text;
            set => tb_Digit.Text = value;
        }

        public DigitButton()
        {
            this.InitializeComponent();
        }
    }
}
