using Microsoft.UI.Dispatching;
using Windows.ApplicationModel;

namespace Dialer.UI.ViewModel
{
    public class AboutViewModel : ViewModelCore
    {
        // Properties
        private string _appName;
        public string AppName
        {
            get => _appName;
            set => Set(ref _appName, value);
        }

        private string _versionNumber;
        public string VersionNumber
        {
            get => _versionNumber;
            set => Set(ref _versionNumber, value);
        }


        // Constructor
        public AboutViewModel(DispatcherQueue dispatcher) : base(dispatcher)
        {
            Initialize();
        }

        // Initialize Stuff
        public void Initialize()
        {
            AppName = GetAppName();
            VersionNumber = GetVersionNumber();
        }


        // Methods
        private string GetAppName()
        {
            Package package = Package.Current;
            string appName = package.DisplayName;

            return appName;
        }

        private string GetVersionNumber()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}