using System.Diagnostics;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;


namespace ThePhotographersForecast.Services;

public class GetGfsData
{
    private GridDetailData.Rootobject SuppData { get; set; }

    private readonly HttpClient _client;
    private GetTimesOfDayData _timeOfDayData;
    private CancellationTokenSource cts;
    public GetGfsData(IHttpClientFactory factory,  GetTimesOfDayData timeOfDayData)
    {
        _client = factory.CreateClient("photographer") ?? new HttpClient();
        _timeOfDayData = timeOfDayData;
    }
    public async Task<bool> GetWeatherAsync(DateTime[] dates, double latitude, double longitude)
    {
        foreach (var date in dates)
        {
            await _timeOfDayData.GetDaysAsync(date, latitude, longitude);
        }
        List<GfsHourlyTimeTemp> timeTemp = new List<GfsHourlyTimeTemp>();

        try
        {
            var data = new CoordinatesToGridData.Rootobject();
            string apiString = $@"https://api.weather.gov/points/{latitude},{longitude}";
            // var data = await Services.JsonDataService.GetJsonDataAsync<CoordinatesToGridData.Rootobject>(apiString);
            var request = new HttpRequestMessage(HttpMethod.Get, apiString);
            //request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PhotographersDay", "3.0"));
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return false;

            data = await response.Content.ReadFromJsonAsync<CoordinatesToGridData.Rootobject>() ?? new CoordinatesToGridData.Rootobject();



            if (data == null)
                return false;

            var gfsData = data.properties;

            SS.SelectedLocation.City = gfsData.relativeLocation.properties.city;
            SS.SelectedLocation.State = gfsData.relativeLocation.properties.state;
            SS.SelectedLocation.TimeZone = gfsData.timeZone;
            SS.SelectedLocation.Latitude = latitude;
            SS.SelectedLocation.Longitude = longitude;


            // GridDailyData.RootObject dailyForecast = new GridDailyData.RootObject();

            var dailyRequest = new HttpRequestMessage(HttpMethod.Get, gfsData.forecast);
            //dailyRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PhotographersDay", "3.0"));
            var dailyResponse = await _client.SendAsync(dailyRequest, HttpCompletionOption.ResponseHeadersRead);
            if (!dailyResponse.IsSuccessStatusCode)
                return false;

            var dailyForecast = await dailyResponse.Content.ReadFromJsonAsync<GridDailyData.RootObject>();


            if (dailyForecast == null)
                return false;

            int period = dailyForecast.properties.periods.Length;

            var dayForecasts = new GfsDailyForecast[period];


            for (int i = 0; i < period; i++)
            {
                var icon = dailyForecast.properties.periods[i].icon;
                var imgSrc = await GetHourlyImageSource(icon.Replace("medium", "large"));

                dayForecasts[i] = new GfsDailyForecast
                {
                    Number = (int)dailyForecast.properties.periods[i].number,
                    Name = dailyForecast.properties.periods[i].name,
                    StartTime = dailyForecast.properties.periods[i].startTime,
                    EndTime = dailyForecast.properties.periods[i].endTime,
                    IsDaytime = dailyForecast.properties.periods[i].isDaytime,
                    DailyIcon = icon,
                    DailyImageSource = imgSrc,
                    ChanceOfPrecipitation = (int)dailyForecast.properties.periods[i].probabilityOfPrecipitation.value,
                    PrecipitationUnitCode = dailyForecast.properties.periods[i].probabilityOfPrecipitation.unitCode,
                    DetailForecast = dailyForecast.properties.periods[i].detailedForecast,
                    WindSpeedDirection = $"Winds are blowing from the {GetWindDirection(dailyForecast.properties.periods[i].windDirection).ToLowerInvariant()}  at: {dailyForecast.properties.periods[i].windSpeed}.",
                };
            }



            GridHourlyData.Rootobject hourlyForecast = new GridHourlyData.Rootobject();
            var hourlyRequest = new HttpRequestMessage(HttpMethod.Get, gfsData.forecastHourly);
            //hourlyRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PhotographersDay", "3.0"));
            var hourlyResponse = await _client.SendAsync(hourlyRequest, HttpCompletionOption.ResponseHeadersRead);
            if (hourlyResponse.IsSuccessStatusCode)
            {
                hourlyForecast = await hourlyResponse.Content.ReadFromJsonAsync<GridHourlyData.Rootobject>() ?? new GridHourlyData.Rootobject();
            }
            if (hourlyForecast == null)
                return false;

            int pd = hourlyForecast.properties.periods.Length;

            var hourlyForecasts = new GfsHourlyForecast[pd];
            for (int d = 0; d < pd; d++)
            {
                var icon = hourlyForecast.properties.periods[d].icon;
                var imgSrc = await GetHourlyImageSource(icon.Replace("medium", "large"));

                hourlyForecasts[d] = new GfsHourlyForecast
                {
                    Number = hourlyForecast.properties.periods[d].number,
                    StartTime = hourlyForecast.properties.periods[d].startTime,
                    EndTime = hourlyForecast.properties.periods[d].endTime,
                    IsDaytime = hourlyForecast.properties.periods[d].isDaytime,
                    Temperature = hourlyForecast.properties.periods[d].temperature,
                    TemperatureUnit = $"°{hourlyForecast.properties.periods[d].temperatureUnit}",
                    PrecipitationUnitCode = hourlyForecast.properties.periods[d].probabilityOfPrecipitation.unitCode,
                    ChanceOfPrecipitation = (int)hourlyForecast.properties.periods[d].probabilityOfPrecipitation.value,
                    Dewpoint = hourlyForecast.properties.periods[d].dewpoint.unitCode == "wmoUnit:degC" ? (int)Math.Round((hourlyForecast.properties.periods[d].dewpoint.value * 32) * 1.8, 0) : (int)Math.Round(hourlyForecast.properties.periods[d].dewpoint.value, 0),
                    DewpointUnit = hourlyForecast.properties.periods[d].dewpoint.unitCode == "wmoUnit:degC" ? "°F" : "°C",
                    Humidity = $"{hourlyForecast.properties.periods[d].relativeHumidity.value}%",
                    HourlyIcon = icon,
                    WindDirection = hourlyForecast.properties.periods[d].windDirection,
                    WindSpeed = hourlyForecast.properties.periods[d].windSpeed,
                    ShortForecast = hourlyForecast.properties.periods[d].shortForecast, //WR
                    HourlyImageSource = imgSrc
                };

                var hourly = hourlyForecasts[d];
                // inside the outer Parallel.ForAsync body (after assigning gfsDataService.HourlyForecasts[d])
                //WR
                string feelLike = string.Empty;
                switch (hourly.Dewpoint)
                {
                    case <= 55:
                        if ((double)hourly.Dewpoint / hourly.Temperature < 80.0)
                        {
                            feelLike = "comfortable.";
                        }
                        else
                        {
                            feelLike = "there's a chance of fog or rain, otherwise its comfortable.";
                        }

                        break;
                    case > 55 and <= 60:
                        feelLike = "slightly humid, but generally comfortable.";
                        break;
                    case > 60 and <= 65:
                        feelLike = "noticeable humid, not too comfortable.";
                        break;
                    case > 65 and <= 70:
                        feelLike = "sticky, humid, and uncomfortable.";
                        break;
                    case > 70:
                        feelLike = "oppressively humid, like walking through a wall of water, and wicked uncomfortable. Be careful!";
                        break;
                }

                hourly.DetailForecast =
                       $"{hourly.IntroTimeOfDay}. The forecast is {hourly.ShortForecast.ToLowerInvariant()} throughout " +
                       $"the {hourly.StartTime.ToString("htt")}. The temperature will be {hourly.Temperature}{hourly.TemperatureUnit}, with winds blowing " +
                       $"from the {GetWindDirection(hourly.WindDirection).ToLowerInvariant()} at {hourly.WindSpeed}. The great outdoors feels {feelLike} " +
                       $"{Environment.NewLine}Relative Humidity: {hourly.Humidity} " +
                       $"{Environment.NewLine}Dewpoint: {hourly.Dewpoint}{hourly.DewpointUnit}" +
                       $"{Environment.NewLine}Chance Of Precipitation: {hourly.ChanceOfPrecipitation}%";

            }
            //SS.Hours = [.. hourlyForecasts.Select(x => x.StartTime).Distinct()];

            for (int w = 0; w < dayForecasts.Length; w++)
            {
                dayForecasts[w].HourlyWeathers = hourlyForecasts.Where(h => h.StartTime >= dayForecasts[w].StartTime && h.EndTime <= dayForecasts[w].EndTime).ToArray();
            }

            for (int h = 0; h < dayForecasts.Length; h++)
            {
                dayForecasts[h].HourlyWeathers.MinBy(x => x.Temperature)?.IsLow = true;
                dayForecasts[h].HourlyWeathers.MaxBy(x => x.Temperature)?.IsHigh = true;
            }

            HourlyForecastUpdateHighLow(ref hourlyForecasts, dayForecasts);



            var hourlyCollection = await BuildHourlyCollection(hourlyForecasts);

            var eventData = SS.EventsData.SelectMany(m => m.EventTimes).ToArray();

            //for (int e = 0; e < eventData.Length; e++)
            //{
            //    eventData[e] = new EventTime
            //    {
            //        EventName = eventData[e].EventName,
            //        AstroTime = eventData[e].AstroTime,
            //        OtherTime = eventData[e].OtherTime == DateTime.MinValue ? null : eventData[e].OtherTime,
            //        Date = eventData[e].Date
            //    };
            //}


            var dailyCollection = await BuildDailyCollection(dayForecasts, hourlyCollection);

            SS.GfsData = new GfsWeatherModel();
            SS.GfsData.DailyForecasts = new GfsDailyForecast[dayForecasts.Length];
            SS.GfsData.HourlyForecasts = new GfsHourlyForecast[hourlyForecasts.Length];
            SS.DailyCollection = new MappedDaily[dailyCollection.Length];
            SS.HourlyCollection = new MappedHourly[hourlyCollection.Length];

            for (int d = 0; d < dayForecasts.Length; d++) { SS.GfsData.DailyForecasts[d] = new GfsDailyForecast(); SS.GfsData.DailyForecasts[d] = dayForecasts[d]; }
            for (int h = 0; h < hourlyForecasts.Length; h++) { SS.GfsData.HourlyForecasts[h] = new GfsHourlyForecast(); SS.GfsData.HourlyForecasts[h] = hourlyForecasts[h]; }
            for (int dc = 0; dc < dailyCollection.Length; dc++) { SS.DailyCollection[dc] = new MappedDaily(); SS.DailyCollection[dc] = dailyCollection[dc]; }
            for (int hc = 0; hc < hourlyCollection.Length; hc++) { SS.HourlyCollection[hc] = new MappedHourly(); SS.HourlyCollection[hc] = hourlyCollection[hc]; }

            SS.StartDateTime = dayForecasts[0].StartTime;
            SS.EndDateTime = dayForecasts[dayForecasts.Length - 1].EndTime;

            SS.DailyStartTimes = [.. dayForecasts.Select(x => x.StartTime).Distinct()];
            SS.Hours = [.. hourlyForecasts.Select(x => x.StartTime).Distinct()];



            SS.IsComplete = true;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: {ex.Message}.{ex.InnerException}");
            return false;
            throw new Exception("There's an error processing the GfsData file", ex);
        }

    }

