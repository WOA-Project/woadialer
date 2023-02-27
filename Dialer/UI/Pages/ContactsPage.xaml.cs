using Dialer.Systems;
using Dialer.UI.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dialer.UI.Pages
{
    public sealed partial class ContactsPage : Page
    {
        private Timer _hideHintTimer;
        private ObservableCollection<ContactControl> _contactControls;

        public static ContactsPage CurrentInstance;

        public ContactsPage()
        {
            InitializeComponent();

            _contactControls = new ObservableCollection<ContactControl>();
            CurrentInstance = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SizeChanged += ContactsPage_SizeChanged;

            LoadingGrid.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

            _ = Task.Run(Aaa); //TODO: This still hangs the UI in some cases 🥲 
        }

        private void Aaa()
        {
            if (ContactSystem.ContactControls == null)
            {
                ContactSystem.ContactsLoaded += (object sender, EventArgs e) => LoadDataCompleted();
            }
            else
            {
                LoadDataCompleted();
            }
        }

        private void LoadDataCompleted()
        {
            _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
            {
                ContactsItemsControl.ItemsSource = null;
                _contactControls = ContactSystem.ContactControls;
                ContactsItemsControl.ItemsSource = _contactControls;
                LoadingGrid.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
        }

        private void ContactsPage_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            viScrollbar.FixSpacing();
        }

        public void NavigateToLetter(string letter)
        {
            ScrollLetterHint.Text = letter;
            if (ScrollLetterGrid.Visibility == Microsoft.UI.Xaml.Visibility.Collapsed)
            {
                ScrollLetterHintShow.Begin();
            }

            ScrollLetterGrid.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            if (_hideHintTimer != null)
            {
                _ = _hideHintTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _hideHintTimer.Dispose();
                _hideHintTimer = null;
            }

            IEnumerable<ContactControl> ContactsWithLetter = from contact in _contactControls where contact.ContactName.ToUpper().StartsWith(letter) select contact;

            try
            {
                ContactsWithLetter.First().StartBringIntoView();
            }
            catch
            {
                //TODO: Fix for missing letter -> move to previous/next letter
            }

            _hideHintTimer = new Timer((state) =>
            {
                _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                {
                    ScrollLetterHintHide.Begin();
                    ScrollLetterHintHide.Completed += (object sender, object e) =>
                    {
                        _ = DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ScrollLetterGrid.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed);

                        if (_hideHintTimer != null)
                        {
                            _ = _hideHintTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            _hideHintTimer.Dispose();
                            _hideHintTimer = null;
                        }
                    };
                });
            }, null, 1000, 1000);

            Debug.WriteLine("Got request to navigate to letter " + letter);
        }

        public void RemoveContactControl(ContactControl cc)
        {
            _ = _contactControls.Remove(cc);
        }
    }
}
