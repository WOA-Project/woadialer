using Dialer.Systems;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using Windows.ApplicationModel.Calls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Dialer.UI.Controls
{
    public sealed partial class ContactControl : UserControl
    {
        private List<Tuple<string, string>> additionalPhoneContacts;
        private ObservableCollection<AdditionalPhoneContactPresenter> additionalPhoneContactPresenters;

        public string ContactName
        {
            get => ContactNameTB.Text;
            set => ContactNameTB.Text = value;
        }

        public string ContactMainPhone
        {
            get => ContactMainPhoneTB.Text;
            set => ContactMainPhoneTB.Text = value;
        }

        public List<Tuple<string, string>> AdditionalContactPhones
        {
            get => additionalPhoneContacts;
            set
            {
                additionalPhoneContacts = value;
                foreach (Tuple<string, string> additionalPhone in value)
                {
                    AdditionalPhoneContactPresenter apcp = new AdditionalPhoneContactPresenter();
                    apcp.PhoneType = additionalPhone.Item1;
                    apcp.PhoneNumber = additionalPhone.Item2;
                    additionalPhoneContactPresenters.Add(apcp);
                }
            }
        }

        public IRandomAccessStreamReference ContactPicture
        {
            set 
            {
                try
                {
                    ContactImage.ImageSource = new BitmapImage(new Uri(((StorageFile)value).Path));
                } catch
                {
                    ContactImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets//NoContactIcon.png"));
                }
            }
        }

        public ContactControl()
        {
            InitializeComponent();
            additionalPhoneContactPresenters = new ObservableCollection<AdditionalPhoneContactPresenter>();
        }

        private void MainCallButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //TODO: Check for missing phone lines. If no phone lines, show an alert
            try
            {
                App.Current.CallSystem.DefaultLine?.DialWithOptions(new PhoneDialOptions() { Number = ContactMainPhone.ToString() });
            }
            catch { }
        }
    }
}