    private void HourlyForecastUpdateHighLow(ref GfsHourlyForecast[] hourlyForecasts, GfsDailyForecast[] dayForecasts)
    {

        List<GfsHourlyForecast> highTemps = dayForecasts.SelectMany(x => x.HourlyWeathers).Where(y => y.IsHigh == true).ToList();
        var startTimes = highTemps.Select(x => x.StartTime).Distinct().ToArray();

        List<GfsHourlyForecast> lowTemps = dayForecasts.SelectMany(x => x.HourlyWeathers).Where(y => y.IsLow == true).ToList();
        var lowTimes = lowTemps.Select(x => x.StartTime).Distinct().ToArray();
        for (int i = 0; i < hourlyForecasts.Length; i++)
        {
            if (startTimes.Contains(hourlyForecasts[i].StartTime))
            {
                hourlyForecasts[i].IsHigh = true;
            }
            if (lowTimes.Contains(hourlyForecasts[i].StartTime))
            {
                hourlyForecasts[i].IsLow = true;
            }
        }
    }

    private async Task<MappedHourly[]> BuildHourlyCollection(GfsHourlyForecast[] hourlyForecasts)
    {

        MappedHourly[] hourlies = new MappedHourly[hourlyForecasts.Length];
        for (int i = 0; i < hourlyForecasts.Length; i++)
        {
            var y = hourlyForecasts[i];
            hourlies[i] = new MappedHourly
            {
                StartTime = y.StartTime,
                EndTime = y.EndTime,
                IsDay = y.IsDaytime,
                HourlyTemperature = y.Temperature,
                HourlyTemperatureUnits = y.TemperatureUnit,
                IsDailyHigh = y.IsHigh,
                IsDailyLow = y.IsLow,
                HourlyForecast = y.DetailForecast,
                HourlyImageSource = y.HourlyImageSource
            };
        }
        return hourlies;
    }

