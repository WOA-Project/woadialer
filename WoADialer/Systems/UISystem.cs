using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WoADialer.UI.Pages;

namespace WoADialer.Systems
{
    public sealed class UISystem
    {
        public const string CALL_HISTORY_PAGE = "CallHistoryPage";
        public const string CALL_UI_PAGE = "CallUIPage";
        public const string CONTACTS_PAGE = "ContactsPage";
        public const string DIAL_PAGE = "DialPage";

        public static Type PageNameToType(string name)
        {
            switch(name)
            {
                case CALL_HISTORY_PAGE:
                    return typeof(HistoryPage);
                case CALL_UI_PAGE:
                    return typeof(CallUIPage);
                case CONTACTS_PAGE:
                    return typeof(ContactsPage);
                case DIAL_PAGE:
                    return typeof(DialPage);
                default:
                    return null;
            }
        }

        private readonly ObservableCollection<string> _MainPagePages;
        private CoreDispatcher Dispatcher;

        public ReadOnlyObservableCollection<string> MainPagePages { get; }

        public UISystem()
        {
            _MainPagePages = new ObservableCollection<string>()
            {
                CALL_HISTORY_PAGE,
                CONTACTS_PAGE,
                DIAL_PAGE
            };
            MainPagePages = new ReadOnlyObservableCollection<string>(_MainPagePages);
        }

        private async void CallManager_ActiveCallChanged(CallManager sender, Call args)
        {
            if (args != null)
            {
                if (!_MainPagePages.Contains(CALL_UI_PAGE))
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _MainPagePages.Add(CALL_UI_PAGE));
                }
                switch(args.State)
                {
                    case CallState.Incoming:
                    case CallState.Dialing:

                        break;
                }
            }
            else if (_MainPagePages.Contains(CALL_UI_PAGE))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _MainPagePages.Remove(CALL_UI_PAGE));
            }
        }

        public void Initializate(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            App.Current.CallSystem.CallManager.ActiveCallChanged += CallManager_ActiveCallChanged;
        }
    }
}
