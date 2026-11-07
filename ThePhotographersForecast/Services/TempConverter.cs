using static ThePhotographersForecast.Data.NwsSupplementalData;

namespace ThePhotographersForecast.Services
{
    public static class TempConverter
    {
        public static string Temp = (Preferences.Get("TemperatureUnit", "celsius") == "celsius") ? "C" : "F";
        public static double FToC(double temp)
        {
            double cTemp = 0;
            var globalTemp = Preferences.Get("TemperatureUnit", "celsius");
            if (globalTemp == "celsius")
            {
                cTemp = Math.Round((temp - 32) / 1.8, 0);
            }
            return cTemp;
        }

        public static double CToF(double temp)
        {
            double fTemp = 0;
            var globalTemp = Preferences.Get("TemperatureUnit", "fahrenheit");
            if (globalTemp == "fahrenheit")
            {
                fTemp = Math.Round(temp * 1.8 + 32, 0);
            }
            return fTemp;
        }

        public static double MphToKmh(double speed)
        {
            return Math.Round(speed*1.609344, 1);
        }
        public static double StoredToMs(double speed)
        {
            return Math.Round(speed / 2.23693629, 1);
        }
    }
}
