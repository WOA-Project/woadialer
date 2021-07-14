using Dialer.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Windows.ApplicationModel.Contacts;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Dialer.UI.Pages
{

    public sealed partial class ContactsPage : Page
    {

        ObservableCollection<ContactControl> ContactControls;

        public static ContactsPage CurrentInstance;

        private Timer HideHintTimer;

        public ContactsPage()
        {
            InitializeComponent();
            ContactControls = new ObservableCollection<ContactControl>();

            CurrentInstance = this;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SizeChanged += ContactsPage_SizeChanged;

            //TODO: Improve loading by showing a percentage or number of contacts loaded?
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
                    //TODO: Improve loading by showing a percentage or number of contacts loaded?
                    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => {
                    //    LoadingGridProgressCount.Text = "(" + ContactControls.Count + " out of " + contacts.Count + ")";
                    //});
                    ContactControl cc = new ContactControl();
                    cc.ContactName = contact.DisplayName;
                    if (contact.Phones.Count == 0) continue;
                    cc.ContactMainPhone = contact.Phones[0].Number;
                    List<Tuple<string, string>> additionalPhones = new List<Tuple<string, string>>();
                    foreach (ContactPhone contactPhone in contact.Phones)
                    {
                        additionalPhones.Add(new Tuple<string, string>(contactPhone.Kind.ToString(), contactPhone.Number));
                    }
                    cc.AdditionalContactPhones = additionalPhones;
                    if(contact.SmallDisplayPicture != null) 
                        //TODO: Fix wrong cast
                        cc.ContactPicture = contact.SmallDisplayPicture;
                    ContactControls.Add(cc);
                }
            }

            LoadingGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void ContactsPage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            viScrollbar.FixSpacing();
        }

        public void NavigateToLetter(string letter)
        {
            IEnumerable<ContactControl> ContactsWithLetter = from contact in ContactControls where contact.ContactName.ToUpper().StartsWith(letter) select contact;

            ScrollLetterHint.Text = letter;
            if(ScrollLetterGrid.Visibility == Windows.UI.Xaml.Visibility.Collapsed) ScrollLetterHintShow.Begin();
            ScrollLetterGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (HideHintTimer != null)
            {
                HideHintTimer.Stop();
                HideHintTimer.Dispose();
            }
            HideHintTimer = new Timer(1000);
            HideHintTimer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    ScrollLetterHintHide.Begin();
                    ScrollLetterHintHide.Completed += async (object sender, object e) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            ScrollLetterGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        });
                    };
                });
            };
            HideHintTimer.Start();

            //TODO: Fix for missing letter -> move to previous/next letter
            try
            {
                ContactsWithLetter.First().StartBringIntoView();
            } catch { }
            Debug.WriteLine("Got request to navigate to letter " + letter);
        }

        private void ScrollLetterHintHide_Completed(object sender, object e)
        {
            throw new NotImplementedException();
        }
    }
}
