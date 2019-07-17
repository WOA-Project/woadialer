using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace WoADialer.Model
{
    public struct MovingCallInfo
    {
        public bool IsActive;
        public PhoneNumber Number;
        public Contact Contact;
    }
}
