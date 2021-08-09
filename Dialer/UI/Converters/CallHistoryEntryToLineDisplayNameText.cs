using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Calls;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml.Data;

namespace Dialer.UI.Converters
{
    public sealed class CallHistoryEntryToLineDisplayNameText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PhoneCallHistoryEntry entry)
            {
                try
                {
                    switch (entry.RemoteId)
                    {
                        case string celluarID when celluarID.StartsWith(PhoneLineTransport.Cellular.ToString()):
                            celluarID = celluarID.Split('|')[1];
                            IAsyncOperation<PhoneLine> task0 = PhoneLine.FromIdAsync(Guid.Parse(celluarID));
                            task0.AsTask().Wait();
                            return task0.GetResults().NetworkName;
                        case string appID when appID.StartsWith(PhoneLineTransport.VoipApp.ToString()):
                            appID = appID.Split('|')[1];
                            IAsyncOperation<IList<AppDiagnosticInfo>> task1 = AppDiagnosticInfo.RequestInfoForPackageAsync(appID);
                            task1.AsTask().Wait();
                            return task1.GetResults().First().AppInfo.DisplayInfo.DisplayName;
                        case string deviceID when deviceID.StartsWith(PhoneLineTransport.Bluetooth.ToString()):
                        //deviceID = deviceID.Split('|')[1];
                        //IAsyncOperation<DeviceInformation> task2 = DeviceInformation.CreateFromIdAsync(deviceID);
                        //task2.AsTask().Wait();
                        //return task2.GetResults().Name;
                        default:
                            return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
