using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Haptics;
using Windows.Devices.Sensors;

namespace Dialer.Systems
{
    public sealed class DeviceSystem
    {
        private ProximitySensorDisplayOnOffController DisplayController;

        public ProximitySensor ProximitySensor
        {
            get; private set;
        }
        public VibrationDevice VibrationDevice
        {
            get; private set;
        }
        public bool IsDisplayControlledByProximitySensor
        {
            get => DisplayController != null;
            set
            {
                if (value && DisplayController == null)
                {
                    DisplayController = ProximitySensor?.CreateDisplayOnOffController();
                }
                else if (!value && DisplayController != null)
                {
                    DisplayController.Dispose();
                    DisplayController = null;
                }
            }
        }

        public DeviceSystem()
        {
        }

        public async Task Initialize()
        {
            try
            {
                if (App.Current.PermissionSystem.Vibration == VibrationAccessStatus.Allowed)
                {
                    VibrationDevice = await VibrationDevice.GetDefaultAsync();
                }
                DeviceInformation proximityDevice = (await DeviceInformation.FindAllAsync(ProximitySensor.GetDeviceSelector())).FirstOrDefault();
                if (proximityDevice != null)
                {
                    ProximitySensor = ProximitySensor.FromId(proximityDevice.Id);
                }
            }
            catch
            {
            }
        }
    }
}
