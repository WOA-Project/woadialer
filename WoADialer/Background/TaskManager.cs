using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Calls.Background;
using Windows.UI.Notifications;

namespace WoADialer.Background
{
    public static class TaskManager
    {
        public const string USER_NOTIFICATION_CHANGED = "UserNotificationChanged";
        public const string LINE_STATE_CHANGED = "LineStateChanged";

        public static async Task RegisterBackgroudTasks()
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            foreach (var task3 in BackgroundTaskRegistration.AllTasks)
            {
                task3.Value.Unregister(true);
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = LINE_STATE_CHANGED;
            builder.SetTrigger(new PhoneTrigger(PhoneTriggerType.LineChanged, false));
            BackgroundTaskRegistration task = builder.Register();

            builder = new BackgroundTaskBuilder();

            builder.Name = USER_NOTIFICATION_CHANGED;
            builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
            task = builder.Register();
        }
    }
}