    private async Task<MappedDaily[]> BuildDailyCollection(GfsDailyForecast[] dayForecasts, MappedHourly[] hourlies)
    {
        MappedDaily[] dailies = new MappedDaily[dayForecasts.Length];
        for (int i = 0; i < dayForecasts.Length; i++)
        {
            var d = dayForecasts[i];
            dailies[i] = new MappedDaily
            {
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                IsDay = d.IsDaytime,
                Name = d.Name,
                DailyTemperature = d.Temperature,
                DailyTemperatureUnits = d.TemperatureUnit,
                DailyHigh = $"with a high of {d.HourlyWeathers.Where(x => x.IsHigh == true).First().Temperature}°{d.TemperatureUnit} at {d.HourlyWeathers.Where(x => x.IsHigh == true).First().StartTime.ToString("h tt")}",
                DailyLow = $"with a low of {d.HourlyWeathers.Where(x => x.IsLow == true).First().Temperature}°{d.TemperatureUnit} at {d.HourlyWeathers.Where(x => x.IsLow == true).First().StartTime.ToString("h tt")}",
                DailyImageSource = d.DailyImageSource,
                DailyForecast = d.DetailForecast,
                HourlyCollection = hourlies.Where(h => h.StartTime >= d.StartTime && h.EndTime <= d.EndTime).ToArray(),
                AstroData = SS.EventsData.Select(x => x.AstronomicalData).Where(a => a.Date == d.StartTime.Date).First() ?? new DailyAstronomicalData(),
                DailyEvents = SS.EventsData.SelectMany(x => x.EventTimes).Where(e => e.WeatherHour >= d.StartTime && e.WeatherHour < d.EndTime).ToArray()
            };
        }
        return dailies;
    }

