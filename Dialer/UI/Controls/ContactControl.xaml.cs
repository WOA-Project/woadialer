using Dialer.Systems;
using Dialer.UI.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Contacts;
using Windows.Storage.Streams;

namespace Dialer.UI.Controls
{
    public sealed partial class ContactControl : UserControl
    {
        private List<Tuple<string, string>> additionalPhoneContacts;
        private readonly ObservableCollection<AdditionalPhoneContactPresenter> additionalPhoneContactPresenters;

        public Contact AssociatedContact;

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
                    AdditionalPhoneContactPresenter apcp = new()
                    {
                        PhoneType = additionalPhone.Item1,
                        PhoneNumber = additionalPhone.Item2
                    };
                    additionalPhoneContactPresenters.Add(apcp);
                }
            }
        }

        private async void SetContactPicture(IRandomAccessStreamReference randomAccessStreamReference)
        {
            try
            {
                BitmapImage bitmapImage = new();
                using IRandomAccessStreamWithContentType valueStream = await randomAccessStreamReference.OpenReadAsync();
                bitmapImage.SetSource(valueStream);
                ContactImage.ImageSource = bitmapImage;
            }
            catch
            {
                ContactImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets//NoContactIcon.png"));
            }
        }

        public IRandomAccessStreamReference ContactPicture
        {
            set => SetContactPicture(value);
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
                App.Current.CallSystem.DefaultLine?.DialWithOptions(new PhoneDialOptions() { Number = ContactMainPhone });
            }
            catch { }
        }

        private async void FlyoutDeleteContact_Click(object sender, RoutedEventArgs e)
        {
            await ContactSystem.DeleteContact(AssociatedContact);
            ContactsPage.CurrentInstance.RemoveContactControl(this);
        }

        private void FlyoutCallContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Current.CallSystem.DefaultLine?.DialWithOptions(new PhoneDialOptions() { Number = ContactMainPhone });
            }
            catch { }
        }
    }
}
