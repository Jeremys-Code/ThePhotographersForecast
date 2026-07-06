using System.Diagnostics;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.Narratives;

namespace ThePhotographersForecast.Services
{
    //The Goal of this class is to be take in the numerical weather data and convert it into a human readable
    //format that can be used in the UI. It will also be responsible for determining the appropriate weather
    //icons to display based on the weather conditions.

    //Daily Weather Report:
    //Weather Summary with precipitation:
    // With a 55% to 80% chance of precipitation. Expect 0.125 inches of precipitation.
    // 0.025 inches of rain and 0.1 inches of snow. Mostly cloudy skies with a high of 35°F 
    // and a low of 25°F. The clouds will {increase or decrease throughout the day from {10% to 60%}.}
    //The Morning will start off {bright100%, cloudy50%, overcast25%}. Around noon expect {continuing, more clouds, clouds clearing}
    // The evening sky will be {clear, or degree of cloudiness, if clear always.... perfect for star gazing.}
    // Winds from the northeast up to 15 mph, with gusts up to 40 mph.
    //




    // Weather Summary with no precipitation:
    // There is no precipitation expected. Clear skies with a high of 75°F and a low of 55°F.
    // Winds from the north-northwest up to 8 mph, with gust up to 25 mph.
    //
    //    Health & Safety Summary:
    //    Outside feels like 60°F to 75°F, {with the dewpoint at 55°F, it will feel slightly humid but generally comfortable.}
    //The UV Index is Low / Green(2.0).You can safely enjoy
    //    being outside. The wet-bulb temperature is 75°F, which is safe for outdoor activities.
    //    15 minute break each hour.
    public class WeatherReporter
    {

        private GetWeatherService _weatherService;

        public void Init(GetWeatherService weatherService)
        {
            _weatherService = weatherService;
        }
        public (string weather, string health, string precipitation) GetDailyWeatherSummary(DailyForecast dailyForecast, HourlyForecast[] hourlyForecasts)
        {

            var precipitationSummary = GetDailyPrecipitation(dailyForecast);
            string dailySummary = $"{precipitationSummary} " +
                $"{dailyForecast.WeatherCodeDescription} " +
                $"{CloudCoverSummary.BuildCondensedSummary(BuildDailyBins(dailyForecast, hourlyForecasts))}" +
                $"with a high of {String.Concat(Math.Round(dailyForecast.Temperature2mMax.Value, 0).ToString(), dailyForecast.Temperature2mMax.Unit)} " +
                $"and a low of {String.Concat(Math.Round(dailyForecast.Temperature2mMin.Value, 0).ToString(), dailyForecast.Temperature2mMin.Unit)}. " +
                $"{dailyForecast.WindDirection10mDominant.Description} winds up to {String.Concat(dailyForecast.WindSpeed10mMax.Value.ToString(), dailyForecast.WindSpeed10mMax.Unit)} " +
                (dailyForecast.WindGusts10mMax.Value == 0 ? "." : $"with gusts up to {String.Concat(dailyForecast.WindGusts10mMax.Value.ToString(), dailyForecast.WindGusts10mMax.Unit)}.");

            var healthSummary = GetDailyHealthInfo(dailyForecast);

            return (dailySummary, healthSummary, precipitationSummary);
        }

        public MeteorologisticAgent BuildDailyBins(DailyForecast dailyForecast, HourlyForecast[] hourlyForecasts)
        {

            // Filter hourlies for this day
            var hours = hourlyForecasts
                .ToList();

            // Group by time-of-day bucket
            var groups = hours
                .GroupBy(h => GetTimeOfDayBucket(h.Time))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build the agent
            var agent = new MeteorologisticAgent
            {
                BinDate = dailyForecast.Date.ToDateTime(TimeOnly.MinValue),

                Overnight = BuildBin("Overnight", groups),
                EarlyMorning = BuildBin("EarlyMorning", groups),
                Morning = BuildBin("Morning", groups),
                Afternoon = BuildBin("Afternoon", groups),
                Evening = BuildBin("Evening", groups),
                Night = BuildBin("Night", groups)
            };

            return agent;
        }
        private Bin BuildBin(string label, Dictionary<string, List<HourlyForecast>> groups)
        {
            if (!groups.TryGetValue(label, out var list))
                return new Bin { Label = label, Values = Enumerable.Empty<double>() };

            return new Bin
            {
                Label = label,
                Values = list.Select(h => h.CloudCover.Value)
            };
        }