    //Values Needed
    //ApparentTemperature
    //WetBulbGlobeTemperature
    //HeatIndex
    //WindChill
    //SkyCover
    //WindGust
    //Weather
    //HeatRisk
    private async Task<GfsHourlySupplement> HourlySupplimentalAsync(string gridUrl, GfsHourlyForecast hourly)
    {

        SuppData = new GridDetailData.Rootobject();
        var hourlySuppRequest = new HttpRequestMessage(HttpMethod.Get, gridUrl);
        var results = await _client.SendAsync(hourlySuppRequest, HttpCompletionOption.ResponseHeadersRead);
        if (results.IsSuccessStatusCode)
        {
            SuppData = await results.Content.ReadFromJsonAsync<GridDetailData.Rootobject>() ?? new GridDetailData.Rootobject();
        }
        if (SuppData != null)
        {
            var data = SuppData.properties.apparentTemperature;
            for (int i = 0; i < data.values.Length; i++)
            {
            }
        }
        return new GfsHourlySupplement();
    }

    public static string GetWindDirection(string windDirection)
    {
        string direction = string.Empty;
        switch (windDirection.ToLowerInvariant())
        {
            case "n":
                return "North";

            case "nne":
                return "North-Northeast";

            case "ne":
                return "Northeast";

            case "ene":
                return "East-Northeast";

            case "e":
                return "East";

            case "ese":
                return "East-Southeast";

            case "s":
                return "South";

            case "ssw":
                return "South-Southwest";

            case "sw":
                return "Southwest";

            case "wsw":
                return "West-Southwest";

            case "w":
                return "West";

            case "wnw":
                return "West-Northwest";

            case "nw":
                return "Northwest";

            case "nnw":
                return "North-Northwest";

            case "sse":
                return "South-Southeast";

            default:
                return "Unknown";
        }
    }

