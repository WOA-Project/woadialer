using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Haptics;
using Windows.Devices.Sensors;

namespace WoADialer.Background
{
    public sealed class DeviceSystem
    {
        private ProximitySensorDisplayOnOffController DisplayController;

        public ProximitySensor ProximitySensor { get; private set; }
        public VibrationDevice VibrationDevice { get; private set; }
        public bool IsDisplayControlledByProximitySensor
        {
            get => DisplayController != null;
            set
            {
                if (value && DisplayController == null)
                {
                    DisplayController = ProximitySensor.CreateDisplayOnOffController();
                }
                else if (!value && DisplayController != null)
                {
                    DisplayController.Dispose();
                    DisplayController = null;
                }
            }
        }

        public DeviceSystem() { }

        public async Task Initializate()
        {
            try
            {
                VibrationDevice = await VibrationDevice.GetDefaultAsync();
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
