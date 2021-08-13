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
    internal static class ContactSystem
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
                return _contacts ?? null;
            }
        }

        public static ObservableCollection<ContactControl> ContactControls
        {
            get
            {
                return _contactControls ?? null;
            }
        }

        public static async void LoadContacts()
        {
            if (_contacts == null && !ContactsLoading)
            {
                try
                {
                    ContactsLoading = true;
                    _contactStore = await ContactManager.RequestStoreAsync();
                    ObservableCollection<Contact> t_contacts = new(await _contactStore.FindContactsAsync());

                    Debug.WriteLine("Found " + t_contacts.Count + " contacts");

                    ObservableCollection<ContactControl> t_contactControls = new();

                    foreach (Contact contact in t_contacts)
                    {
                        ContactControl cc = new();
                        cc.AssociatedContact = contact;
                        cc.ContactName = contact.DisplayName;
                        if (contact.Phones.Count == 0) continue;
                        cc.ContactMainPhone = contact.Phones[0].Number;
                        List<Tuple<string, string>> additionalPhones = new();
                        foreach (ContactPhone contactPhone in contact.Phones)
                        {
                            additionalPhones.Add(new Tuple<string, string>(contactPhone.Kind.ToString(), contactPhone.Number));
                        }
                        cc.AdditionalContactPhones = additionalPhones;
                        if (contact.SmallDisplayPicture != null)
                        {
                            //TODO: Fix wrong cast
                            cc.ContactPicture = contact.SmallDisplayPicture;
                        }

                        t_contactControls.Add(cc);
                    }
                    _contacts = t_contacts;
                    _contactControls = t_contactControls;
                }
                catch
                {
                }

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
