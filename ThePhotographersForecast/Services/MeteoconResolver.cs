using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Services
{
    public static class MeteoconsResolver
    {
        public static SKFileLottieImageSource ResolveIcon(int weatherCode, bool isDay)
        {
            // --- WMO / NWS Unified Mapping ---
            // This mapping is deterministic and audit-safe.
            string slug = weatherCode switch
            {
                // --- Clear ---
                0 => isDay ? "clear_day" : "clear_night",

                // --- Mostly Clear ---
                1 => isDay ? "mostly_clear_day" : "mostly_clear_night",

                // --- Partly Cloudy ---
                2 => isDay ? "partly_cloudy_day" : "partly_cloudy_night",

                // --- Overcast ---
                3 => isDay ? "overcast_day" : "overcast_night",

                // --- Fog / Mist ---
                45 => isDay ? "fog_day" : "fog_night",
                48 => isDay ? "overcast_day_fog" : "overcast_night_fog",

                // --- Drizzle ---
                51 => isDay ? "mostly_clear_day_drizzle" : "mostly_clear_night_drizzle",
                53 => isDay ? "overcast_day_drizzle" : "overcast_night_drizzle",
                55 => isDay ? "extreme_day_drizzle" : "extreme_night_drizzle",
                56 => isDay ? "mostly_clear_day_drizzle" : "mostly_clear_night_drizzle",
                57 => isDay ? "overcast_day_drizzle" : "overcast_night_drizzle",

                // ___ Rain ___
                61 => isDay ? "mostly_clear_day_rain" : "mostly_clear_night_rain",
                63 => isDay ? "partly_cloudy_day_rain" : "partly_cloudy_night_rain",
                65 => isDay ? "overcast_day_rain" : "overcast_night_rain",
                80 => isDay ? "mostly_clear_day_rain" : "mostly_clear_night_rain",
                81 => isDay ? "partly_cloudy_day_rain" : "partly_cloudy_night_rain",
                82 => isDay ? "overcast_day_rain" : "overcast_night_rain",

                // ___ Sleet / Ice Pellets ___
                66 => isDay ? "mostly_clear_day_sleet" : "mostly_clear_night_sleet",
                67 => isDay ? "extreme_day_sleet" : "extreme_night_sleet",
                77 => isDay ? "mostly_clear_day_sleet" : "mostly_clear_night_sleet",

                // ___ Snow ___
                71 => isDay ? "mostly_clear_day_snow" : "mostly_clear_night_snow",
                73 => isDay ? "partly_cloudy_day_snow" : "partly_cloudy_night_snow",
                75 => isDay ? "overcast_day_snow" : "overcast_night_snow",
                85 => isDay ? "mostly_clear_day_snow" : "mostly_clear_night_snow",
                86 => isDay ? "overcast_day_snow" : "overcast_night_snow",

                // ___ Thunderstorms ___
                95 => isDay ? "thunderstorms_day" : "thunderstorms_night",
                96 => isDay ? "thunderstorms_day_hail" : "thunderstorms_night_hail",
                99 => isDay ? "extreme_thunderstorms_overcast_day_hail"
                            : "extreme_thunderstorms_overcast_night_hail",

                // ___ Fallback ___
                _ => isDay ? "clear_day" : "clear_night"
            };
            var lottieFile = $"{slug}.json";
            var fileLocation = lottieFile;

            //string json = string.Empty;
            //Task.Run(async () =>
            //{
            //    var stream = await FileSystem.Current.OpenAppPackageFileAsync(lottieFile);
            //    using var reader = new StreamReader(stream);
            //    json = await reader.ReadToEndAsync();
            //});

            var animation = new SKFileLottieImageSource { File = lottieFile };


            return animation;


            //using var stream = await FileSystem.Current.OpenAppPackageFileAsync(Path.Combine("meteocons", lottieFile));
            //using var inputStream = File.Create(fileLocation);
            //await stream.CopyToAsync(inputStream);

            //return new SKFileLottieImageSource { File = fileLocation };

            //return new SKFileLottieImageSource { File = "not_available.json" };
        }

        public static SKFileLottieImageSource ResolveMoons(string phase)
        {
            var lottieFile = phase.ToLowerInvariant().Replace("-", "_").Replace(" ", "_") switch { "first_quarter" => "moon_first_quarter.json", "full_moon" => "moon_full.json", "last_quarter" => "moon_last_quarter.json", "waning_crescent" => "moon_waning_crescent.json", "waning_gibbous" => "moon_waning_gibbous.json", "waxing_crescent" => "moon_waxing_crescent.json", "waxing_gibbous" => "moon_waxing_gibbous.json", "new_moon" => "moon_new.json", _ => "unavailable.json" };

            var animation = new SKFileLottieImageSource { File = lottieFile };


            return animation;
            //            return new SKFileLottieImageSource { File = json };



            //using var stream = await FileSystem.Current.OpenAppPackageFileAsync(Path.Combine("meteocons", lottieFile));
            //using var inputStream = File.Create(fileLocation);
            //await stream.CopyToAsync(inputStream);

            //return new SKFileLottieImageSource { File = fileLocation };

        }

        public static SKFileLottieImageSource ResolveNwsDaily(string icon)
        {
            var uriArray = icon.Split("/").ToArray();
            bool isDay = true;
            string animation = string.Empty;
            ObservableCollection<string> lottieCollection = new();
            if (uriArray[5] == "night")
            {
                isDay = false;
            }
            var urlPart = uriArray[6];
            if (urlPart.Contains(','))
            {
                animation = urlPart.Split(',')[0].Trim();
                lottieCollection.Add(animation);
            }
            else if (urlPart.Contains('?'))
            {
                animation = urlPart.Split('?')[0].Trim();
                lottieCollection.Add(animation);
            }
            else
            {
                animation = urlPart.Trim();

            }



            var lottieFile = animation switch
            {
                "skc" => isDay ? "clear_day" : "clear_night",
                "few" => isDay ? "mostly_clear_day" : "mostly_clear_night",
                "sct" => isDay ? "mostly_clear_day" : "mostly_clear_night",
                "bkn" => isDay ? "partly_cloudy_day" : "partly_cloudy_night",
                "ovc" => isDay ? "overcast_day" : "overcast_night",
                "wind_skc" => isDay ? "wind" : "wind",
                "wind_few" => isDay ? "mostly_clear_day" : "mostly_clear_night",
                "wind_sct" => isDay ? "mostly_clear_day" : "mostly_clear_night",
                "wind_bkn" => isDay ? "partly_cloudy_day" : "partly_cloudy_night",
                "wind_ovc" => isDay ? "overcast_day" : "overcast_night",
                "snow" => isDay ? "mostly_clear_day_snow" : "mostly_clear_night_snow",
                "rain_snow" => isDay ? "partly_cloudy_day_sleet" : "partly_cloudy_night_sleet",
                "rain_sleet" => isDay ? "partly_cloudy_day_sleet" : "partly_cloudy_night_sleet",
                "snow_sleet" => isDay ? "partly_cloudy_day_sleet" : "partly_cloudy_night_sleet",
                "fzra" => isDay ? "partly_cloudy_day_sleet" : "partly_cloudy_night_sleet",
                "rain_fzra" => isDay ? "partly_cloudy_day_rain" : "partly_cloudy_night_rain",
                "snow_fzra" => isDay ? "partly_cloudy_day_sleet" : "partly_cloudy_night_sleet",
                "sleet" => isDay ? "partly_cloudy_day_sleet" : "partly_cloudy_night_sleet",
                "rain" => isDay ? "partly_cloudy_day_rain" : "partly_cloudy_night_rain",
                "rain_showers" => isDay ? "partly_cloudy_day_rain" : "partly_cloudy_night_rain",
                "rain_showers_hi" => isDay ? "overcast_day_rain" : "overcast_night_rain",
                "tsra" => isDay ? "thunderstorms_day" : "thunderstorms_night",
                "tsra_sct" => isDay ? "thunderstorms_mostly_clear_day" : "thunderstorms_mostly_clear_night",
                "tsra_hi" => isDay ? "thunderstorms_extreme_day" : "thunderstorms_extreme_night",
                "tornado" => "tornado",
                "hurricane" => "hurricane",
                "tropical_storm" => isDay ? "extreme_thunderstorms_overcast_day_rain" : "extreme_thunderstorms_overcast_night_rain",
                "dust" => isDay ? "dust_day" : "dust_night",
                "smoke" => isDay ? "smoke_day" : "smoke_night",
                "haze" => isDay ? "haze_day" : "haze_night",
                "hot" => "sun_hot",
                "cold" => "beanie",
                "blizzard" => "wind_snow",
                "fog" => isDay ? "fog_day" : "fog_night",
                _ => "not_available"
            } + ".json";
            //  string json = string.Empty;
            //Task.Run(async () =>
            //{
            //    var stream = await FileSystem.Current.OpenAppPackageFileAsync(lottieFile);
            //    using var reader = new StreamReader(stream);
            //    json = await reader.ReadToEndAsync();
            //});

            var skLottie = new SKFileLottieImageSource { File = lottieFile };
            return skLottie;

        }
    }
}