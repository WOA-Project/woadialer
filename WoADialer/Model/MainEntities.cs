using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Internal.Windows.Calls;

namespace WoADialer.Model
{
    internal static class MainEntities
    {
        public static bool Initialized { get; private set; }
        public static SystemPhoneCallManager API { get; private set; }
        public static PhoneLine DefaultLine { get; private set; }
        public static PhoneCallStore CallStore { get; private set; }

        public static async Task Initialize()
        {
            API = await SystemPhoneCallManager.GetSystemPhoneCallManagerAsync();
            CallStore = await PhoneCallManager.RequestStoreAsync();
            try
            {
                DefaultLine = await PhoneLine.FromIdAsync(await CallStore.GetDefaultLineAsync());
            }
            catch
            {

            }
            Initialized = true;
        }
    }
}
