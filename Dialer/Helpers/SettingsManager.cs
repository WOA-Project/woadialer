using Windows.Storage;

namespace Dialer.Helpers
{
    internal static class SettingsManager
    {
        public static bool isFirstTimeRun()
        {
            return getNumberFormatting()?.Length == 0 || getNumberFormatting() == null || getNumberFormatting()?.Length == 0;
        }

        public static void setDefaultSettings()
        {
            setDialPadSize("Tall");
            setNumberFormatting("None");
            setProximitySensorOn(true);
        }

        public static string getNumberFormatting()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            string currentNumberFormatting = roamingSettings.Values["numberFormatting"] as string;
            return currentNumberFormatting;
        }

        public static string getDialPadSize()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            string currentDialPadSize = roamingSettings.Values["dialpadSize"] as string;
            return currentDialPadSize;
        }

        public static bool getProximitySensorOn()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values["proximitySensorOn"] == null)
            {
                setProximitySensorOn(true);
            }

            return (bool)roamingSettings.Values["proximitySensorOn"];
        }

        public static void setNumberFormatting(string newFormatting)
        {
            //maybe check if the newFormatting is a valid value
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["numberFormatting"] = newFormatting;
        }

        public static void setDialPadSize(string newDialPadSize)
        {
            //maybe check if the newDialPadSize is a valid value
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["dialpadSize"] = newDialPadSize;
        }

        public static void setProximitySensorOn(bool on)
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["proximitySensorOn"] = on;
        }
    }
}
