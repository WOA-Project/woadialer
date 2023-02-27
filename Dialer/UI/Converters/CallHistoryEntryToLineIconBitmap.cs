using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;

namespace Dialer.UI.Converters
{
    public sealed class CallHistoryEntryToLineIconBitmap : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PhoneCallHistoryEntry entry)
            {
                try
                {
                    IRandomAccessStream stream;
                    switch (entry.RemoteId)
                    {
                        case string celluarID when celluarID.StartsWith(nameof(PhoneLineTransport.Cellular)):
                            celluarID = celluarID.Split('|')[1];
                            IAsyncOperation<PhoneLine> task0 = PhoneLine.FromIdAsync(Guid.Parse(celluarID));
                            task0.AsTask().Wait();
                            IAsyncOperation<IReadOnlyList<AppListEntry>> task3 = Package.Current.GetAppListEntriesAsync();
                            task3.AsTask().Wait();
                            IAsyncOperation<IRandomAccessStreamWithContentType> task2 = task3.GetResults()[0].DisplayInfo.GetLogo(new Size(512, 512)).OpenReadAsync();
                            task2.AsTask().Wait();
                            stream = task2.GetResults();
                            break;
                        case string appID when appID.StartsWith(nameof(PhoneLineTransport.VoipApp)):
                            appID = appID.Split('|')[1];
                            IAsyncOperation<IList<AppDiagnosticInfo>> task1 = AppDiagnosticInfo.RequestInfoForPackageAsync(appID);
                            task1.AsTask().Wait();
                            task2 = task1.GetResults()[0].AppInfo.DisplayInfo.GetLogo(new Size(512, 512)).OpenReadAsync();
                            task2.AsTask().Wait();
                            stream = task2.GetResults();
                            break;
                        case string deviceID when deviceID.StartsWith(nameof(PhoneLineTransport.Bluetooth)):
                        //deviceID = deviceID.Split('|')[1];
                        //task0 = PhoneLine.FromIdAsync(Guid.Parse(deviceID));
                        //task0.AsTask().Wait();
                        //IAsyncOperation<DeviceInformation> task3 = DeviceInformation.CreateFromIdAsync(task0.GetResults().TransportDeviceId);
                        //task3.AsTask().Wait();
                        //IAsyncOperation<DeviceThumbnail> task4 = task3.GetResults().GetThumbnailAsync();
                        //task4.AsTask().Wait();
                        //stream = task4.GetResults();
                        //break;
                        default:
                            return null;
                    }
                    BitmapImage a = new();
                    a.SetSource(stream);
                    return a;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
