using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.LockScreen;

using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Calls.Background;

namespace WoADialer.Model
{
    public sealed class CallWaiter
    {
        public event EventHandler<Call> CallAppeared;

        public CallWaiter()
        {

        }

        private void CallManager_CallAppeared(CallManager sender, Call args)
        {
            if (args.State == CallState.Incoming)
            {
                CallAppeared?.Invoke(this, args);
            }
        }

        public void RegisterListener()
        {
            MainEntities.CallManager.CallAppeared += CallManager_CallAppeared;
            Call call = MainEntities.CallManager.CurrentCalls.FirstOrDefault(x => x.State == CallState.Incoming);
            if (call?.State == CallState.Incoming)
            {
                CallAppeared?.Invoke(this, call);
            }
        }

        public void UnregisterListener()
        {
            MainEntities.CallManager.CallAppeared -= CallManager_CallAppeared;
        }
    }
}
