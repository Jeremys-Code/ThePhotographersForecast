using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Modules
{
    public static class FeelsLikeEngine
    {
        // Lans P. Rothfusz and described in a 1990 National Weather Service (NWS) Technical Attachment (SR 90-23)

        private static string CalcCloudCoverage(double skyCover, int? isDaytime, bool? isDay)
        {
            string cloudConditions = string.Empty;
            if (isDay == null && isDaytime.HasValue)
            {
                if (isDaytime == 1) { isDay = true; } else { isDay = false; }
            }
            if (isDay.HasValue)
            {
                switch (skyCover)
                {
                    case < 12.5:
                        cloudConditions += (bool)isDay ? "sunny" : "clear sky";
                        break;
                    case < 37.5:
                        cloudConditions += (bool)isDay ? "mostly sunny" : "mostly clear";
                        break;
                    case < 62.5:
                        cloudConditions += (bool)isDay ? "partly sunny" : "partly cloudy";
                        break;
                    case < 87.5:
                        cloudConditions += "mostly cloudy";
                        break;
                    case <= 100:
                        cloudConditions += "cloudy";
                        break;
                    default:
                        break;
                }
            }
            return cloudConditions;
        }

        public static string TempCategory(double temp)
        {
            if (temp <= 30) return "freezing";
            if (temp <= 40) return "cold";
            if (temp <= 60) return "cool";
            if (temp <= 70) return "mild";
            if (temp <= 80) return "warm";
            if (temp <= 90) return "hot";
            if (temp > 90) return "extremely hot";
            return "unknown";
        }

        public static string CalcWhatAFeeling(double heatindex)
        {
            // Determine the Physiological Temperature Strain Index
            double ptsi;
            string confoIndex = string.Empty;
            var unit = Preferences.Get("TemperatureUnit", "fahrenheit");
            if (unit == "fahrenheit")
            {
                //Formula that takes into account Temp, Humidity, Dew Point, Energy Exterted
                ptsi = heatindex * 12;
                confoIndex = ptsi switch
                {
                    < 68 => "Comfortable, no sweat!",
                    >= 68 and < 86 => "noticeable warmth. Minimal or light sweat with exertion.",
                    >= 86 and < 104 => "uncomfortable. Moderate sweat to heavy sweat if exerting.",
                    >= 104 and <= 113 => "very uncomfortable. Avoidance of exertion advised. Heavy sweat regardless of the activity.",
                    > 113 => "RISK OF HEAT STROKE! Profuse sweating, high physiological strain.",
                    _ => "Comfortable, no sweat!"
                };
            }
            else
            {
                ptsi = heatindex * 6.5;
                confoIndex = ptsi switch
                {
                    < 20 => "comfortable, no sweat!",
                    >= 20 and < 30 => "noticeable warmth. Minimal or light sweat with exertion.",
                    >= 30 and < 40 => "uncomfortable. Moderate sweat to heavy sweat if exerting.",
                    >= 40 and <= 45 => "very uncomfortable. Avoidance of exertion advised. Heavy sweat regardless of the activity.",
                    > 45 => "RISK OF HEAT STROKE! Profuse sweating, high physiological strain.",
                    _ => "comfortable, no sweat!"
                };
            }
            return confoIndex;
        }

        private static string CalcWind(string windSpeed, double? wind)
        {
            double speed;

            // Prefer explicit wind value if provided
            if (wind is not null)
            {
                speed = wind.Value;
            }
            else if (!string.IsNullOrEmpty(windSpeed))
            {
                var speedResult = double.TryParse(
                    windSpeed.Replace("mph", "").Trim(),
                    out speed);

                if (!speedResult) return null;
            }
            else
            {
                return null;
            }

            // Beaufort scale classification (mph)
            return speed switch
            {
                >= 0 and <= 1 => "Calm",
                >= 1 and <= 3 => "Light Air",
                >= 4 and <= 7 => "Light Breeze",
                >= 8 and <= 12 => "Gentle Breeze",
                >= 13 and <= 18 => "Moderate Breeze",
                >= 19 and <= 24 => "Fresh Breeze",
                >= 25 and <= 31 => "Strong Breeze",
                >= 32 and <= 38 => "Near Gale",
                >= 39 and <= 46 => "Gale",
                >= 47 and <= 54 => "Severe Gale",
                >= 55 and <= 63 => "Storm",
                >= 64 and <= 72 => "Violent Storm",
                >= 73 => "Hurricane",
                _ => null
            };
        }

        private static string GustSpeed(double gustSpeed)
        {
            return gustSpeed switch
            {
                >= 0 and <= 3 => "No wind gusts to report.",
                >= 4 and <= 12 => "Gusts felt on face, leaves rustle.",
                >= 13 and <= 24 => "Gusts move small branches and raise loose dust.",
                >= 25 and <= 38 => "Gusts cause inconvenience; whole trees sway.",
                >= 39 and <= 54 => "Gusts break twigs and may cause noticeable damage.",
                >= 55 => "Destructive gusts capable of widespread damage.",
                _ => "No wind gusts to report."
            };
        }

        // Basic stats on a bin
        public static double Mean(Bin bin) =>
            bin.Values.Any() ? bin.Values.Average() : 0;

        public static double Min(Bin bin) =>
            bin.Values.Any() ? bin.Values.Min() : 0;

        public static double Max(Bin bin) =>
            bin.Values.Any() ? bin.Values.Max() : 0;

        // Trend between two bins
        public static string Trend(double previousMean, double currentMean)
        {
            double delta = currentMean - previousMean;

            if (delta >= 0.30) return "rapidly increasing";
            if (delta >= 0.15) return "increasing";
            if (delta <= -0.30) return "rapidly decreasing";
            if (delta <= -0.15) return "decreasing";
            return "remaining fairly steady";
        }
    }

}
