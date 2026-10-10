using SkiaSharp.Extended.UI.Controls;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public class WmoLookup
    {
        private static readonly Dictionary<(int WeatherCode, int IsDay), WmoCode> _wmoDictionary = BuildDefaultWmoDictionary();
        private static List<WmoCode>? _wmoCodesList;
        private static readonly object _lock = new();

        public static List<WmoCode> WmoCodesList
        {
            get
            {
                lock (_lock)
                {
                    return _wmoCodesList ??= _wmoDictionary.Values.ToList();
                }
            }
            set
            {
                lock (_lock)
                {
                    _wmoCodesList = value;
                }
            }
        }

        public static async Task ImportWmoCodesAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("wmocodes.csv");
                using var reader = new StreamReader(stream);

                var contents = await reader.ReadToEndAsync();
                var newList = new List<WmoCode>();

                foreach (var line in contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 5)
                    {
                        var weatherCode = int.TryParse(parts[0].Trim(), out int code) ? code : 0;
                        var isDay = int.TryParse(parts[1].Trim(), out int day) ? day : 0;
                        var description = parts[2].Replace("\"", "").Trim();
                        var icon = parts[3].Replace("\"", "").Trim();
                        var lottieFile = parts[4].Replace("\"", "").Trim();


                        var wmo = new WmoCode
                        {
                            WeatherCode = weatherCode,
                            IsDay = isDay,
                            Description = description,
                            Icon = icon,
                            WmoLottie = new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource
                            {
                                File = Path.Combine("meteocons", lottieFile)
                            }
                        };

                        newList.Add(wmo);
                        _wmoDictionary[(weatherCode, isDay)] = wmo;
                    }
                }

                lock (_lock)
                {
                    _wmoCodesList = newList;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WmoLookup.ImportWmoCodesAsync encountered error: {ex}");
            }
        }

        public static WmoCode GetWeather(int weatherCode, int isDay = 1)
        {
            if (_wmoDictionary.TryGetValue((weatherCode, isDay), out var result))
            {
                return result;
            }

            // Fallback for opposite day/night if exact match not found
            if (_wmoDictionary.TryGetValue((weatherCode, isDay == 1 ? 0 : 1), out var fallbackDayNight))
            {
                return fallbackDayNight;
            }

            return new WmoCode
            {
                WeatherCode = weatherCode,
                IsDay = isDay,
                Description = "Unknown",
                Icon = "unknown.png",
                WmoLottie = new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource { File = Path.Combine("meteocons", "not_available.json") }
            };
        }

        private static Dictionary<(int WeatherCode, int IsDay), WmoCode> BuildDefaultWmoDictionary()
        {
            var dict = new Dictionary<(int WeatherCode, int IsDay), WmoCode>();

            void Add(int code, int day, string desc, string icon, string lottie)
            {
                var item = new WmoCode
                {
                    WeatherCode = code,
                    IsDay = day,
                    Description = desc,
                    Icon = icon,
                    WmoLottie = new SKFileLottieImageSource { File = Path.Combine("meteocons", lottie) }
                };

                //var lottieFile = Path.Combine("meteocons", lottie);
                //var lottieLocation = Path.Combine(FileSystem.CacheDirectory,"meteocons", lottie);
                //if (File.Exists(lottieLocation))
                //{
                //    item.WmoLottie = new SKFileLottieImageSource { File = lottieLocation };
                //}
                //else
                //{
                //    Task.Run(async () =>
                //    {
                //        var stream = await FileSystem.OpenAppPackageFileAsync(lottieFile);
                //        var read = new StreamReader(stream);
                //        await File.WriteAllTextAsync(lottieLocation, await read.ReadToEndAsync());
                //        return new SKFileLottieImageSource { File = lottieLocation };
                //    });
                //}


                dict[(code, day)] = item;
            }

            // Clear
            Add(0, 1, "clear skies", "skc.png", "clear_day.json");
            Add(0, 0, "clear skies", "nskc.png", "clear_night.json");

            // Mostly clear / Few clouds
            Add(1, 1, "mix of sun and clouds", "few.png", "mostly_clear_day.json");
            Add(1, 0, "mix of sun and clouds", "nfew.png", "mostly_clear_night.json");

            // Partly cloudy / Scattered clouds
            Add(2, 1, "mostly cloudy", "sct.png", "partly_cloudy_day.json");
            Add(2, 0, "mostly cloudy", "nsct.png", "partly_cloudy_night.json");

            // Overcast
            Add(3, 1, "overcast", "ovc.png", "overcast_day.json");
            Add(3, 0, "overcast", "novc.png", "overcast_night.json");

            // Fog
            Add(45, 1, "fog", "fg.png", "fog_day.json");
            Add(45, 0, "fog", "nfg.png", "fog_night.json");

            // Freezing fog
            Add(48, 1, "freezing fog", "fg.png", "overcast_day_fog.json");
            Add(48, 0, "freezing fog", "nfg.png", "overcast_night_fog.json");

            // Drizzle
            Add(51, 1, "light drizzle", "minus_ra.png", "mostly_clear_day_drizzle.json");
            Add(51, 0, "light drizzle", "nra.png", "mostly_clear_night_drizzle.json");
            Add(53, 1, "moderate drizzle", "minus_ra.png", "overcast_day_drizzle.json");
            Add(53, 0, "moderate drizzle", "nra.png", "overcast_night_drizzle.json");
            Add(55, 1, "heavy drizzle", "ra.png", "extreme_day_drizzle.json");
            Add(55, 0, "heavy drizzle", "nra.png", "extreme_night_drizzle.json");

            // Freezing drizzle
            Add(56, 1, "light freezing drizzle", "fzra.png", "mostly_clear_day_drizzle.json");
            Add(56, 0, "light freezing drizzle", "nfzra.png", "mostly_clear_night_drizzle.json");
            Add(57, 1, "heavy freezing drizzle", "ra_fzra.png", "overcast_day_drizzle.json");
            Add(57, 0, "heavy freezing drizzle", "nra_fzra.png", "overcast_night_drizzle.json");

            // Rain
            Add(61, 1, "light rain", "minus_ra.png", "mostly_clear_day_rain.json");
            Add(61, 0, "light rain", "nra.png", "mostly_clear_night_rain.json");
            Add(63, 1, "moderate rain", "ra.png", "partly_cloudy_day_rain.json");
            Add(63, 0, "moderate rain", "nra.png", "partly_cloudy_night_rain.json");
            Add(65, 1, "heavy rain", "ra.png", "overcast_day_rain.json");
            Add(65, 0, "heavy rain", "nra.png", "overcast_night_rain.json");

            // Freezing rain
            Add(66, 1, "light freezing rain", "fzra.png", "mostly_clear_day_sleet.json");
            Add(66, 0, "light freezing rain", "nfzra.png", "mostly_clear_night_sleet.json");
            Add(67, 1, "heavy freezing rain", "fzra.png", "extreme_day_sleet.json");
            Add(67, 0, "heavy freezing rain", "nfzra.png", "extreme_night_sleet.json");

            // Snow
            Add(71, 1, "light snow", "sn.png", "mostly_clear_day_snow.json");
            Add(71, 0, "light snow", "nsn.png", "mostly_clear_night_snow.json");
            Add(73, 1, "moderate snow", "sn.png", "partly_cloudy_day_snow.json");
            Add(73, 0, "moderate snow", "sn.png", "partly_cloudy_night_snow.json");
            Add(75, 1, "heavy snow", "sn.png", "overcast_day_snow.json");
            Add(75, 0, "heavy snow", "nsn.png", "overcast_night_snow.json");

            // Snow grains
            Add(77, 1, "snow grains", "sn.png", "mostly_clear_day_sleet.json");
            Add(77, 0, "snow grains", "nsn.png", "mostly_clear_night_sleet.json");

            // Rain showers
            Add(80, 1, "light rain showers", "shra.png", "mostly_clear_day_rain.json");
            Add(80, 0, "light rain showers", "nshra.png", "mostly_clear_night_rain.json");
            Add(81, 1, "moderate rain showers", "shra.png", "partly_cloudy_day_rain.json");
            Add(81, 0, "moderate rain showers", "nshra.png", "partly_cloudy_night_rain.json");
            Add(82, 1, "heavy rain showers", "raip.png", "overcast_day_rain.json");
            Add(82, 0, "heavy rain showers", "nraip.png", "overcast_night_rain.json");

            // Snow showers
            Add(85, 1, "light snow showers", "sn.png", "mostly_clear_day_snow.json");
            Add(85, 0, "light snow showers", "nsn.png", "mostly_clear_night_snow.json");
            Add(86, 1, "heavy snow showers", "snip.png", "overcast_day_snow.json");
            Add(86, 0, "heavy snow showers", "nsnip.png", "overcast_night_snow.json");

            // Thunderstorms
            Add(95, 1, "thunderstorms", "tsra.png", "thunderstorms_day.json");
            Add(95, 0, "thunderstorms", "ntsra.png", "thunderstorms_night.json");
            Add(96, 1, "thunderstorms light hail", "hi_tsra.png", "thunderstorms_day_hail.json");
            Add(96, 0, "thunderstorms light hail", "hi_ntsra.png", "thunderstorms_night_hail.json");
            Add(99, 1, "thunderstorms heavy hail", "hi_shwrs.png", "extreme_thunderstorms_overcast_day_hail.json");
            Add(99, 0, "thunderstorms heavy hail", "hi_nshwrs.png", "extreme_thunderstorms_overcast_night_hail.json");

            return dict;
        }
    }
}