        public string GetDailyPrecipitation(DailyForecast day)
        {
            string precipitation = string.Empty;

            var amt = Math.Round(day.PrecipitationSum.Value, 3);

            if (amt == 0)
            {
                return "There is no precipitation expected.";
            }
            if (amt > 0)
            {

                var snow = Math.Round(day.SnowfallSum.Value, 3);
                bool isSnow = (snow > 0 ? true : false);
                var rain = Math.Round(day.RainSum.Value, 3);
                bool isRain = (rain > 0 ? true : false);
                var showers = Math.Round(day.ShowersSum.Value, 3);
                bool isShowers = (showers > 0 ? true : false);
                string precip = string.Empty;
                if (isSnow)
                {
                    if (precip == string.Empty)
                    {
                        precip = "snow";
                    }
                    else
                    {
                        precip += " and snow";
                    }

                }
                if (isRain)
                {
                    if (precip == string.Empty)
                    {
                        precip = "rain";
                    }
                    else
                    {
                        precip += "/rain mix";
                    }

                }
                if (isShowers)
                {
                    if (precip == string.Empty)
                    {
                        precip = "showers";
                    }
                    else
                    {
                        precip += " showers";
                    }

                }

                precipitation = $"with a {String.Concat(day.PrecipitationProbabilityMin.Value.ToString(), day.PrecipitationProbabilityMin.Unit)} to {String.Concat(day.PrecipitationProbabilityMax.Value.ToString(), day.PrecipitationProbabilityMax.Unit)} chance of {precip}. ";
                string precipitationType = $"Expect {String.Concat(Math.Round(day.PrecipitationSum.Value, 3), day.PrecipitationSum.Unit)} of {precip}. ";
                //while (amt > 0)
                //{
                //    if (isShowers)
                //    {
                //        string shower = $"{String.Concat(day.ShowersSum.Value, day.ShowersSum.Unit)} of showers, ";
                //        amt -= showers;
                //        if (amt == 0)
                //        {
                //            precipitationType += shower.Remove(shower.LastIndexOf(", "), 2) + ". ";
                //            break;
                //        }


                //    }
                //    if (isRain)
                //    {
                //        string rn = $"{String.Concat(day.RainSum.Value, day.RainSum.Unit)} of rain, ";
                //        amt -= rain;
                //        if (amt == 0)
                //        {
                //            if (precipitationType.Contains("showers"))
                //            {
                //                precipitationType = precipitationType.Replace(". ", " and ");
                //                precipitationType += string.Concat(rn.Remove(rn.LastIndexOf(", "), 2), ". ");
                //                break;
                //            }
                //            else
                //            {
                //                precipitationType += precipitationType.Remove(precipitationType.LastIndexOf(", "), 2) + ". ";
                //                break;
                //            }
                //        }
                //    }
                //    if (isSnow)
                //    {
                //        amt -= snow;
                //        if (amt == 0)
                //        {
                //            string snw = $"{String.Concat(snow, day.SnowfallSum.Unit)} of snow. ";
                //            if (precipitationType.Contains("showers") && precipitationType.Contains("rain"))
                //            {
                //                precipitationType = precipitationType.Replace(" and ", ", ");
                //                precipitationType = precipitationType.Replace(". ", " and ");
                //                precipitationType += snw;
                //                break;
                //            }
                //            else if (precipitationType.Contains("showers") || precipitationType.Contains("rain"))
                //            {
                //                precipitationType += snw.Replace(". ", " and ");
                //                precipitationType += snw;
                //                break;
                //            }
                //            else
                //            {
                //                precipitationType += snw;
                //                break;
                //            }

                //            break;
                //        }
                //    }
                //    else
                //    {
                //        Debug.WriteLine($"Error: Precipitation amount does not equal the rain, snow, and showers. Offset amount: {amt}.");
                //        break;
                //    }
                //}
                precipitation += precipitationType;

            }

            return precipitation;
        }

