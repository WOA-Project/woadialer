using Windows.Storage;

namespace Dialer.Helpers
{
    class SettingsManager
    {
        public static bool isFirstTimeRun()
        {
            if (getNumberFormatting() == "" || getNumberFormatting() == null || getNumberFormatting() == string.Empty) return true;
            else return false;
        }

        public static void setDefaultSettings()
        {
            setDialPadSize("Tall");
            setNumberFormatting("None");
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
    }
}
