using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.LockScreen;

namespace WoADialer.Model
{
    public sealed class BackgroungCallMonitor : IBackgroundTask
    {
        BackgroundTaskDeferral _Deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();
            
            _Deferral.Complete();
        }
    }
}