        //
        public string GetDailyHealthInfo(DailyForecast day)
        {
            //Sun health and safety
            //Working outside health and safety information
            string health = String.Concat("Outside feels like ", day.ApparentTemperatureMin.Value, day.ApparentTemperatureMin.Unit, " to ", day.ApparentTemperatureMax.Value, day.ApparentTemperatureMax.Unit, " and ");
            if (TempConverter.Temp == "F")
            {
                switch (day.DewPoint2mMean.Value)
                {
                    case <= 55:
                        health += "comfortable.";
                        break;
                    case > 55 and <= 60:
                        health += "slightly humid, but generally comfortable.";
                        break;
                    case > 60 and <= 65:
                        health += "noticeable humid, not too comfortable.";
                        break;
                    case > 65 and <= 70:
                        health += "sticky, humid, and uncomfortable.";
                        break;
                    case > 70:
                        health += "oppressively humid, like walking through a wall of water, and wicked uncomfortable. Be careful!";
                        break;
                }
            }
            else
            {
                switch (day.DewPoint2mMean.Value)
                {
                    case <= 13:
                        health += "comfortable.";
                        break;
                    case > 13 and <= 16:
                        health += "slightly humid, but generally comfortable.";
                        break;
                    case > 16 and <= 18:
                        health += "noticeable humid, not too comfortable.";
                        break;
                    case > 18 and <= 21:
                        health += "sticky, humid, and uncomfortable.";
                        break;
                    case > 21:
                        health += "oppressively humid, like walking through a wall of water, and wicked uncomfortable. Be careful!";
                        break;
                }
            }
            health += "The UV Index is ";
            double uv = day.UvIndexMax.Value;
            switch (uv)
            {
                case < 3.0:
                    health += $"Low/Green ({day.UvIndexMax}). You can safely enjoy being outside.";
                    break;
                case >= 3.0 and < 6.0:
                    health += $"Moderate/Yellow ({day.UvIndexMax}). Seek shade during the midday hours. Put on a shirt, use sunscreen, and wear a hat.";
                    break;
                case >= 6.0 and < 8.0:
                    health += $"High/Orange ({day.UvIndexMax}). Seek shade during the midday hours. Put on a shirt, use sunscreen, and wear a hat.";
                    break;
                case >= 8.0 and < 11.0:
                    health += $"Very High/Red ({day.UvIndexMax}). Avoid being outside during midday hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    break;
                case >= 11.0:
                    health += $"Extreme/Purple ({day.UvIndexMax}). Protect yourself from the sun. Avoid being outside during midday hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    break;
                default:
                    break;
            }

            var wet = day.WetBulbTemperature2mMax.Value;
            if (TempConverter.Temp == "F")
            {
                switch (wet)
                {
                    case <= 85.0:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is safe for outdoor activities. 15 minute break each hour.";
                        break;
                    case > 85.0 and <= 88.0:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is a moderate risk for outdoor activities. 30 minute break each hour.";
                        break;
                    case >= 88.0 and < 90.0:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is a high risk for outdoor activities. 40 minute break each hour.";
                        break;
                    case > 90:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is an extreme risk for outdoor activities. Avoid being outside.";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (wet)
                {
                    case <= 29:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is safe for outdoor activities. 15 minute break each hour.";
                        break;
                    case > 29 and <= 31:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is a moderate risk for outdoor activities. 30 minute break each hour.";
                        break;
                    case >= 31 and < 32:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is a high risk for outdoor activities. 40 minute break each hour.";
                        break;
                    case > 32:
                        health += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax}, which is an extreme risk for outdoor activities. Avoid being outside.";
                        break;
                    default:
                        break;
                }
            }
            return health;
        }


