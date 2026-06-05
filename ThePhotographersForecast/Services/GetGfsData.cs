using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;


namespace ThePhotographersForecast.Services;

public partial class GetGfsData : ObservableObject
{
    [ObservableProperty]
    private GfsWeatherModel gfsDataService;

    public static GridDailyData DailyForecasts { get; set; } = new GridDailyData();
    public static GridHourlyData HourlyForecasts { get; set; } = new GridHourlyData();

    private readonly IHttpClientFactory _factory;
    public GetGfsData(IHttpClientFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        GfsDataService = new GfsWeatherModel();
    }
    public async Task<GfsWeatherModel> GetWeatherAsync(double latitude, double longitude)
    {
        var _client = _factory.CreateClient("nws");
        try
        {
            var data = new CoordinatesToGridData.Rootobject();
            string apiString = $@"https://api.weather.gov/points/{latitude},{longitude}";
            // var data = await Services.JsonDataService.GetJsonDataAsync<CoordinatesToGridData.Rootobject>(apiString);
            var request = new HttpRequestMessage(HttpMethod.Get, apiString);
            request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PhotographersDay", "3.0"));
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadFromJsonAsync<CoordinatesToGridData.Rootobject>() ?? new CoordinatesToGridData.Rootobject();
            }


            if (data != null)
            {
                var gfsData = data.properties;

                ServiceHelper.GetService<PhotoLocation>().City = gfsData.relativeLocation.properties.city;
                ServiceHelper.GetService<PhotoLocation>().State = gfsData.relativeLocation.properties.state;
                ServiceHelper.GetService<PhotoLocation>().TimeZone = gfsData.timeZone;
                ServiceHelper.GetService<PhotoLocation>().Latitude = latitude;
                ServiceHelper.GetService<PhotoLocation>().Longitude = longitude;


                GridDailyData.RootObject dailyForecast = new GridDailyData.RootObject();

                var dailyRequest = new HttpRequestMessage(HttpMethod.Get, gfsData.forecast);
                //dailyRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PhotographersDay", "3.0"));
                var dailyResponse = await _client.SendAsync(dailyRequest, HttpCompletionOption.ResponseHeadersRead);
                if (dailyResponse.IsSuccessStatusCode)
                {
                    dailyForecast = await dailyResponse.Content.ReadFromJsonAsync<GridDailyData.RootObject>() ?? new GridDailyData.RootObject();
                }

                if (dailyForecast != null)
                {
                    int period = dailyForecast.properties.periods.Length;
                    ServiceHelper.GetService<SelectLocationViewModel>().ForecastCount = period;
                    GfsDataService.DailyForecasts = new GfsDailyForecast[period];

                    await Parallel.ForAsync(0, period, async (i, ct) =>
                    {
                        GfsDataService.DailyForecasts[i] = new GfsDailyForecast
                        {
                            Number = (int)dailyForecast.properties.periods[i].number,
                            Name = dailyForecast.properties.periods[i].name,
                            StartTime = dailyForecast.properties.periods[i].startTime,
                            EndTime = dailyForecast.properties.periods[i].endTime,
                            IsDaytime = dailyForecast.properties.periods[i].isDaytime,
                            DailyIcon = dailyForecast.properties.periods[i].icon,
                            ChanceOfPrecipitation = (int)dailyForecast.properties.periods[i].probabilityOfPrecipitation.value,
                            PrecipitationUnitCode = dailyForecast.properties.periods[i].probabilityOfPrecipitation.unitCode,
                            DetailForecast = dailyForecast.properties.periods[i].detailedForecast,
                            WindSpeedDirection = dailyForecast.properties.periods[i].windDirection + " winds at: " + dailyForecast.properties.periods[i].windSpeed,
                        };
                    });
                }
                ;

                GridHourlyData.Rootobject hourlyForecast = new GridHourlyData.Rootobject();
                var hourlyRequest = new HttpRequestMessage(HttpMethod.Get, gfsData.forecastHourly);
                //hourlyRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PhotographersDay", "3.0"));
                var hourlyResponse = await _client.SendAsync(hourlyRequest, HttpCompletionOption.ResponseHeadersRead);
                if (hourlyResponse.IsSuccessStatusCode)
                {
                    hourlyForecast = await hourlyResponse.Content.ReadFromJsonAsync<GridHourlyData.Rootobject>() ?? new GridHourlyData.Rootobject();
                }
                if (hourlyForecast != null)
                {
                    int pd = hourlyForecast.properties.periods.Length;
                    GfsDataService.HourlyForecasts = new GfsHourlyForecast[pd];

                    await Parallel.ForAsync(0, pd, async (d, ct) =>
                    {
                        GfsDataService.HourlyForecasts[d] = new GfsHourlyForecast
                        {
                            Number = hourlyForecast.properties.periods[d].number,
                            StartTime = hourlyForecast.properties.periods[d].startTime,
                            EndTime = hourlyForecast.properties.periods[d].endTime,
                            IsDaytime = hourlyForecast.properties.periods[d].isDaytime,
                            Temperature = hourlyForecast.properties.periods[d].temperature,
                            TemperatureUnit = hourlyForecast.properties.periods[d].temperatureUnit,
                            PrecipitationUnitCode = hourlyForecast.properties.periods[d].probabilityOfPrecipitation.unitCode,
                            ChanceOfPrecipitation = (int)hourlyForecast.properties.periods[d].probabilityOfPrecipitation.value,
                            Dewpoint = hourlyForecast.properties.periods[d].dewpoint.unitCode == "wmoUnit:degC" ? $"{Math.Round((hourlyForecast.properties.periods[d].dewpoint.value * 1.8) + 32, 0)}°F" : "N/A",
                            DewpointUnit = hourlyForecast.properties.periods[d].dewpoint.unitCode == "wmoUnit:degC" ? "°F" : "°C",
                            Humidity = $"{hourlyForecast.properties.periods[d].relativeHumidity.value}%",
                            HourlyIcon = hourlyForecast.properties.periods[d].icon,
                            WindDirection = hourlyForecast.properties.periods[d].windDirection,
                            WindSpeed = hourlyForecast.properties.periods[d].windSpeed,
                            ShortForecast = hourlyForecast.properties.periods[d].shortForecast,

                        };
                    });
                    await Parallel.ForAsync(0, GfsDataService.HourlyForecasts.Length, async (h, ct) =>
                     {
                         // inside the outer Parallel.ForAsync body (after assigning GfsDataService.HourlyForecasts[d])


                         var hf = GfsDataService.HourlyForecasts[h];
                         if (hf != null)
                         {
                             string feelLike = string.Empty;
                             switch (double.Parse(hf.Dewpoint.Replace("°F", "")))
                             {
                                 case <= 55:
                                     feelLike = "comfortable. Do what you want to do.";
                                     break;
                                 case > 55 and <= 60:
                                     feelLike = "slightly humid, but generally comfortable.";
                                     break;
                                 case > 60 and <= 65:
                                     feelLike = "noticeable humid, not comfortable.";
                                     break;
                                 case > 65 and <= 70:
                                     feelLike = "sticky, humid, and uncomfortable.";
                                     break;
                                 case > 70:
                                     feelLike = "oppressively humid, like walking through a wall of water, and wicked uncomfortable. Be careful!";
                                     break;
                             }


                             GfsDataService.HourlyForecasts[h].DetailForecast =
                                        $"Good {FromTimesOfDay(hf.StartTime.Hour)}. Expect at {hf.StartTime:h tt}, {hf.ShortForecast.ToLowerInvariant()} during " +
                                        $"the {FromTimesOfDay(hf.StartTime.Hour)}. The temperature will be {hf.Temperature}°{hf.TemperatureUnit}. Winds blowing " +
                                        $"from the {GetWindDirection(hf.WindDirection).ToLowerInvariant()} at {hf.WindSpeed}. The great outdoors feels {feelLike} " +
                                        $"{Environment.NewLine}Relative Humidity: {hf.Humidity} Dewpoint:{hf.Dewpoint}" +
                                        $"{Environment.NewLine} Chance Of Precipitation: {hf.ChanceOfPrecipitation}%";
                         }

                     });
                    //if (GfsDataService.HourlyForecasts.Length == pd)
                    //{
                    //    if (GfsDataService.DailyForecasts.Length > 1)
                    //        await Parallel.ForAsync(0, GfsDataService.DailyForecasts.Length, async (h, ct) =>
                    //        {
                    // 1. Get the local bounds for the current daily forecast index
                    var startTime = GfsDataService.HourlyForecasts.Min(h => h.StartTime);
                    var current = startTime;
                    var endTime = GfsDataService.HourlyForecasts.Max(h => h.EndTime);
                    var dayCount = endTime.Date.Subtract(startTime.Date).Days + 1;
                    GfsDataService.HourlyTimeTemps = new GfsHourlyTimeTemp[dayCount];
                    for (int i = 0; i < dayCount; i++)
                    {
                        var endOfDay = current.Date.AddDays(1).AddHours(-1);
                        var highLowRange = GfsDataService.HourlyForecasts.AsEnumerable()
                            .Where(t => t.StartTime >= current && t.EndTime <= endOfDay)
                            .OrderByDescending(t => t.Temperature).Select(x => new DailyHigh() { Time = x.StartTime, Temp = x.Temperature }).OrderByDescending(s => s.Temp).ToArray();
                        if (highLowRange.Length == 0)
                        {
                            GfsDataService.HourlyTimeTemps[i] = new GfsHourlyTimeTemp();
                        }
                        else
                        {
                            var high = highLowRange[0];
                            var low = highLowRange[highLowRange.Length - 1];

                            GfsDataService.HourlyTimeTemps[i] = new GfsHourlyTimeTemp
                            {
                                High = new DailyHigh() { Time = high.Time, Temp = high.Temp },
                                Low = new DailyLow() { Time = low.Time, Temp = low.Temp }
                            };
                        }

                        current = current.Date.AddDays(1);
                    }


                    //if (GfsDataService.DailyForecasts[h].IsDaytime)
                    //{
                    //    //3. Filter the hourly temps for the maximum temperature
                    //    var highestHourlyRecord = GfsDataService.HourlyForecasts
                    //        .Where(t => t.StartTime >= startTime && t.EndTime <= endTime)
                    //        .OrderByDescending(t => t.Temperature).FirstOrDefault(new GfsHourlyForecast { Temperature = 0, StartTime = startTime });
                    //    GfsDataService.DailyForecasts[h].DailyHighTemp = highestHourlyRecord.Temperature;
                    //    GfsDataService.DailyForecasts[h].DailyHighTime = highestHourlyRecord.StartTime;
                    //}
                    //else if (!GfsDataService.DailyForecasts[h].IsDaytime)
                    //{
                    //    // 2. Filter the hourly temps once and find the record with the minimum temperature
                    //    var lowestHourlyRecord = GfsDataService.HourlyForecasts
                    //        .Where(t => t.StartTime >= startTime && t.EndTime <= endTime)
                    //        .OrderBy(t => t.Temperature).FirstOrDefault(new GfsHourlyForecast { Temperature = 0, StartTime = startTime });
                    //    GfsDataService.DailyForecasts[h].DailyLowTemp = lowestHourlyRecord.Temperature;
                    //    GfsDataService.DailyForecasts[h].DailyLowTime = lowestHourlyRecord.StartTime;
                    //}


                    //    // 3. Format and assign the string safely if a record was found
                    //    if (lowestHourlyRecord != null)
                    //    GfsDataService.DailyForecasts[h].DailyLow = $"{lowestHourlyRecord.Temperature}°{lowestHourlyRecord.TemperatureUnit} at {lowestHourlyRecord.StartTime:h tt}";
                    //if (highestHourlyRecord != null)
                    //    GfsDataService.DailyForecasts[h].DailyHigh = $"{highestHourlyRecord.Temperature}°{highestHourlyRecord.TemperatureUnit} at {highestHourlyRecord.StartTime:h tt}";
                    //      });
                    // }
                }
            }
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: {ex.Message}.{ex.InnerException}");
        }
        return GfsDataService;
    }

    public string GetWindDirection(string windDirection)
    {
        string direction = string.Empty;
        switch (windDirection.ToLowerInvariant())
        {
            case "n":
                direction = "North";
                break;
            case "nne":
                direction = "North-Northeast";
                break;
            case "ne":
                direction = "Northeast";
                break;
            case "ene":
                direction = "East-Northeast";
                break;
            case "e":
                direction = "East";
                break;
            case "ese":
                direction = "East-Southeast";
                break;
            case "s":
                direction = "South";
                break;
            case "ssw":
                direction = "South-Southwest";
                break;
            case "sw":
                direction = "Southwest";
                break;
            case "wsw":
                direction = "West-Southwest";
                break;
            case "w":
                direction = "West";
                break;
            case "wnw":
                direction = "West-Northwest";
                break;
            case "nw":
                direction = "Northwest";
                break;
            case "nnw":
                direction = "North-Northwest";
                break;
            case "sse":
                direction = "South-Southeast";
                break;
            default:
                direction = "Unknown";
                break;
        }
        return direction;
    }
    public string FromTimesOfDay(int hour)
    {
        switch (hour)
        {
            case int h when (0 <= h && h < 12):
                return "morning";
            case int h when (12 <= h && h < 18):
                return "afternoon";
            case int h when (18 <= h && h < 22):
                return "evening";
            case int h when (22 <= h && h < 24):
                return "night";
            default:
                return string.Empty;
        }
    }

    private DateTimeOffset ParseNwsInterval(string interval)
    {
        // Splits "2024-05-06T12:00:00+00:00/PT1H" and takes the first part
        var parts = interval.Split('/');
        return DateTimeOffset.Parse(parts[0]);
    }
}

