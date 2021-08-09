using Dialer.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace Dialer.Systems
{
    class ContactSystem
    {
        private static ContactStore _contactStore;
        private static ObservableCollection<Contact> _contacts;
        private static ObservableCollection<ContactControl> _contactControls;

        public static event EventHandler ContactsLoaded;

        public static bool ContactsLoading = false;

        public static ObservableCollection<Contact> Contacts
        {
            get
            {
                if (_contacts != null) return _contacts;
                else return null;
            }
        }

        public static ObservableCollection<ContactControl> ContactControls
        {
            get
            {
                if (_contactControls != null) return _contactControls;
                else return null;
            }
        }

        public static async void LoadContacts()
        {
            if(_contacts == null && ContactsLoading == false)
            {
                ContactsLoading = true;
                _contactStore = await ContactManager.RequestStoreAsync();
                ObservableCollection<Contact> t_contacts = new ObservableCollection<Contact>(await _contactStore.FindContactsAsync());

                Debug.WriteLine("Found " + t_contacts.Count + " contacts");

                ObservableCollection<ContactControl> t_contactControls = new ObservableCollection<ContactControl>();

                foreach (Contact contact in t_contacts)
                {
                    ContactControl cc = new ContactControl();
                    cc.AssociatedContact = contact;
                    cc.ContactName = contact.DisplayName;
                    if (contact.Phones.Count == 0) continue;
                    cc.ContactMainPhone = contact.Phones[0].Number;
                    List<Tuple<string, string>> additionalPhones = new List<Tuple<string, string>>();
                    foreach (ContactPhone contactPhone in contact.Phones)
                    {
                        additionalPhones.Add(new Tuple<string, string>(contactPhone.Kind.ToString(), contactPhone.Number));
                    }
                    cc.AdditionalContactPhones = additionalPhones;
                    if (contact.SmallDisplayPicture != null)
                        //TODO: Fix wrong cast
                        cc.ContactPicture = contact.SmallDisplayPicture;
                    t_contactControls.Add(cc);
                }
                _contacts = t_contacts;
                _contactControls = t_contactControls;

                ContactsLoading = false;

                ContactsLoaded?.Invoke(null, EventArgs.Empty);
            }
        }

        public static async Task DeleteContact(Contact contact)
        {
            ContactStore cs = await ContactManager.RequestStoreAsync();
            ContactList cl = null;
            try
            {
                cl = await cs.GetContactListAsync(contact.ContactListId);
            }
            catch
            {
                IReadOnlyList<ContactList> contactlists = await cs.FindContactListsAsync();
                foreach (ContactList _cl in contactlists)
                {
                    try
                    {
                        if (_cl.GetContactAsync(contact.Id) != null) cl = _cl;
                    }
                    catch { }
                }
            }
            if (cl == null) return; //For some reason the correct contact list can't be retrieved. It should be in Contact.ContactListId, but...
            await cl.DeleteContactAsync(contact);
        }
    }
}