    private async Task<ImageSource> GetHourlyImageSource(string icon)
    {
        //Modify Uri to local resource
        var imageName = string.Concat(icon.Split("/").Last().Split("?")[0], ".png");
        ImageSource imageSource = ImageSource.FromFile(imageName);
        return imageSource;
    }


    private DateTimeOffset ParseNwsInterval(string interval)
    {
        // Splits "2024-05-06T12:00:00+00:00/PT1H" and takes the first part
        var parts = interval.Split('/');
        return DateTimeOffset.Parse(parts[0]);
    }
}

// Stores all the time and temps for Daily highs and lows
//var hourlyTimeTemps = new System.Collections.ObjectModel.ObservableCollection<GfsHourlyTimeTemp>();

// Create a temporary List for easy loading all the timetemps at once to the gfsDataService ObservableObject

//Group hourly forecasts by day
//    var hf = dai hourlyForecasts.Where(x => x.StartTime >= new DateTime(x.StartTime.Year,x.StartTime.Month,x.StartTime.Day,6,0,0) && x.EndTime < new DateTime(x.EndTime.Year, x.EndTime.Month, x.EndTime.Day, 18, 0, 0) || x.StartTime >= new DateTime(x.StartTime.Year, x.StartTime.Month, x.StartTime.Day, 18, 0, 0) && x.EndTime < new DateTime(x.EndTime.Year, x.EndTime.Month, x.EndTime.Day, 6, 0, 0)).OrderBy(a=>a.StartTime).GroupBy(y => y.StartTime.Date).OrderBy(z => z.Key).ToArray();

// Iterate through the groups to find the highs and low usig a foreeach
//IGrouping<DateTime, GfsHourlyForecast>? grouped = dayArray[q].GroupBy(d=>d.StartTime);
//// Use MinBy and MaxBy to get the values
//var low = grouped.MinBy(l => l.Temperature);
//var high = grouped.MaxBy(h => h.Temperature);

//if (low != null && high != null)
//{
//    low.IsLow = true;
//    high.IsHigh = true;
//}
//// Add records to the TimeTemp Gfs Object
//timeTemp.Add(new GfsHourlyTimeTemp
//{
//    High = new DailyHigh() { Time = high.StartTime, Temp = high.Temperature },
//    Low = new DailyLow() { Time = low.StartTime, Temp = low.Temperature }
//});
