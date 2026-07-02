using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public class WmoLookup
    {

        public static List<WmoCode> WmoCodesList { get; set; }

        private static async Task ImportWmoCodes()
        {
            WmoCodesList = new List<WmoCode>();
            using var stream = await FileSystem.OpenAppPackageFileAsync("wmocodes.csv");
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
            foreach (var line in contents.Split('\n')) // Skip header
            {
                var parts = line.Split(',');
                if (parts.Length == 4)
                {
                    WmoCodesList.Add(new WmoCode
                    {
                        WeatherCode = int.TryParse(parts[0], out int code) ? code : 0,
                        IsDay = int.TryParse(parts[1], out int day) ? day : 0,
                        Description = parts[2].Replace("\"", "").Trim(),
                        Icon = parts[3].Replace("\"", "").Trim()
                    });

                }
            }
        }
        public static WmoCode GetWeather(int weatherCode, int isDay = 1)
        {
            if (WmoCodesList == null || WmoCodesList.Count == 0)
            {
                WmoCodesList = new List<WmoCode>();
                Task.Run(async () =>
                {
                    await ImportWmoCodes();
                });
            }

            var wmoResults = WmoCodesList.Where(w => w.WeatherCode == weatherCode && w.IsDay == isDay).FirstOrDefault();
            if (wmoResults != null)
            {
                return wmoResults;
            }

            return new WmoCode
            {
                WeatherCode = weatherCode,
                IsDay = isDay,
                Description = "Unknown",
                Icon = "unknown.png"
            };
        }

    }
}