//      }
//}


//                //    GridDetailData.Rootobject details = new GridDetailData.Rootobject();
//                //    using (var httpClient = new HttpClient())
//                //    {
//                //        var request = new HttpRequestMessage(HttpMethod.Get, gfs.ForecastGridDataUrl);
//                //        request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PfApp", "3.0"));
//                //        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
//                //        if (response.IsSuccessStatusCode)
//                //        {
//                //            details = await response.Content.ReadFromJsonAsync<GridDetailData.Rootobject>() ?? new GridDetailData.Rootobject();
//                //        }

//                //    }

//                //    if (details != null)
//                //    {
//                //        for (int i = 0; i < gfs.HourlyForecasts.Length; i++)
//                //        {
//                //            // 1. Find the matching data point first to avoid multiple parses
//                //            var windChill = details.properties.windChill.values
//                //            .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));

//                //            // 2. Perform the transformation and assignment
//                //            gfs.HourlyForecasts[i].WindChill = windChill?.value != null
//                //                ? $"{Math.Round(((double)windChill.value * 1.8) + 32, 0)}°F"
//                //                : "N/A";

//                //            // 1. Find the matching data point first to avoid multiple parses
//                //            var windGusts = details.properties.windGust.values
//                //            .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));

//                //            // 2. Perform the transformation and assignment
//                //            gfs.HourlyForecasts[i].WindGust = windGusts?.value != null
//                //                ? $"{Math.Round(((double)windGusts.value * 0.621371), 2)} mph"
//                //                : "N/A";

