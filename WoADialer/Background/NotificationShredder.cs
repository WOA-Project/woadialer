using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace WoADialer.Background
{
    public sealed class NotificationShredder
    {
        private const string WINDOWS_SYSTEM_TOAST_CALLING = "Windows.SystemToast.Calling";

        public event EventHandler NotificationRemoved;

        public NotificationShredder()
        {

        }

        public async void RegisterListener()
        {
            UserNotificationListener.Current.NotificationChanged += Listener_NotificationChanged;
            IReadOnlyList<UserNotification> notifications = await UserNotificationListener.Current.GetNotificationsAsync(NotificationKinds.Toast);
            UserNotification callToast = notifications.FirstOrDefault(x => x.AppInfo.AppUserModelId == WINDOWS_SYSTEM_TOAST_CALLING);
            if (callToast != null)
            {
                UserNotificationListener.Current.RemoveNotification(callToast.Id);
                NotificationRemoved?.Invoke(this, null);
            }
        }

        public void UnregisterListener()
        {
            UserNotificationListener.Current.NotificationChanged -= Listener_NotificationChanged;
        }

        private void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            if (args.ChangeKind == UserNotificationChangedKind.Added)
            {
                UserNotification notification = sender.GetNotification(args.UserNotificationId);
                if (notification.AppInfo.AppUserModelId == WINDOWS_SYSTEM_TOAST_CALLING)
                {
                    sender.RemoveNotification(notification.Id);
                    NotificationRemoved?.Invoke(this, null);
                }
            }
        }
    }
}
