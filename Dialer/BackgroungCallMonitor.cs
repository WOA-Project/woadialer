using System.Threading;
using Windows.ApplicationModel.Background;

namespace Dialer
{
    public sealed class BackgroungCallMonitor : IBackgroundTask
    {
        private BackgroundTaskDeferral _Deferral;
        private readonly ManualResetEvent a = new(false);

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();
            _ = a.WaitOne();
            _Deferral.Complete();
        }
    }
}
