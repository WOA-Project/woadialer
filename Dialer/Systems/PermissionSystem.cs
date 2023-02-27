using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Haptics;
using Windows.System;
using Windows.UI.Notifications.Management;

namespace Dialer.Systems
{
    public sealed class PermissionSystem
    {
        public VibrationAccessStatus? Vibration
        {
            get; private set;
        }
        public UserNotificationListenerAccessStatus? Notifications
        {
            get; private set;
        }
        public DiagnosticAccessStatus? Diagnostic
        {
            get; private set;
        }
        public bool? CallHistory
        {
            get; private set;
        }
        public bool? Contacts
        {
            get; private set;
        }
        public bool? Calling
        {
            get; private set;
        }
        public bool IsAllPermissionsObtained
        {
            get
            {
                bool result = true;
                result &= Notifications == UserNotificationListenerAccessStatus.Allowed;
                result &= Diagnostic == DiagnosticAccessStatus.Allowed;
                result &= CallHistory ?? false;
                result &= Contacts ?? false;
                result &= Calling ?? false;
                return result;
            }
        }

        public PermissionSystem()
        {
        }

        public async Task<bool> RequestAllPermissions()
        {
            bool result = true;
            result &= await RequestVibrationAccess();
            result &= await RequestNotificationsAccess();
            result &= await RequestDiagnosticAccess();
            result &= await RequestContactsAccess();
            result &= await RequestCallingAccess();
            result &= await RequestCallHistoryAccess();
            return result;
        }

        public async Task<bool> RequestVibrationAccess()
        {
            try
            {
                Vibration = await VibrationDevice.RequestAccessAsync();
            }
            catch
            {
                Vibration = null;
            }
            return Vibration.HasValue;
        }

        public async Task<bool> RequestNotificationsAccess()
        {
            try
            {
                Notifications = await UserNotificationListener.Current.RequestAccessAsync();
            }
            catch
            {
                Notifications = null;
            }
            return Notifications.HasValue;
        }

        public async Task<bool> RequestDiagnosticAccess()
        {
            try
            {
                Diagnostic = await AppDiagnosticInfo.RequestAccessAsync();
            }
            catch
            {
                Diagnostic = null;
            }
            return Diagnostic.HasValue;
        }

        public async Task<bool> RequestCallingAccess()
        {
            try
            {
                Calling = await PhoneCallManager.RequestStoreAsync() is not null;
            }
            catch
            {
                Calling = null;
            }
            return Calling.HasValue;
        }

        public async Task<bool> RequestCallHistoryAccess()
        {
            try
            {
                CallHistory = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite) is not null;
            }
            catch
            {
                CallHistory = null;
            }
            return CallHistory.HasValue;
        }

        public async Task<bool> RequestContactsAccess()
        {
            try
            {
                Contacts = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly) is not null;
            }
            catch
            {
                Contacts = null;
            }
            return Contacts.HasValue;
        }
    }
}