//                //            // 1. Find the matching data point first to avoid multiple parses
//                //            var heatIndex = details.properties.heatIndex.values
//                //            .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));

//                //            // 2. Perform the transformation and assignment
//                //            gfs.HourlyForecasts[i].HeatIndex = heatIndex?.value != null
//                //                ? $"{Math.Round(((double)heatIndex.value * 1.8) + 32, 0)}°F"
//                //                : "N/A";
//                //            var apparentTemp = details.properties.apparentTemperature.values
//                //                .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));
//                //            gfs.HourlyForecasts[i].ApparentTemperature = apparentTemp?.value != null
//                //                ? $"{Math.Round(((double)apparentTemp.value * 1.8) + 32, 0)}°F"
//                //                : "N/A";
//                //            var skyCover = details.properties.skyCover.values
//                //                .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));
//                //            gfs.HourlyForecasts[i].SkyCover = skyCover?.value != null
//                //                ? $"{skyCover.value}%"
//                //                : "N/A";
//                //            var wBGT = details.properties.wetBulbGlobeTemperature.values
//                //                .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));
//                //            gfs.HourlyForecasts[i].WetBulbGlobeTemperature = wBGT?.value != null
//                //                ? $"{Math.Round(((double)wBGT.value * 1.8) + 32, 0)}°F"
//                //                : "N/A";
//                //            var quantitativePrecipitation = details.properties.quantitativePrecipitation.values
//                //                .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));
//                //            gfs.HourlyForecasts[i].QuantitativePrecipitation = quantitativePrecipitation?.value != null
//                //            ? $"{(double)quantitativePrecipitation.value / 25.4} inches"
//                //            : "N/A";
//                //            var weatherValues = details.properties.weather.values
//                //                .FirstOrDefault(t => DateTime.Parse(t.validTime.Split('/')[0]) == DateTime.Parse(gfs.HourlyForecasts[i].StartTime.ToString("o")));
//                //            gfs.HourlyForecasts[i].Weather = weatherValues?.value != null
//                //                ? new GfsDetailForecastWeather
//                //                {
//                //                    Coverage = weatherValues.value[0].coverage,
//                //                    Intensity = weatherValues.value[0].intensity,
//                //                    Weather = weatherValues.value[0].weather,
//                //                    Attributes = weatherValues.value[0].attributes != null ? string.Join(", ", weatherValues.value[0].attributes) : "None",
//                //                    Visibility = weatherValues.value[0].visibility != null ? string.Concat(weatherValues.value[0].visibility.value) : "N/A"
//                //                }.WeatherSummary
//                //                : "N/A";
//                //        }
//                //    }

//            });
//        }
//    }
//}