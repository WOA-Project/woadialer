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
    public sealed class BackgroungCallMonitor : IBackgroundTask
    {
        BackgroundTaskDeferral _Deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();
            string text = "????";
            if (taskInstance.TriggerDetails is PhoneLineChangedTriggerDetails lineDetails)
            {
                text = "lineDetails";
            }
            ToastVisual toast = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = "TEST"
                        },

                        new AdaptiveText()
                        {
                            Text = text
                        }
                    }
                }
            };
            ToastContent toastContent = new ToastContent()
            {
                Visual = toast, Scenario = ToastScenario.IncomingCall
            };
            ToastNotification notification = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(notification);
            _Deferral.Complete();
        }
    }
}
