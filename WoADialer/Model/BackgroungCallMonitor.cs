using Internal.Windows.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.LockScreen;

namespace WoADialer.Model
{
    public sealed class BackgroungCallMonitor : IBackgroundTask
    {
        BackgroundTaskDeferral _Deferral;
        ManualResetEvent a = new ManualResetEvent(false);

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();
            await MainEntities.Initialize();
            MainEntities.CallManager.CallAppeared += CallManager_CallAppeared;
            a.WaitOne();
            _Deferral.Complete();
        }

        private void CallManager_CallAppeared(CallManager sender, Call args)
        {
            if (args.State == CallState.Incoming)
            {
                
            }
        }
    }
}
