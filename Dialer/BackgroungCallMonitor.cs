using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.LockScreen;

namespace WoADialer.Background
{
    public sealed class BackgroungCallMonitor : IBackgroundTask
    {
        BackgroundTaskDeferral _Deferral;
        ManualResetEvent a = new ManualResetEvent(false);

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();
            a.WaitOne();
            _Deferral.Complete();
        }
    }
}
