using Dialer.Systems;
using Dialer.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

            LoadingGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;

            Task.Run(() => Aaa()); //TODO: This still hangs the UI in some cases 🥲 
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

        private async void LoadDataCompleted()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ContactsItemsControl.ItemsSource = null;
                _contactControls = ContactSystem.ContactControls;
                ContactsItemsControl.ItemsSource = _contactControls;
                LoadingGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            });
        }

        private void ContactsPage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            viScrollbar.FixSpacing();
        }

        public void NavigateToLetter(string letter)
        {
            ScrollLetterHint.Text = letter;
            if(ScrollLetterGrid.Visibility == Windows.UI.Xaml.Visibility.Collapsed) ScrollLetterHintShow.Begin();
            ScrollLetterGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (_hideHintTimer != null)
            {
                _hideHintTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _hideHintTimer.Dispose();
                _hideHintTimer = null;
            }

            IEnumerable<ContactControl> ContactsWithLetter = from contact in _contactControls where contact.ContactName.ToUpper().StartsWith(letter) select contact;

            try
            {
                ContactsWithLetter.First().StartBringIntoView();
            } catch {
                //TODO: Fix for missing letter -> move to previous/next letter
            }

            _hideHintTimer = new Timer(async (state) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    ScrollLetterHintHide.Begin();
                    ScrollLetterHintHide.Completed += async (object sender, object e) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => ScrollLetterGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed);

                        if (_hideHintTimer != null)
                        {
                            _hideHintTimer.Change(Timeout.Infinite, Timeout.Infinite);
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
            _contactControls.Remove(cc);
        }
    }
}