        public (string weather, string health, string precipitation) GetHourlyWeatherSummary(HourlyForecast hourlyForecast)
        {
            var precipitationSummary = GetHourlyPrecipitation(hourlyForecast);
            string hourlySummary = $"{Services.GetTimeOfDay.TimesOfDayGreeting(hourlyForecast.Time.Hour)}. The forecast is {hourlyForecast.WeatherCodeDescription.ToLowerInvariant()} for " +
                   $"the {hourlyForecast.Time.ToString("htt")}. " +
                $"{precipitationSummary} " +
                $"Temperature at  {String.Concat(Math.Round(hourlyForecast.Temperature2m.Value, 0).ToString(), hourlyForecast.Temperature2m.Unit)} " +
                $"with winds blowing from the {hourlyForecast.WindDirection10m.Description} at {String.Concat(hourlyForecast.WindSpeed10m.Value.ToString(), hourlyForecast.WindSpeed10m.Unit)}";
            string gusts = (hourlyForecast.WindGusts10m.Value == 0 ? $" and gusts up to {String.Concat(hourlyForecast.WindGusts10m.Value.ToString(), hourlyForecast.WindGusts10m.Unit)}." : ".");

            var healthSummary = GetHourlyHealthInfo(hourlyForecast);
            return (hourlySummary, healthSummary, precipitationSummary);
        }




        public string GetHourlyPrecipitation(HourlyForecast hour)
        {
            string precipitation = string.Empty;

            var amt = Math.Round(hour.Precipitation.Value, 3);
            if (amt == 0)
            {
                return "There is no precipitation expected.";
            }
            if (amt > 0)
            {
                var snow = Math.Round(hour.Snowfall.Value, 3);
                bool isSnow = (snow > 0 ? true : false);
                var rain = Math.Round(hour.Rain.Value, 3);
                bool isRain = (rain > 0 ? true : false);
                var showers = Math.Round(hour.Showers.Value, 3);
                bool isShowers = (showers > 0 ? true : false);
                string precip = string.Empty;
                if (isSnow)
                {
                    if (precip == string.Empty)
                    {
                        precip = "snow";
                    }
                    else
                    {
                        precip += " and snow";
                    }

                }
                if (isRain)
                {
                    if (precip == string.Empty)
                    {
                        precip = "rain";
                    }
                    else
                    {
                        precip += "/rain mix";
                    }

                }
                if (isShowers)
                {
                    if (precip == string.Empty)
                    {
                        precip = "showers";
                    }
                    else
                    {
                        precip += " showers";
                    }

                }

                precipitation = $"with a {String.Concat(hour.PrecipitationProbability.Value.ToString(), hour.PrecipitationProbability.Unit)} chance of {precip}. ";
                string precipitationType = $"Expect {String.Concat(Math.Round(hour.Precipitation.Value, 3), hour.Precipitation.Unit)} of {precip}. ";
                precipitation += precipitationType;
            }

            return precipitation;
        }
        public string GetHourlyHealthInfo(HourlyForecast hour)
        {
            //Working outside health and safety information//Sun health and safety
            string health = String.Concat("Outside feels like ", hour.ApparentTemperature.Value, hour.ApparentTemperature.Unit, " and ");
            switch (hour.DewPoint2m.Value)
            {
                case <= 55:
                    health += "comfortable.";
                    break;
                case > 55 and <= 60:
                    health += "slightly humid, but generally comfortable.";
                    break;
                case > 60 and <= 65:
                    health += "noticeable humid, not too comfortable.";
                    break;
                case > 65 and <= 70:
                    health += "sticky, humid, and uncomfortable.";
                    break;
                case > 70:
                    health += "oppressively humid, like walking through a wall of water, and wicked uncomfortable. Be careful!";
                    break;
            }

            health += " The UV Index is ";
            double uv = hour.UvIndex.Value;
            switch (uv)
            {
                case < 3.0:
                    health += $"Low/Green ({hour.UvIndex}). You can safely enjoy being outside.";
                    break;
                case >= 3.0 and < 6.0:
                    health += $"Moderate/Yellow ({hour.UvIndex}). Seek shade during the afternoon hours. Put on a shirt, use sunscreen, and wear a hat.";
                    break;
                case >= 6.0 and < 8.0:
                    health += $"High/Orange ({hour.UvIndex}). Seek shade during the afternoon hours. Put on a shirt, use sunscreen, and wear a hat.";
                    break;
                case >= 8.0 and < 11.0:
                    health += $"Very High/Red ({hour.UvIndex}). Avoid being outside during afternoon hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    break;
                case >= 11.0:
                    health += $"Extreme/Purple ({hour.UvIndex}). Protect yourself from the sun. Avoid being outside during afternoon hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    break;
                default:
                    break;
            }

            var wet = hour.WetBulbTemperature2m.Value;
            switch (wet)
            {
                case <= 85.0:
                    health += $" The wet bulb temperature is {hour.WetBulbTemperature2m}, which is safe for outdoor activities. 15 minute break each hour.";
                    break;
                case > 85.0 and <= 88.0:
                    health += $" The wet bulb temperature is {hour.WetBulbTemperature2m}, which is a moderate risk for outdoor activities. 30 minute break each hour.";
                    break;
                case >= 88.0 and < 90.0:
                    health += $" The wet bulb temperature is {hour.WetBulbTemperature2m}, which is a high risk for outdoor activities. 40 minute break each hour.";
                    break;
                case > 90:
                    health += $" The wet bulb temperature is {hour.WetBulbTemperature2m}, which is an extreme risk for outdoor activities. Avoid being outside.";
                    break;
                default:
                    break;
            }
            health += $"{Environment.NewLine}Relative Humidity: {hour.RelativeHumidity2m.Value}{hour.RelativeHumidity2m.Unit}";
            health += $"{Environment.NewLine}Dewpoint: {hour.DewPoint2m.Value}{hour.DewPoint2m.Unit}";
            health += $"{Environment.NewLine}Chance Of Precipitation: {hour.PrecipitationProbability.Value}{hour.PrecipitationProbability.Unit}";
            return health;
        }

