using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace WoADialer.Background
{
    public static class NotificationHelper
    {
        public static async void RemoveSystemCallNotification()
        {
            UserNotificationListener listener = UserNotificationListener.Current;
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);
            UserNotification f = userNotifications.FirstOrDefault(x => (x.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric)?.GetTextElements()?.FirstOrDefault()?.Text ?? "") == "Tele2");
            if (f != null)
            {
                listener.RemoveNotification(f.Id);
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
                            Text = $"modelId: {f.AppInfo?.AppUserModelId}, id: {f.AppInfo?.Id}"
                        }
                    }
                    }
                };
                ToastContent toastContent = new ToastContent()
                {
                    Visual = toast,
                    Scenario = ToastScenario.IncomingCall
                };
                ToastNotification notification = new ToastNotification(toastContent.GetXml());
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }
        }
    }
}
