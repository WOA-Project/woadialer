using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Internal.Windows.Calls;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Devices.Haptics;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Calls.Background;

namespace WoADialer.Model
{
    internal static class MainEntities
    {
        public static bool Initialized { get; private set; }
        public static CallManager CallManager { get; private set; }
        public static PhoneLine DefaultLine { get; private set; }
        public static PhoneCallStore CallStore { get; private set; }
        public static PhoneCallHistoryStore CallHistoryStore { get; private set; }
        public static ProximitySensor ProximitySensor { get; private set; }
        
        public static async Task Initialize()
        {
            DeviceInformationCollection devices;
            CallManager = await CallManager.GetSystemPhoneCallManagerAsync();
            CallStore = await PhoneCallManager.RequestStoreAsync();
            Windows.ApplicationModel.Calls.Provider.PhoneCallOriginManager.ShowPhoneCallOriginSettingsUI();
            CallHistoryStore = await PhoneCallHistoryManager.RequestStoreAsync(PhoneCallHistoryStoreAccessType.AllEntriesReadWrite);
            devices = await DeviceInformation.FindAllAsync(ProximitySensor.GetDeviceSelector());
            ProximitySensor = devices.Count > 0 ? ProximitySensor.FromId(devices.First().Id) : null;
            try
            {
                //DefaultLine = await PhoneLine.FromIdAsync(await CallStore.GetDefaultLineAsync());
            }
            catch
            {

            }
            Initialized = true;
            RegisterBackgroudTask();
        }

        public static void RegisterBackgroudTask()
        {
            var taskRegistered = false;
            var exampleTaskName = "BackgroungCallMonitor";
            BackgroundExecutionManager.RequestAccessAsync();
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    task.Value.Unregister(true);
                    //taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "WoADialer.Model.BackgroungCallMonitor";
                builder.SetTrigger(new PhoneTrigger(PhoneTriggerType.CallHistoryChanged, false));
                BackgroundTaskRegistration task = builder.Register();
            }
        }
    }
}
