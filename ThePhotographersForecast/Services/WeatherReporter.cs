using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    //The Goal of this class is to be take in the numerical weather data and convert it into a human readable
    //format that can be used in the UI. It will also be responsible for determining the appropriate weather
    //icons to display based on the weather conditions.

    //Daily Weather Report:
    //Weather Summary with precipitation:
    // With a 55% to 80% chance of rain. Expect 0.125 inches of rain.
    // Mostly cloudy skies with a high of 35°F 
    // and a low of 25°F. The clouds will {increase or decrease throughout the day from {10% to 60%}.}
    //The Morning will start off {bright100%, cloudy50%, overcast25%}. Around noon expect {continuing, more clouds, clouds clearing}
    // The evening sky will be {clear, or degree of cloudiness, if clear always.... perfect for star gazing.}
    // Winds from the northeast up to 15 mph, with gusts up to 40 mph.
    //Weather Revised
    //IF Percipitation (Rainy Days
    // Weather Description
    //If percipitation precipitation amount
    //              High        Low
    // Temp



    // Weather Summary with no precipitation:
    // There is no precipitation expected. Clear skies with a high of 75°F and a low of 55°F.
    // Winds from the north-northwest up to 8 mph, with gust up to 25 mph.
    //
    //    Health & Safety Summary:
    //    Outside feels like 60°F to 75°F, {with the dewpoint at 55°F, it will feel slightly humid but generally comfortable.}
    //The UV Index is Low / Green(2.0).You can safely enjoy
    //    being outside. The wet-bulb temperature is 75°F, which is safe for outdoor activities.
    //    15 minute break each hour.
    public class WeatherReporter : IWeatherReporter
    {

        private IImageCacheService _cacheService;
        private ISession _session;
        public WeatherReporter(ISession session, IImageCacheService cacheService)
        {

            _session = session;
            _cacheService = cacheService;
        }
        /// <summary>
        /// Daily Weather Reporter - Produces a story-like forecast from 
        /// the numerical weather model data gathered. 
        /// </summary>
        /// <param name="dailyForecast"></param>
        /// <param name="hourlyForecasts"></param>
        /// <returns>
        /// DailyForecasts that describes the weather and how it evolves throughout the day
        /// Health and Safety statement including WBGT, UV Index, heat index, and Physiological Temperature Strain Index
        /// </returns>
        // Sample: Here's your daily forecast at Boston on Tuesday, September 22,
        // Daily Start
        public ForecastSummaries GetDailyWeatherSummary(DailyForecast dailyForecast, HourlyForecast[] hourlyForecasts)
        {
            var fs = new ForecastSummaries
            {
                Time = dailyForecast.Date.ToDateTime(TimeOnly.MinValue),
                IsDaily = true
            };
            //    var precipitationSummary = GetDailyPrecipitation(dailyForecast);
            var highTemp = hourlyForecasts.Where(t => t.IsDay == 1).MaxBy(h => h.Temperature2m.Value);
            var lowTemp = hourlyForecasts.Where(t => t.IsDay == 0 && t.HourlyTime.Hour >= 12).MinBy(c => c.Temperature2m.Value);
            fs.DailyLowAndTime = $"{lowTemp.Temperature2m.Value}{lowTemp.Temperature2m.Unit} @ {lowTemp.HourlyTime.ToString("h tt")}";
            fs.DailyHighAndTime = $"{highTemp.Temperature2m.Value}{highTemp.Temperature2m.Unit} @ {highTemp.HourlyTime.ToString("h tt")}";
            string firstStatement = dailyForecast.WeatherCode < 10 ? dailyForecast.WeatherCodeDescription : GetDailyPrecipitation(dailyForecast);
            string gustStatement = dailyForecast.WindGusts10mMax.Value > dailyForecast.WindSpeed10mMax.Value * 1.5 ? $" and wind gusts up to {dailyForecast.WindGusts10mMax.Value}{dailyForecast.WindGusts10mMax.Unit.Replace(@"/", "")}." : ".";

            fs.WeatherSummary = $"Here's your daily forecast at {_session.SelectedLocation.City} on {dailyForecast.Date.ToString("DDDD, MMMM DD")} : {firstStatement} " +
               $"The high temperature is {dailyForecast.Temperature2mMax.Value}{dailyForecast.Temperature2mMax.Unit} and the low is {dailyForecast.Temperature2mMin.Value}{dailyForecast.Temperature2mMin.Unit}. " +
                //  $"{CloudCoverSummary.BuildCondensedSummary(BuildDailyBins(dailyForecast))} " +
                $"Winds from the {dailyForecast.WindDirection10mDominant.Description} at {dailyForecast.WindSpeed10mMax.Value} {(dailyForecast.WindSpeed10mMax.Unit == "mp/h" ? "mph" : dailyForecast.WindSpeed10mMax.Unit)}{gustStatement}";



            var healthSummary = GetDailyHealthInfo(dailyForecast);

            fs.HealthSafetySummary = healthSummary.healthSummary;
            fs.UVIndexColor = healthSummary.uvIndexColor;

            return fs;
        }

        public string GetDailyPrecipitation(DailyForecast day)
        {

            string precipitation = $"There is a {String.Concat(Math.Round(day.PrecipitationProbabilityMin.Value, 0), day.PrecipitationProbabilityMin.Unit)} to {String.Concat(Math.Round(day.PrecipitationProbabilityMax.Value, 0), day.PrecipitationProbabilityMax.Unit)} cof {day.WeatherCodeDescription.ToLowerInvariant()}{GetAmount(day)}";
            return precipitation;
        }

        private string GetAmount(DailyForecast day)
        {
            if (day.PrecipitationSum.Value > 0)
            {
                var amt = Math.Round(day.PrecipitationSum.Value, 3) switch
                {
                    <= 0.1 => ", totaling about 1/10 of an inch.",
                    > 0.1 and <= 0.25 => ", totaling about 1/4 of an inch.",
                    > 0.25 and <= 0.50 => ", totaling about 1/2 of an inch.",
                    > 0.50 and <= 0.75 => ", totaling about 3/4 of an inch.",
                    > 0.75 and <= 1.0 => ", totaling about an inch.",
                    > 1.0 => $", totaling about {Math.Round(day.PrecipitationSum.Value, 0)} inches.",
                    _ => ", there is no precipation to report."
                };

                return amt;
            }
            else
            {
                return ".";
            }
        }
        //
        public (string healthSummary, Color uvIndexColor) GetDailyHealthInfo(DailyForecast day)
        {
            var uvIndexColor = Colors.Green;
            //Sun healthSummary and safety
            //Working outside healthSummary and safety information
            string healthSummary = String.Concat("Outside feels like ", day.ApparentTemperatureMin.Value, day.ApparentTemperatureMin.Unit, " to ", day.ApparentTemperatureMax.Value, day.ApparentTemperatureMax.Unit, " and ");
            if (day.DewPoint2mMean.Unit.EndsWith("F"))
            {
                switch (day.DewPoint2mMean.Value)
                {
                    case <= 30:
                        healthSummary += "dry. ";
                        break;
                    case > 40 and <= 60:
                        healthSummary += "comfortable. ";
                        break;
                    case > 60 and <= 70:
                        healthSummary += "sticky. ";
                        break;
                    case > 70:
                        healthSummary += "tropical. ";
                        break;
                }
            }
            else if (day.DewPoint2mMean.Unit.EndsWith("C"))
            {
                switch (day.DewPoint2mMean.Value)
                {
                    case <= -1.111:
                        healthSummary += "dry. ";
                        break;
                    case > 4.444 and <= 15.555:
                        healthSummary += "comfortable. ";
                        break;
                    case > 15.555 and <= 21:
                        healthSummary += "sticky. ";
                        break;
                    case > 21:
                        healthSummary += "tropical. ";
                        break;
                }
            }

            healthSummary += "The UV Index is {day.UvIndexMax.Value}: ";
            double uv = day.UvIndexMax.Value;
            switch (uv)
            {
                case < 3.0:
                    healthSummary += $"Low/Green. You can safely enjoy being outside.";
                    break;
                case >= 3.0 and < 6.0:
                    healthSummary += $"Moderate/Yellow. Seek shade during the afternoon hours. Put on a shirt, use sunscreen, and wear a hat.";
                    uvIndexColor = Colors.Yellow;
                    break;
                case >= 6.0 and < 8.0:
                    healthSummary += $"High/Orange. Seek shade during the afternoon hours. Put on a shirt, use sunscreen, and wear a hat.";
                    uvIndexColor = Colors.Orange;
                    break;
                case >= 8.0 and < 11.0:
                    healthSummary += $"Very High/Red. Avoid being outside during afternoon hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    uvIndexColor = Colors.Red;
                    break;
                case >= 11.0:
                    healthSummary += $"Extreme/Purple. Protect yourself from the sun. Avoid being outside during afternoon hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    uvIndexColor = Colors.Purple;
                    break;
                default:
                    healthSummary += " unreported.";
                    break;
            }

            var wet = day.WetBulbTemperature2mMax.Value;
            if (day.WetBulbTemperature2mMax.Unit.EndsWith("F"))
            {
                switch (wet)
                {
                    case <= 85.0:
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is safe for outdoor activities.";
                        break;
                    case > 85.0 and <= 88.0:
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is a moderate risk for outdoor activities.";
                        break;
                    case >= 88.0 and < 90.0:
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is a high risk for outdoor activities.";
                        break;
                    case > 90:
                        healthSummary += $" The wet-bulb temperature is{day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit} {day.WetBulbTemperature2mMax}, which is an extreme risk for outdoor activities.";
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
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is safe for outdoor activities.";
                        break;
                    case > 29 and <= 31:
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is a moderate risk for outdoor activities.";
                        break;
                    case >= 31 and < 32:
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is a high risk for outdoor activities.";
                        break;
                    case > 32:
                        healthSummary += $" The wet-bulb temperature is {day.WetBulbTemperature2mMax.Value}{day.WetBulbTemperature2mMax.Unit}, which is an extreme risk for outdoor activities.";
                        break;
                    default:
                        break;
                }
            }
            return (healthSummary, uvIndexColor);
        }
        // Daily End

        // Build the Bins to use with the Engines Start
        //public MeteorologisticAgent BuildDailyBins(DailyForecasts dailyForecast)
        //{

        //    // Filter hourlies for this day
        //    var groups = dailyForecast.SelectedDailyHourlyForecasts
        //        .OrderBy(h => h.HourlyTime)
        //        .GroupBy(t => t.TimeOfDay)
        //        .ToDictionary(g => g.Key, g => g
        //        .ToList());




        //    // Build the agent
        //    var agent = new MeteorologisticAgent
        //    {
        //        BinDate = dailyForecast.Time.ToDateTime(TimeOnly.MinValue),

        //        EarlyHours = BuildBin("Early Hours", groups),
        //        EarlyMorning = BuildBin("Early Morning", groups),
        //        Morning = BuildBin("Morning", groups),
        //        Afternoon = BuildBin("Afternoon", groups),
        //        Evening = BuildBin("Evening", groups),
        //        Night = BuildBin("Night", groups)
        //    };

        //    return agent;
        //}
        //private Bin BuildBin(string label, Dictionary<string, List<HourlyForecast>> groups)
        //{
        //    if (!groups.TryGetValue(label, out var list))
        //        return new Bin { Label = label, Values = new List<double>() };

        //    return new Bin
        //    {
        //        Label = label,
        //        Values = list.Select(h => h.CloudCover.Value).ToList()
        //    };
        //}

        //// Bins End



        // Hourly Forecast
        public ForecastSummaries GetHourlyWeatherSummary(HourlyForecast hourlyForecast)
        {
            var fs = new ForecastSummaries
            {
                Time = hourlyForecast.HourlyTime,
                IsDaily = false
            };
            fs.WeatherSummary =
            String.Concat($"Here's your forecast for {hourlyForecast.HourlyTime.ToString("htt")}: ", hourlyForecast.WeatherCode > 10 ? GetHourlyPrecipitation(hourlyForecast) + " " : "",
                Math.Round(hourlyForecast.Temperature2m.Value, 0), hourlyForecast.Temperature2m.Unit, " and ", CloudCoverEngine.Category(hourlyForecast.CloudCover.Value / 100), ". ",
                "Winds from the ", hourlyForecast.WindDirection10m.Description, " at ", hourlyForecast.WindSpeed10m.Value, hourlyForecast.WindSpeed10m.Unit == "mp/h" ? "mph" : hourlyForecast.WindSpeed10m.Unit);

            fs.WeatherSummary += hourlyForecast.WindGusts10m.Value > (hourlyForecast.WindSpeed10m.Value * 1.5) ? " and wind gusts up to " + hourlyForecast.WindGusts10m.Value + hourlyForecast.WindGusts10m.Unit + "." : ".";
            fs.WeatherSummary = fs.WeatherSummary.Replace("/", "");


            var healthSummary = GetHourlyHealthInfo(hourlyForecast);
            fs.HealthSafetySummary = healthSummary.healthSummary;
            fs.UVIndexColor = healthSummary.uvIndexColor;
            return fs;
        }

        public string GetHourlyPrecipitation(HourlyForecast hour)
        {

            string precipitation = $"There is a {String.Concat(Math.Round(hour.PrecipitationProbability.Value, 0), hour.PrecipitationProbability.Unit)} chance of {hour.WeatherCodeDescription.ToLowerInvariant()}{GetHourAmount(hour)}";
            return precipitation;
        }

        private string GetHourAmount(HourlyForecast hour)
        {
            if (hour.Precipitation.Value > 0)
            {
                var amt = Math.Round(hour.Precipitation.Value, 3) switch
                {
                    <= 0.1 => ", totaling about 1/10 of an inch.",
                    > 0.1 and <= 0.25 => ", totaling about 1/4 of an inch.",
                    > 0.25 and <= 0.50 => ", totaling about 1/2 of an inch.",
                    > 0.50 and <= 0.75 => ", totaling about 3/4 of an inch.",
                    > 0.75 and <= 1.0 => ", totaling about an inch.",
                    > 1.0 => $", totaling about {Math.Round(hour.Precipitation.Value, 0)} inches.",
                    _ => ", there is no precipation to report."
                };

                return amt;
            }
            else
            {
                return ".";
            }
        }

        public (string healthSummary, Color uvIndexColor) GetHourlyHealthInfo(HourlyForecast hour)
        {
            var uvIndexColor = Colors.Green;
            //Sun healthSummary and safety
            //Working outside healthSummary and safety information
            string healthSummary = String.Concat("Outside feels like ", hour.ApparentTemperature.Value, hour.ApparentTemperature.Unit, " and ");
            if (hour.DewPoint2m.Unit.EndsWith("F"))
            {
                switch (hour.DewPoint2m.Value)
                {
                    case <= 30:
                        healthSummary += "dry. ";
                        break;
                    case > 40 and <= 60:
                        healthSummary += "comfortable. ";
                        break;
                    case > 60 and <= 70:
                        healthSummary += "sticky. ";
                        break;
                    case > 70:
                        healthSummary += "tropical. ";
                        break;
                }
            }
            else if (hour.DewPoint2m.Unit.EndsWith("C"))
            {
                switch (hour.DewPoint2m.Value)
                {
                    case <= -1.111:
                        healthSummary += "dry. ";
                        break;
                    case > 4.444 and <= 15.555:
                        healthSummary += "comfortable. ";
                        break;
                    case > 15.555 and <= 21:
                        healthSummary += "sticky. ";
                        break;
                    case > 21:
                        healthSummary += "tropical. ";
                        break;
                }
            }

            healthSummary += "The UV Index is {day.UvIndexMax.Value}: ";
            double uv = hour.UvIndex.Value;
            switch (uv)
            {
                case < 3.0:
                    healthSummary += $"Low/Green. You can safely enjoy being outside.";
                    break;
                case >= 3.0 and < 6.0:
                    healthSummary += $"Moderate/Yellow. Seek shade during the afternoon hours. Put on a shirt, use sunscreen, and wear a hat.";
                    uvIndexColor = Colors.Yellow;
                    break;
                case >= 6.0 and < 8.0:
                    healthSummary += $"High/Orange. Seek shade during the afternoon hours. Put on a shirt, use sunscreen, and wear a hat.";
                    uvIndexColor = Colors.Orange;
                    break;
                case >= 8.0 and < 11.0:
                    healthSummary += $"Very High/Red. Avoid being outside during afternoon hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    uvIndexColor = Colors.Red;
                    break;
                case >= 11.0:
                    healthSummary += $"Extreme/Purple. Protect yourself from the sun. Avoid being outside during afternoon hours. Make sure you seek shade. Shirt, sunscreen and hat are a must.";
                    uvIndexColor = Colors.Purple;
                    break;
                default:
                    healthSummary += " unreported.";
                    break;
            }

            var wet = hour.WetBulbTemperature2m.Value;
            if (hour.WetBulbTemperature2m.Unit.Contains("F"))
            {
                switch (wet)
                {
                    case <= 85.0:
                        healthSummary += $" The wet-bulb temperature is {hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is safe for outdoor activities.";
                        break;
                    case > 85.0 and <= 88.0:
                        healthSummary += $" The wet-bulb temperature is {hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is a moderate risk for outdoor activities.";
                        break;
                    case >= 88.0 and < 90.0:
                        healthSummary += $" The wet-bulb temperature is {hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is a high risk for outdoor activities.";
                        break;
                    case > 90:
                        healthSummary += $" The wet-bulb temperature is{hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is an extreme risk for outdoor activities.";
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
                        healthSummary += $" The wet-bulb temperature is {hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is safe for outdoor activities.";
                        break;
                    case > 29 and <= 31:
                        healthSummary += $" The wet-bulb temperature is {hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is a moderate risk for outdoor activities.";
                        break;
                    case >= 31 and < 32:
                        healthSummary += $" The wet-bulb temperature is {hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is a high risk for outdoor activities.";
                        break;
                    case > 32:
                        healthSummary += $" The wet-bulb temperature is{hour.WetBulbTemperature2m.Value}{hour.WetBulbTemperature2m.Unit}, which is an extreme risk for outdoor activities.";
                        break;
                    default:
                        break;
                }
            }
            return (healthSummary, uvIndexColor);
        }
    }
}
