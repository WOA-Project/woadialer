using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Calls;
using Windows.Foundation;
using Windows.System;

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
                        case string celluarID when celluarID.StartsWith(nameof(PhoneLineTransport.Cellular)):
                            celluarID = celluarID.Split('|')[1];
                            return App.Current.CallSystem.DisplayableLines.First(x => x.Line.Id == Guid.Parse(celluarID)).DisplayName;
                        case string appID when appID.StartsWith(nameof(PhoneLineTransport.VoipApp)):
                            appID = appID.Split('|')[1];
                            IAsyncOperation<IList<AppDiagnosticInfo>> task1 = AppDiagnosticInfo.RequestInfoForPackageAsync(appID);
                            task1.AsTask().Wait();
                            return task1.GetResults()[0].AppInfo.DisplayInfo.DisplayName;
                        case string deviceID when deviceID.StartsWith(nameof(PhoneLineTransport.Bluetooth)):
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