        //public async Task LoadWeatherForDay(DayViewModel day)
        //{
        //    if (day != null)
        //        return;
        //    // Already loaded
        //if (ServiceHelper.GetService<GetWeatherService>().WeatherData.Any())
        //{
        //var photoDay = Forecast.Where<PhotoDay>(pd => new DateTime(pd.Date.Year, pd.Date.Month, pd.Date.Day, 0, 0, 0).Date == day.Date.Date).FirstOrDefault();
        //if (photoDay == null) return;

        //var weatherData = ServiceHelper.GetService<GetWeatherService>().WeatherData;
        //if (weatherData == null) return; // Or fetch it if not available

        //// Daily Summary
        //var daily = weatherData.Where(x => x.Forecast.Where(f => f.Date.Date == day.Date.Date).FirstOrDefault<DailyForecast>();
        //var dailySummary = ServiceHelper.GetService<WeatherReporter>().GetDailyWeatherSummary(daily);
        //// Func<int, int, (int quotient, int remainder)> divRem = (a, b) => (Math.DivRem(a, b, out int rem), rem);
        //// Hourly Weather
        //var hourlyVms = new ObservableCollection<HourlyWeatherViewModel>();
        ////var hourly = weatherData.HourlyForecasts.Where(h => Math.DivRem(h.ForecastTime.Hour, 2, out int rem) == 0).Select(x => x);
        //foreach (var hour in hours)
        //{
        //    var hourlyvm = new HourlyWeatherViewModel();
        //    hourlyvm.TimeOfDay = hour.TimeOfDay;
        //    hourlyvm.Summary = hour.WeatherSummary;
        //    hourlyvm.Hour = hour.ForecastTime.ToString("hh:mm tt");
        //    hourlyvm.OutdoorHealthSafety = hour.HealthSafetySummary;
        //    hourlyvm.WmoImageUrl = hour.WeatherImage;
        //    hourlyVms.Add(hourlyvm);
        //}
        //day.WeatherDetails.HourlyWeather = hourlyVms;

        //  }

        private string GetTimeOfDayBucket(DateTime time)
        {
            int hour = time.Hour;

            return hour switch
            {
                >= 0 and <= 2 => "Overnight",
                >= 3 and <= 5 => "EarlyMorning",
                >= 6 and <= 11 => "Morning",
                >= 12 and <= 17 => "Afternoon",
                >= 18 and <= 21 => "Evening",
                _ => "Night"
            };
        }
    }
}
