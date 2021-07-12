using Dialer.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Dialer.UI.Pages
{

    public sealed partial class ContactsPage : Page
    {

        ObservableCollection<ContactControl> ContactControls;

        public ContactsPage()
        {
            InitializeComponent();
            ContactControls = new ObservableCollection<ContactControl>();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SizeChanged += ContactsPage_SizeChanged;

            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => {
            //    LoadingGridProgressCount.Text = "";
            //});
            LoadingGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if(ContactControls.Count == 0)
            {
                ContactStore contactStore = await ContactManager.RequestStoreAsync();
                IReadOnlyList<Contact> contacts = await contactStore.FindContactsAsync();

                Debug.WriteLine("Found " + contacts.Count + " contacts");

                foreach (Contact contact in contacts)
                {
                    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => {
                    //    LoadingGridProgressCount.Text = "(" + ContactControls.Count + " out of " + contacts.Count + ")";
                    //});
                    ContactControl cc = new ContactControl();
                    cc.ContactName = contact.DisplayName;
                    if (contact.Phones.Count == 0) continue;
                    cc.ContactMainPhone = contact.Phones[0].Number;
                    try
                    {
                        if(contact.SmallDisplayPicture != null) cc.ContactPicture = contact.SmallDisplayPicture;
                    } catch { }
                    ContactControls.Add(cc);
                }
            }

            LoadingGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void ContactsPage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            viScrollbar.FixSpacing();
        }

        public void NavigateToLetter(char letter)
        {
            throw new NotImplementedException();
        }
    }
}
