using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;


namespace ThePhotographersForecast.Services;

public partial class GetNwsData : ObservableObject, IGetNwsData
{
    private readonly HttpClient _client;
    public Session SharedSession { get; set; }
    private IForecastService _forecastService;
    [ObservableProperty]
    public partial string DailyUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string HourlyUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string DetailUrl { get; set; } = string.Empty;

    public GetNwsData(IHttpClientFactory client, Session sharedSession, IForecastService forecastService)
    {
        _client = client.CreateClient("nwsClient");
        SharedSession = sharedSession;
        _forecastService = forecastService;
    }
    private async Task<T?> GetDataAsync<T>(HttpRequestMessage request)
    {
        try
        {
            // ensure response is disposed so connection returns to pool promptly
            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"ERROR: {_client.BaseAddress} FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                return default;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullNumberConverterFactory());
            var result = await JsonSerializer.DeserializeAsync<T>(stream, options);
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is ObjectDisposedException || ex is SocketException)
        {
            Debug.WriteLine($"Transient error: {ex.GetType().Name} - {ex.Message}");
            return default;
        }
    }

    public async Task<NwsWeatherModel> GetWeatherAsync(double latitude, double longitude)
    {

        NwsWeatherModel nwsData = new NwsWeatherModel();
        var data = new CoordinatesToGridData.Rootobject();
        try
        {
            string apiString = $"https://api.weather.gov/points/{Math.Round(latitude, 5)},{Math.Round(longitude, 5)}";

            var request = new HttpRequestMessage(HttpMethod.Get, apiString);
            data = await GetDataAsync<CoordinatesToGridData.Rootobject>(request);
            if (data == null) return default;

        }
        catch (ApplicationException ex)
        {
            Debug.WriteLine($"ERROR WHILE FETCHING NWS URLS: {ex.Message}.{ex.InnerException}");
            return default;
        }
        DailyUrl = data.properties.forecast;
        HourlyUrl = data.properties.forecastHourly;
        DetailUrl = data.properties.forecastGridData;

        // GOTO SelectLocationViewModel Line: 411
        SharedSession.SelectedLocation = new PhotoLocation
        {
            Latitude = latitude,
            Longitude = longitude,
            City = data.properties.relativeLocation.properties.city,
            State = data.properties.relativeLocation.properties.state,

        };
        SharedSession.SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(data.properties.timeZone);
        SharedSession.SelectedTimeZoneOffset = (int)SharedSession.SelectedTimeZone.GetUtcOffset(DateTime.UtcNow).TotalSeconds;
        Debug.WriteLine($"The Selected Location is now created on Session for Gps users. {SharedSession.SelectedLocation.City}, {SharedSession.SelectedLocation.State}");

        Debug.WriteLine($"Lat:{SharedSession.SelectedLocation.Latitude}");
        Debug.WriteLine($"Lon:{SharedSession.SelectedLocation.Longitude}");
        Debug.WriteLine($"DN:{SharedSession.SelectedLocation.DisplayName}");
        // Fetch independent endpoints concurrently to eliminate round-trip latency
        Debug.WriteLine($"{SharedSession.StartDate.ToString("F")}");
        Debug.WriteLine($"{SharedSession.EndDate.ToString("F")}");
        Task<NwsDailyForecast[]> dailyTask = GetNwsDailyForecastAsync(DailyUrl);
        Task<NwsHourlyForecast[]> hourlyTask = GetNwsHourlyForecastAsync(HourlyUrl);
        Task<NwsHourlySupplemental[]> supplementalTask = GetNwsHourlySupplementalAsync(DetailUrl);
        await Task.WhenAll(dailyTask, hourlyTask, supplementalTask);//.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

        if (dailyTask.IsCompletedSuccessfully && hourlyTask.IsCompletedSuccessfully && supplementalTask.IsCompletedSuccessfully)
        {
            var nwsDailies = await dailyTask;
            var nwsHourlies = await hourlyTask;
            var nwsHourlySupplementals = await supplementalTask;

            //var nwsDailies = dailyTask.Result;
            //var nwsHourlies = hourlyTask.Result;
            //var nwsHourlySupplementals = supplementalTask.Result;
            nwsData = await BuildTheNwsWeatherModelAsync(nwsDailies, nwsHourlies, nwsHourlySupplementals).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        else
        {
            Debug.WriteLine($"ERROR: One or more tasks failed. Daily Task: {dailyTask.Status}, Hourly Task: {hourlyTask.Status}, Supplemental Task: {supplementalTask.Status}");
            throw new Exception("One or more tasks failed while fetching NWS data.");
        }

        return nwsData;
    }
    private async Task<NwsDailyForecast[]> GetNwsDailyForecastAsync(string url)
    {
        var dailyForecast = new GridDailyData.Rootobject();
        try
        {
            var usUrl = String.Concat(url, "?units=us");
            var dailyRequest = new HttpRequestMessage(HttpMethod.Get, usUrl);
            // Adding the new Quantititative Features
            dailyRequest.Headers.Add("Feature-Flags", "forecast_temperature_qv,forecast_wind_speed_qv");
            dailyForecast = await GetDataAsync<GridDailyData.Rootobject>(dailyRequest);
            if (dailyForecast == null) return default;
            var UnitCode = Preferences.Default.Get("PrecipitationUnit", "inch");
            if (UnitCode == "mm")
            {
                MainThread.BeginInvokeOnMainThread(() => SharedSession.SelectedLocation.Elevation = new ElevationValue { Elevation = dailyForecast.properties.elevation.value, UnitCode = "m" });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => SharedSession.SelectedLocation.Elevation = new ElevationValue { Elevation = dailyForecast.properties.elevation.value * 3.2808399, UnitCode = "ft" });
            }

            var periods = dailyForecast.properties.periods;
            var nwsDailies = new NwsDailyForecast[periods.Length];

            for (int i = 0; i < periods.Length; i++)
            {
                var icon = periods[i].icon;
                var lotties = MeteoconsResolver.ResolveNwsDaily(icon);
                // var imgSrc = await GetHourlyImageSource(icon.Replace("medium", "large"));

                nwsDailies[i] = new NwsDailyForecast
                {

                    Number = periods[i].number,
                    Name = periods[i].name,
                    StartTime = periods[i].startTime,
                    EndTime = periods[i].endTime,
                    IsDaytime = periods[i].isDaytime,
                    Temperature = Math.Round(periods[i].temperature.value * 1.8 + 32, 0),
                    TemperatureUnit = $"°F",
                    DailyIcon = icon,
                    ChanceOfPrecipitation = periods[i].probabilityOfPrecipitation.value,
                    PrecipitationUnitCode = periods[i].probabilityOfPrecipitation.unitCode,
                    ShortForecast = periods[i].shortForecast,
                    DetailForecast = periods[i].detailedForecast,
                    WindSpeed = new WindSpeedValue { Value = Math.Round(periods[i].windSpeed.value / 1.609344, 0), Max = Math.Round(periods[i].windSpeed.maxValue / 1.609344, 0), Min = Math.Round(periods[i].windSpeed.minValue / 1.609344, 0), UnitCode = "mph" },
                    WindDirection = GetWindDirection(periods[i].windDirection),
                    WindGusts = new WindGustValue { Value = periods[i].windGust != null ? periods[i].windGust.value / 1.609344 : double.NaN, UnitCode = "mph" },
                    WindSpeedStatement = $"Winds from the {GetWindDirection(periods[i].windDirection).ToLowerInvariant()} at {Math.Round(periods[i].windSpeed.minValue / 1.609344, 0)} to {Math.Round(periods[i].windSpeed.maxValue / 1.609344, 0)} mph.",
                    SkDailyAnimation = lotties

                };
            }

            return nwsDailies;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: {ex.Message}.{ex.InnerException}");
            throw new Exception("There's an error processing the NwsData file", ex);
        }
        return null;
    }

    // Create the National Weather Service Hourly Forecasts (same format as the daily forecast)
    private async Task<NwsHourlyForecast[]> GetNwsHourlyForecastAsync(string url)
    {
        var hourlyForecasts = new GridHourlyData.Rootobject();
        try
        {
            var usUrl = String.Concat(url, "?units=us");
            var hourlyRequest = new HttpRequestMessage(HttpMethod.Get, usUrl);
            // Adding the new Quantititative Features
            hourlyRequest.Headers.Add("Feature-Flags", "forecast_temperature_qv,forecast_wind_speed_qv");

            hourlyForecasts = await GetDataAsync<GridHourlyData.Rootobject>(hourlyRequest);

            if (hourlyForecasts == null)
                return null;

            var pd = hourlyForecasts.properties.periods;

            var nwsHourlies = new NwsHourlyForecast[pd.Length];
            for (int d = 0; d < pd.Length; d++)
            {
                var icon = pd[d].icon;
                var lotties = MeteoconsResolver.ResolveNwsDaily(icon);

                nwsHourlies[d] = new NwsHourlyForecast
                {
                    Number = pd[d].number,
                    StartTime = pd[d].startTime,
                    EndTime = pd[d].endTime,
                    IsDaytime = pd[d].isDaytime,
                    Temperature = new NwsHourlyForecast.TemperatureHourly { Value = Math.Round(pd[d].temperature.value * 1.8 + 32, 0), UnitCode = "°F" },
                    ProbabilityOfPrecipitation = new NwsHourlyForecast.ProbabilityOfPrecipitationHourly { Value = pd[d].probabilityOfPrecipitation.value, UnitCode = "%" },
                    DewPoint = new NwsHourlyForecast.DewPointHourly { Value = pd[d].dewpoint.unitCode == "wmoUnit:degC" ? Math.Round(pd[d].dewpoint.value * 1.8 + 32, 0) : Math.Round(pd[d].dewpoint.value, 0), UnitCode = "°F" },
                    RelativeHumidity = new NwsHourlyForecast.RelativeHumidityHourly { Value = pd[d].relativeHumidity.value, UnitCode = "%" },
                    Icon = icon,
                    WindDirection = GetWindDirection(pd[d].windDirection),
                    WindSpeed = new NwsHourlyForecast.WindSpeedHourly { Value = Math.Round(pd[d].windSpeed.value / 1.609344, 0), UnitCode = "mph" },
                    WindGust = pd[d].windGust != null ? new NwsHourlyForecast.WindGustHourly { Value = Math.Round(pd[d].windGust.value / 1.609344, 0), UnitCode = "mph" } : null,
                    ShortForecast = pd[d].shortForecast,
                    SkHourlyAnimation = lotties
                };
            }
            return nwsHourlies;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"This error was received while creating nws hourly forecast objects {ex}");
        }
        return null;
    }
    // inside the outer Parallel.ForAsync body (after assigning NwsDataService.SelectedDailyHourlyForecasts[d])
    //WR
    private async Task<NwsWeatherModel> BuildTheNwsWeatherModelAsync(NwsDailyForecast[] nwsDailies, NwsHourlyForecast[] nwsHourlies, NwsHourlySupplemental[] supplementals)
    {
        var nwsData = new NwsWeatherModel();
        //MainThread.BeginInvokeOnMainThread(() => SharedSession.NwsData = new NwsWeatherModel());


        //await MainThread.InvokeOnMainThreadAsync(async() =>
        // {
        //    SharedSession.NwsData.SelectedDailyHourlyForecasts = new NwsHourlyForecast[nwsHourlies.Length];
        nwsData.HourlyForecasts = new NwsHourlyForecast[nwsHourlies.Length];
        for (int s = 0; s < nwsHourlies.Length; s++)
        {
            var supplemental = supplementals.Where(d => d.Time == nwsHourlies[s].StartTime).First() ?? new NwsHourlySupplemental();
            nwsHourlies[s].DetailedForecast = CreateHourlyDetailDescription(nwsHourlies[s]);
            Debug.WriteLine($"{s}Adding DetailedForecast to Hourly Forecast.");
            nwsHourlies[s].Supplemental = supplemental;
            Debug.WriteLine($"{s}Adding supplemental to hourly");
            //      SharedSession.NwsData.SelectedDailyHourlyForecasts[s] = nwsHourlies[s];
            nwsData.HourlyForecasts[s] = nwsHourlies[s];
        }
        ;
        //});
        //await MainThread.InvokeOnMainThreadAsync(async () =>
        //{
        //  SharedSession.NwsData.DailyForecasts = new NwsDailyForecast[nwsDailies.Length];
        nwsData.DailyForecasts = new NwsDailyForecast[nwsDailies.Length];
        for (int i = 0; i < nwsDailies.Length; i++)
        {
            var hours = nwsHourlies.Where(h => h.StartTime >= nwsDailies[i].StartTime && h.EndTime <= nwsDailies[i].EndTime).OrderBy(o => o.StartTime).ToArray();
            nwsDailies[i].HourlyWeathers = [.. hours];
            nwsData.DailyForecasts[i] = nwsDailies[i];
            Debug.WriteLine($"Nws Hourly Detail Forecast created for {nwsDailies[i].StartTime.ToString("MM/dd HH:MM")}");

            //    SharedSession.NwsData.DailyForecasts[i] = nwsDailies[i];

        }
        ;

        //await BuildSuperDuperForecastAsync(nwsData.DailyForecasts);
        // });
        // await MainThread.InvokeOnMainThreadAsync(async () =>
        // {
        //    SharedSession.NwsData.HourlySupplementals = new NwsHourlySupplemental[supplementals.Length];
        nwsData.HourlySupplementals = new NwsHourlySupplemental[supplementals.Length];
        for (int s = 0; s < supplementals.Length; s++)
        {
            Debug.WriteLine($"{s}Adding supplementals");
            //      SharedSession.NwsData.HourlySupplementals[s] = supplementals[s];
            nwsData.HourlySupplementals[s] = supplementals[s];
        }
        //});

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SharedSession.NwsData = new NwsWeatherModel(nwsData);
        });
        return nwsData;
    }


    private string CreateHourlyDetailDescription(NwsHourlyForecast nwsHourly)
    {
        var detailForecast = new StringBuilder();
        detailForecast.Append(nwsHourly.ShortForecast);
        string windGusts = string.Empty;
        if (nwsHourly.WindGust?.Value != null && nwsHourly.WindGust?.Value is double gusts)
        {
            windGusts = $" and gusts up to {gusts} mph";
        }
        string wind = $" Winds from the {nwsHourly.WindDirection} at {nwsHourly.WindSpeed.Value}{nwsHourly.WindSpeed.UnitCode}{(windGusts != string.Empty ? windGusts : ".")}";

        detailForecast.Append($" with temperature at {nwsHourly.TempAndUnit}, dew point is {nwsHourly.DewPoint.Value}{nwsHourly.DewPoint.UnitCode}, and relative humidity of {nwsHourly.RelativeHumidity.Value}%.{wind}.");
        return detailForecast.ToString();
    }



    //private string CreateDailyDetailDescription(NwsHourlyForecast[] hourlies, NwsDailyForecast daily)
    //{
    //    //Storms, precipitation, or weather condition
    //    var statementOne = new StringBuilder();
    //    //Skycover and temperature
    //    var statementTwo = new StringBuilder();
    //    //Wind, Gusts, and humidity/Dew Point
    //    var statementThree = new StringBuilder();
    //    //Get the supplemental weather conditions


    /// <summary>
    /// This is what a storm forecast should look like!!!!
    /// </summary>
    /// <returns></returns>
    //HEAD'S UP! THERE'S SOME WEATHER AFOOT!Expect snow starting at Wednesday at 3 PM and continuing through the night until 5 AM.
    //    There is a moderate risk of thunderstorms and strong winds.On Wednesday afternoon, the snow will start off light and reach a pinnacle around 10 PM.THe
    //        snow will continue at a moderate rate for 3 hours, until Thursday morning.After the snow stops, expect partly cloudy conditions from 6 AM to 1 PM, then mostly sunny
    //        until 6 PM.The high for the day will be 40



    //    if (hourlies.Any(s => s.Supplemental?.Weather != null || s.Supplemental?.ProbabilityOfThunder != null))
    //    {
    //        var hourlyWeathers = hourlies.Where(x => x.Supplemental != null && x.Supplemental.Weather.Statement != null && x.Supplemental.QuantitativePrecipitation != null).OrderBy(o => o.StartTime).Select(s => new { s.Supplemental?.Weather, s.Supplemental?.QuantitativePrecipitation, s.Supplemental?.ProbabilityOfThunder }).ToArray();
    //        var groupedWeatherEvents = hourlyWeathers.OrderBy(x => x.Weather.Time).GroupBy(i => i.Weather);
    //        if (groupedWeatherEvents.Any())
    //        {
    //            foreach (var group in groupedWeatherEvents)
    //            {
    //                var start = group.MinBy(x => x.Weather.Time);
    //                var end = group.MaxBy(x => x.Weather.Time);
    //                var thunder = group.MaxBy(x => x.ProbabilityOfThunder.Value);
    //                string thunderStmt = string.Empty;
    //                if (thunder.ProbabilityOfThunder?.Value > 10)
    //                {
    //                    thunderStmt = thunder.ProbabilityOfThunder.description;
    //                }
    //                var highestIntensity = group.MaxBy(x => x.QuantitativePrecipitation?.Value);

    //                var lowestIntensity = group.MinBy(x => x.QuantitativePrecipitation?.Value);
    //                string startTime = start.Weather.Time.ToString("h tt");
    //                string endTime = start.Weather.Time.Time == end.Weather.Time.Time ? $"until {end.Weather.Time.ToString("h tt")}." : $"and continuing through the night until {end.Weather.Time.ToString("h tt")}.";
    //                statementOne.Append($"HEAD'S UP! THERE'S SOME WEATHER AFOOT! Expect {start.Weather.Weather} starting at {startTime} {endTime}");
    //                statementOne.Append(thunderStmt);
    //                statementOne.Append($"The {start.Weather.Weather} will start off {start.Weather.Intensity} and ");
    //                string startCoverage = start.Weather.Coverage;

    //                if (start.QuantitativePrecipitation.Value < highestIntensity.QuantitativePrecipitation.Value && start.Weather.Intensity != highestIntensity.Weather.Intensity)
    //                {
    //                    int highestCount = highestIntensity.Weather.Intensity.Count();
    //                    statementOne.Append($" reach a pinnacle around {highestIntensity.Weather.Time.ToString("h tt")}. ");
    //                    if (highestCount > 1)
    //                    {
    //                        statementOne.Append($"The {start.Weather.Weather} will continue at a {highestIntensity.Weather.Intensity} rate for {highestCount} hours. ");
    //                    }
    //                }

    //                var startTimeOfDay = TimeOfDayService.CalcTimeOfDay(start.Weather.Time.Hour);
    //                var startDayOfWeek = start.Weather.Time.DayOfWeek;
    //                var endTimeOfDay = TimeOfDayService.CalcTimeOfDay(end.Weather.Time.Hour);
    //                var endDayOfWeek = end.Weather.Time.DayOfWeek;
    //                int hourlyCount = hourlies.Length;
    //                int weatherCount = hourlyWeathers.Length;
    //                if (weatherCount >= hourlyCount / 2 && daily.IsDaytime == true)
    //                {

    //                    statementOne.Append($" stick around for most of your {startDayOfWeek} {startTimeOfDay} and can continue into the {endTimeOfDay}");
    //                }
    //                else if (weatherCount >= hourlyCount / 2 && daily.IsDaytime == false)
    //                {
    //                    statementOne.Append($" stick around for most of your {startDayOfWeek} {startTimeOfDay} and continue until {endDayOfWeek} {endTimeOfDay}.");
    //                }
    //            }
    //        }

    //    }
    //    else
    //    {
    //        if (String.IsNullOrEmpty(statementOne.ToString()))
    //        {
    //            var conditions = hourlies.OrderBy(o => o.StartTime).ToArray();
    //            string dayPart = daily.IsDaytime ? "during the day" : "during the night";
    //            var grouped = conditions.GroupBy(x => x.ShortForecast);

    //            var noStorm = new StringBuilder();
    //            for (int i = 0; i < conditions.Length; i++)
    //            {
    //                if (i == 0)
    //                {
    //                    noStorm.Append($"Expect {conditions[i].ShortForecast} conditions at {conditions[i].StartTime.ToString("h tt")}");
    //                }
    //                if (i >= 1)
    //                {
    //                    if (conditions[i].ShortForecast == conditions[i - 1].ShortForecast)
    //                    {
    //                        noStorm.Replace(" at ", " from ");
    //                        noStorm.Append($"to {conditions[i].EndTime.ToString("h tt")}");
    //                    }
    //                    else
    //                    {
    //                        noStorm.Append($", {conditions[i].ShortForecast} at {conditions[i].StartTime.ToString("h tt")}");
    //                    }

    //                }
    //                if (i == conditions.Length - 2)
    //                {
    //                    noStorm.Append(" and ");
    //                }
    //                if (i == conditions.Length - 1)
    //                {
    //                    noStorm.Append(". ");
    //                }

    //            }
    //            statementOne.Append($"{noStorm}");
    //        }
    //    }
    //    string dailyForecast = daily.DetailForecast;

    //    double outDoors = 0.0;
    //    string comfortStatement = string.Empty;
    //    string windGusts = string.Empty;

    //    if (hourlies.Any(s => s.Supplemental?.WetBulbGlobeTemperature != null || s.Supplemental?.ApparentTemperature != null || s.Supplemental?.HeatIndex != null))
    //    {
    //        var minWbgt = hourlies.Select(w => w.Supplemental?.WetBulbGlobeTemperature).MinBy(x => x?.Value);
    //        var maxWbgt = hourlies.Select(w => w.Supplemental?.WetBulbGlobeTemperature).MaxBy(x => x?.Value);
    //        var minApparentTemp = hourlies.Select(w => w.Supplemental?.ApparentTemperature).MinBy(x => x?.Value);
    //        var maxApparentTemp = hourlies.Select(w => w.Supplemental?.ApparentTemperature).MaxBy(x => x?.Value);
    //        var minHeatIndex = hourlies.Select(w => w.Supplemental?.HeatIndex).MinBy(x => x?.Value);
    //        var maxHeatIndex = hourlies.Select(w => w.Supplemental?.HeatIndex).MaxBy(x => x?.Value);
    //    }
    //    //comfortStatement = _forecastService.TempCategory(hourlies[i].Temperature, hourlies[i].Dewpoint, hourlies[i].RelativeHumidity, outDoors);

    //    if (hourlies.Any(x => x.Supplemental?.Skycover != null) && hourlies.Where(x => x.Supplemental?.Skycover.Value != null).Count() >= 3)
    //    { 
    //        var skyCovers = hourlies.Where(x => x.Supplemental.Skycover != null).OrderBy(t => t.StartTime).ToArray();
    //        if (daily.IsDaytime)
    //        {
    //            var skyGroup = skyCovers.OrderBy(x => x.Supplemental.Skycover.Time).GroupBy(s => RoundToNearest10(s.Supplemental.Skycover.Value));

    //            statementTwo.Append($"{skyCovers[0].Supplemental.Skycover.Time.DayOfWeek} {TimeOfDayService.CalcTimeOfDay(skyCovers[0].Supplemental.Skycover.Time.Hour)}, starts off {skyCovers[0].Supplemental.Skycover.Description}");
    //            var skyGroup = skyCovers.OrderBy(x => x.Supplemental.Skycover.Time).GroupBy(s => RoundToNearest10(s.Supplemental.Skycover.Value));
    //        }



    //        if (skyCoverValue <= 10) statementTwo.Append($" A clear {(daily.IsDaytime ? "sunny day" : "night")}");
    //            if (skyCoverValue <= 30) statementTwo.Append($" A mostly {(daily.IsDaytime ? "sunny day" : "clear night")}");
    //            if (skyCoverValue <= 70) statementTwo.Append($" A partly cloudy {(daily.IsDaytime ? "day" : "night")}");
    //            if (skyCoverValue <= 90) statementTwo.Append($" A mostly cloudy {(daily.IsDaytime ? "day" : "night")}");
    //            if (skyCoverValue > 90) statementTwo.Append($" A cloudy {(daily.IsDaytime ? "day" : "night")}");
    //        }
    //    }

    //    string temp = $", with a temperature at {daily.TempAndUnit}.";
    //    return statementOne.ToString();
    //}

    private int RoundToNearest10(int number)
    {
        // Math.Round rounds to nearest integer; divide by 10 first, then multiply back
        return (int)(Math.Round(number / 10.0, MidpointRounding.AwayFromZero) * 10);
    }
    //        statementTwo += $"{temp}, and feeling {comfortStatement}.";
    //        statementTwo += _forecastService.BuildTemperatureStatement(hourlies);



    //        string dp = string.Empty;
    //        var d = hourlies[i].Dewpoint;
    //        if (d < 30) dp = "less than 30";
    //        if (30 <= d && d < 34) dp = "low 30's,";
    //        if (34 <= d && d <= 36) dp = "mid 30's,";
    //        if (37 <= d && d < 40) dp = "high 30's";
    //        if (40 <= d && d < 44) dp = "low 40's";
    //        if (44 <= d && d <= 46) dp = "mid 40's,";
    //        if (47 <= d && d < 50) dp = "high 40's,";
    //        if (50 <= d && d < 54) dp = "low 50's";
    //        if (54 <= d && d <= 56) dp = "mid 50's,";
    //        if (57 <= d && d < 60) dp = "high 50's";
    //        if (60 <= d && d < 64) dp = "low 60's,";
    //        if (64 <= d && d <= 66) dp = "mid 60's,";
    //        if (67 <= d && d < 70) dp = "high 60's";
    //        if (70 <= d && d < 75) dp = "low 70's";
    //        if (d > 75) dp = d.ToString();

    //        var feelsLike = string.Empty;
    //        if (hourlies[i].Temperature > 70)
    //        {
    //            var h = hourlies[i].Temperature;
    //            switch (hourlies[i].Dewpoint)
    //            {
    //                case <= 50:
    //                    feelsLike = "pleasant";
    //                    break;
    //                case > 50 and <= 60:
    //                    feelsLike = "comfortable.";
    //                    break;
    //                case (> 60 and <= 70) when h >= 70 && h < 75:
    //                    feelsLike = "humid";
    //                    break;
    //                case (> 60 and <= 70) when h > 75:
    //                    feelsLike = "mostly conformtable.";
    //                    break;
    //                case > 70 and <= 75:
    //                    feelsLike = "humid.";
    //                    break;
    //                case > 75:
    //                    //Dangerous when paired with high temperatures. Heat-illness risk for outdoor activity, athletes, elderly, and infants
    //                    feelsLike = "tropical.";
    //                    break;
    //            }

    //            if (hourlyExtra?.WindGust?.Value != null)
    //            {
    //                windGusts = $" and gusts up to {Math.Round(hourlyExtra.WindGust.Value / 1.609344, 0)} mph. ";
    //            }
    //            statementThree = $" Winds from the {GetWindDirection(hourlies[i].WindDirection).ToLowerInvariant()} at {hourlies[i].WindSpeed}{(windGusts != string.Empty ? windGusts : ".")}";
    //            statementThree += $" With a dew point in the {dp}, the outdoors will feel {feelsLike}.";

    //        }


    //    }
    //            if (statementOne == string.Empty)
    //            {
    //                statementOne = $"{hourlies[i].ShortForecast.ToLowerInvariant()}.";
    //            }
    //enriched.Add((hourlies[i].StartTime, $"{statementOne}{statementTwo}{statementThree}"));
    //        }
    //    }
    private async Task<NwsHourlySupplemental[]> GetNwsHourlySupplementalAsync(string gridUrl)
    {
        List<NwsHourlySupplemental> supp = new List<NwsHourlySupplemental>();
        var suppData = new GridDetailData.Rootobject();

        var hourlySuppRequest = new HttpRequestMessage(HttpMethod.Get, gridUrl);
        suppData = await GetDataAsync<GridDetailData.Rootobject>(hourlySuppRequest);

        if (suppData == null) return null;

        await GetNwsGridCoordinates(suppData);
        if (SharedSession.Coordinates != null)
        {
            Debug.WriteLine($"The mapsui coordinates where unable to be captured");
        }
        //Updated The process for retreiving the GridData currently on importing the weather and using it in WeatherReporter.cs
        // 1. Weather
        var wx = suppData.properties.weather;
        if (wx.values?.Length > 0 && wx.values[0]?.value[0]?.weather != null)
        {

            for (int i = 0; i < wx.values?.Length; i++)
            {
                NwsHourlySupplemental hs = new NwsHourlySupplemental();
                hs.Time = ParseNwsInterval(wx.values[i].validTime).DateTime;
                string finishedWeather = string.Empty;
                List<string> hourlyWeatherDescription = new();
                for (int x = 0; x < wx.values[i].value?.Length; x++)
                {
                    if (wx.values[i].value[x].weather != null)
                    {
                        hourlyWeatherDescription.Add(CoverageStmt(wx.values[i].value[x].coverage, wx.values[i].value[x].intensity, wx.values[i].value[x].weather));
                    }
                }
                var stmt = hourlyWeatherDescription.ToArray();
                finishedWeather = $"At " + ParseNwsInterval(wx.values[i].validTime).ToString("h tt") + ", ";
                for (int s = 0; s < stmt.Length; s++)
                {
                    if (s == stmt.Length - 1)
                    {
                        finishedWeather += stmt[s] + ". ";
                    }

                    else if (stmt.Length - 2 == s)
                    {
                        finishedWeather += stmt[s] + " and ";
                    }
                    else
                    {
                        finishedWeather += stmt[s] + ", ";
                    }
                }
                supp.Add(new NwsHourlySupplemental
                {
                    Time = ParseNwsInterval(wx.values[i].validTime).DateTime,
                    Weather = new WeatherStatementsValue
                    {
                        Time = ParseNwsInterval(wx.values[i].validTime).DateTime,
                        Statement = finishedWeather
                    }
                });
            }
        }
        else
        {
            supp.Add(new NwsHourlySupplemental
            {
                Time = ParseNwsInterval(suppData.properties.validTimes).DateTime,
                Weather = new WeatherStatementsValue
                {
                    Time = ParseNwsInterval(suppData.properties.validTimes).DateTime,
                    Statement = "There are no storms to report."
                }

            });
        }
        //Finished processing all the daily weathers
        //}//Close if there's data to process
        //}
        // 2. Apparent Temperature
        var apparentTemp = suppData.properties.apparentTemperature;
        if (apparentTemp.values?.Length > 0 && apparentTemp.values[0]?.value != null)
        {
            for (int a = 0; a < apparentTemp.values?.Length; a++)
            {
                if (apparentTemp.values[a].value is double at)
                {
                    supp.Add(new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(apparentTemp.values[a].validTime).DateTime,
                        ApparentTemperature = new ApparentTemperatureValue { Value = at, UnitCode = "°C" }
                    });
                }
            }
        }

        // 3. SkyCover
        var skyCover = suppData.properties.skyCover;
        if (skyCover.values?.Length > 0 && skyCover.values[0]?.value != null)
        {
            for (int a = 0; a < skyCover.values?.Length; a++)
            {
                if (skyCover.values[a].value is double sc)
                {
                    supp.Add(new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(skyCover.values[a].validTime).DateTime,
                        Skycover = new SkyCoverValue { Value = sc, UnitCode = skyCover.uom }
                    });
                }
            }
        }

        // 3. Mixing Height
        var mixingHeight = suppData.properties.mixingHeight;
        if (mixingHeight.values?.Length > 0 && mixingHeight.values[0]?.value != null)
        {
            for (int a = 0; a < mixingHeight.values?.Length; a++)
            {
                if (mixingHeight.values[a].value is double mh)
                {
                    supp.Add(new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(mixingHeight.values[a].validTime).DateTime,
                        MixingHeight = new MixingHeightValue { Value = mh, UnitCode = "meters" }
                    });
                }
            }
        }

        // 4. Gusts
        var gusts = suppData.properties.windGust;
        if (gusts.values?.Length > 0 && gusts.values[0]?.value != null)
        {
            for (int a = 0; a < gusts.values?.Length; a++)
            {
                if (gusts.values[a].value is double gust)
                {
                    supp.Add(new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(gusts.values[a].validTime).DateTime,
                        WindGust = new WindGustValue { Value = gust, UnitCode = gusts.uom }
                    });
                }
            }
        }

        // 5. Quantitative Precipitation
        var qp = suppData.properties.quantitativePrecipitation;
        if (qp.values?.Length > 0 && qp.values[0]?.value != null)
        {
            for (int a = 0; a < qp.values?.Length; a++)
            {
                if (qp.values[a].value is double quanPrec)
                {
                    supp.Add(new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(qp.values[a].validTime).DateTime,
                        QuantitativePrecipitation = new QuantitativePrecipitationValue { Value = quanPrec / 25.4, UnitCode = "inches" }
                    });
                }
            }
        }

        // 6. ForecastHeatRisk (This Feels like temperature with a humidity factor
        var heat = suppData.properties.heatRisk;

        if (heat.values?.Length > 0 && heat.values[0]?.value != null)
        {
            for (int h = 0; h < heat.values?.Length; h++)
            {
                if (heat.values[h].value is int heatrisknbr)
                {
                    var heatrisk = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(heat.values[h].validTime).DateTime,
                        HeatRisk = new HeatRiskValue { Value = heatrisknbr }
                    };
                    supp.Add(heatrisk);
                }
            }
        }
        else
        {
            var noHeatRisk = new NwsHourlySupplemental
            {
                Time = DateTime.Now,
                HeatRisk = new HeatRiskValue { Value = 0 }
            };
            supp.Add(noHeatRisk);
        }

        // 7. ApparentTemperature and humidity
        var heatIndex = suppData.properties.heatIndex;
        try
        {
            if (heatIndex.values?.Length > 0 && heatIndex.values[0]?.value != default)
            {
                for (int h = 0; h < heatIndex.values?.Length; h++)
                {
                    if (heatIndex.values[h].value is double heatindexdb)
                    {
                        var heatrisk = new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(heatIndex.values[h].validTime).DateTime,
                            HeatIndex = new HeatIndexValue { Value = heatindexdb, UnitCode = heatIndex.uom }
                        };
                        supp.Add(heatrisk);
                    }
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            Debug.WriteLine($"This error was received while creating nws hourly supplemental heat index objects {ex}");
        }

        // 8. WindChill
        var windchill = suppData.properties.windChill;
        if (windchill.values?.Length > 0 && windchill.values[0]?.value != null)
        {
            for (int h = 0; h < windchill.values?.Length; h++)
            {
                if (windchill.values[h].value is double wc)
                {
                    var windChill = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(windchill.values[h].validTime).DateTime,
                        WindChill = new WindchillValue { Value = wc, UnitCode = windchill.uom }
                    };
                    supp.Add(windChill);
                }
            }
        }

        // 9. Daily Minimum Temperature
        var mintemp = suppData.properties.minTemperature;

        if (mintemp.values?.Length > 0 && mintemp.values[0]?.value != null)
        {
            for (int h = 0; h < mintemp.values?.Length; h++)
            {
                if (mintemp.values[h].value is double mt)
                {
                    var minTemp = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(mintemp.values[h].validTime).DateTime,
                        MinTemperature = new MinTemperatureValue { Value = mt, UnitCode = mintemp.uom }
                    };
                    supp.Add(minTemp);
                }
            }
        }

        // 10. Daily Maximum Temperature
        var maxtemp = suppData.properties.maxTemperature;

        if (maxtemp.values?.Length > 0 && maxtemp.values[0]?.value != null)
        {
            for (int h = 0; h < maxtemp.values?.Length; h++)
            {
                if (maxtemp.values[h].value is double mt)
                {
                    var maxTemp = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(maxtemp.values[h].validTime).DateTime,
                        MaxTemperature = new MaxTemperatureValue { Value = mt, UnitCode = maxtemp.uom }
                    };
                    supp.Add(maxTemp);
                }
            }
        }

        // 11. Relative RelativeHumidity
        var rh = suppData.properties.relativeHumidity;

        if (rh.values?.Length > 0 && rh.values[0]?.value != null)
        {
            for (int h = 0; h < rh.values?.Length; h++)
            {
                if (rh.values[h].value is double relHum)
                {
                    var rhValue = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(rh.values[h].validTime).DateTime,
                        RelativeHumidity = new RelativeHumidityValue { Value = relHum, UnitCode = rh.uom }
                    };
                    supp.Add(rhValue);
                }
            }
        }

        // 12: Dew Point
        var dp = suppData.properties.dewpoint;

        if (dp.values?.Length > 0 && dp.values[0]?.value != null)
        {
            for (int h = 0; h < dp.values?.Length; h++)
            {
                if (dp.values[h].value is double dew)
                {
                    var dewPoint = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(dp.values[h].validTime).DateTime,
                        DewPoint = new DewPointValue { Value = dew, UnitCode = dp.uom }
                    };
                    supp.Add(dewPoint);
                }
            }
        }
        // 13. Temperature
        var temp = suppData.properties.temperature;

        if (temp.values?.Length > 0 && temp.values[0]?.value != null)
        {
            for (int h = 0; h < temp.values?.Length; h++)
            {
                if (temp.values[h].value is double tmp)
                {
                    var temperature = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(temp.values[h].validTime).DateTime,
                        Temperature = new TemperatureValue { Value = tmp, UnitCode = temp.uom }
                    };
                    supp.Add(temperature);
                }
            }
        }

        // 14. Thunder
        var thunder = suppData.properties.probabilityOfThunder;

        if (thunder.values?.Length > 0 && thunder.values[0]?.value != null)
        {
            for (int h = 0; h < thunder.values?.Length; h++)
            {
                if (thunder.values[h].value is double ts)
                {
                    var tstorm = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(thunder.values[h].validTime).DateTime,
                        ProbabilityOfThunder = new ProbabilityOfThunderValue
                        {
                            Time = ParseNwsInterval(thunder.values[h].validTime).DateTime,
                            Value = ts,
                            UnitCode = "%",
                            description = ts switch
                            {
                                > 75 => " there is an extremely high risk wide spread and intense thunderstorms with strong winds.",
                                > 50 and <= 75 => " there is a high risk of thunderstorms and strong winds.",
                                > 30 and <= 50 => " there is a moderate risk of thunderstorms and strong winds.",
                                > 10 and <= 30 => " there is a slight risk of scattered thunderstorms possible.",
                                > 1 and <= 10 => " there is a low chance of thunderstorms.",
                                _ => " No thunderstorms expected."
                            }
                        }

                    };
                    supp.Add(tstorm);
                }
            }
        }

        // 15. Low DistanceVisible Occurence Risk Index
        var lvori = suppData.properties.lowVisibilityOccurrenceRiskIndex;

        if (lvori.values?.Length > 0 && lvori.values[0]?.value != null)
        {
            for (int h = 0; h < lvori.values?.Length; h++)
            {
                if (lvori.values[h].value is double vis)
                {
                    var lvoriValue = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(lvori.values[h].validTime).DateTime,
                        LowVisibilityOccurrenceRiskIndex = new LowVisibilityOccurrenceRiskIndexValue
                        {
                            Value = vis,
                            description = vis switch
                            {
                                1 or 2 or 3 => "Low risk of very low Visibility.",
                                4 or 5 or 6 => "Moderate risk of low Visibility.",
                                7 or 8 or 9 => "High risk of low Visibility.",
                                10 => "Exreme risk - very low Visibility expected.",
                                _ => "No low Visibility risk issued."
                            }
                        }
                    };
                    supp.Add(lvoriValue);
                }
            }
        }

        // 16. Ice Accumulation
        var ice = suppData.properties.iceAccumulation;

        if (ice.values?.Length > 0 && ice.values[0]?.value != null)
        {
            for (int h = 0; h < ice.values?.Length; h++)
            {
                if (ice.values[h].value is double iceAccum)
                {
                    var iceAccumulate = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(ice.values[h].validTime).DateTime,
                        IceAccumulation = new IceAccumulationValue { Value = iceAccum, UnitCode = ice.uom }
                    };
                    supp.Add(iceAccumulate);
                }
            }
        }

        // 17. Snowfall Amount
        var snow = suppData.properties.snowfallAmount;

        if (snow.values?.Length > 0 && snow.values[0]?.value != null)
        {
            for (int h = 0; h < snow.values?.Length; h++)
            {
                if (snow.values[h].value is double snowy)
                {
                    var snowFall = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(snow.values[h].validTime).DateTime,
                        SnowfallAmount = new SnowFallAmountValue { Value = snowy, UnitCode = snow.uom }
                    };
                    supp.Add(snowFall);
                }
            }
        }

        // 18. Probability Of Precipitation
        var precip = suppData.properties.probabilityOfPrecipitation;

        if (precip.values?.Length > 0 && precip.values[0]?.value != null)
        {
            for (int h = 0; h < precip.values?.Length; h++)
            {
                if (precip.values[h].value is double rain)
                {
                    var probPrecip = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(precip.values[h].validTime).DateTime,
                        ProbabilityOfPrecipitation = new ProbabilityOfPrecipitationValue { Value = rain, UnitCode = precip.uom }
                    };
                    supp.Add(probPrecip);
                }
            }
        }
        var wbgt = suppData.properties.wetBulbGlobeTemperature;

        if (wbgt.values?.Length > 0 && wbgt.values[0]?.value != null)
        {
            for (int h = 0; h < wbgt.values?.Length; h++)
            {
                if (wbgt.values[h].value is double wet)
                {
                    var wetBulb = new NwsHourlySupplemental
                    {
                        Time = ParseNwsInterval(wbgt.values[h].validTime).DateTime,
                        WetBulbGlobeTemperature = new WetBulbGlobeTemperatureValue { Value = wet, UnitCode = wbgt.uom }
                    };
                    supp.Add(wetBulb);
                }
            }
        }

        NwsHourlySupplemental[] supplementals = supp.OrderBy(o => o.Time).GroupBy(g => g.Time).Select(w => new NwsHourlySupplemental
        {
            Time = w.Key,
            ApparentTemperature = w.Max(y => y.ApparentTemperature) as ApparentTemperatureValue,
            IceAccumulation = w.Max(y => y.IceAccumulation) as IceAccumulationValue,
            SnowfallAmount = w.Max(y => y.SnowfallAmount) as SnowFallAmountValue,
            DewPoint = w.Max(y => y.DewPoint) as DewPointValue,
            HeatIndex = w.Max(y => y.HeatIndex) as HeatIndexValue,
            HeatRisk = w.Max(y => y.HeatRisk) as HeatRiskValue,
            LowVisibilityOccurrenceRiskIndex = w.Max(y => y.LowVisibilityOccurrenceRiskIndex) as LowVisibilityOccurrenceRiskIndexValue,
            MaxTemperature = w.Max(y => y.MaxTemperature) as MaxTemperatureValue,
            MinTemperature = w.Max(y => y.MinTemperature) as MinTemperatureValue,
            ProbabilityOfPrecipitation = w.Max(y => y.ProbabilityOfPrecipitation) as ProbabilityOfPrecipitationValue,
            ProbabilityOfThunder = w.Max(y => y.ProbabilityOfThunder) as ProbabilityOfThunderValue,
            QuantitativePrecipitation = w.Max(y => y.QuantitativePrecipitation) as QuantitativePrecipitationValue,
            RelativeHumidity = w.Max(y => y.RelativeHumidity) as RelativeHumidityValue,
            Skycover = w.Max(y => y.Skycover) as SkyCoverValue,
            Temperature = w.Max(y => y.Temperature) as TemperatureValue,
            Weather = w.Max(y => y.Weather) as WeatherStatementsValue,
            WetBulbGlobeTemperature = w.Max(y => y.WetBulbGlobeTemperature) as WetBulbGlobeTemperatureValue,
            WindChill = w.Max(y => y.WindChill) as WindchillValue,
            WindGust = w.Max(y => y.WindGust) as WindGustValue
        }).ToArray();

        return supplementals;
    }

    private async Task GetNwsGridCoordinates(GridDetailData.Rootobject suppData)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Part 1. Fetching the Polygon geographic coordinates
            try
            {
                // Geometry.coordinates can be Polygon (double[][][]), MultiPolygon (double[][][][]) or Point (double[])
                if (suppData is NwsSupplementalData.Rootobject)
                {
                    var coordsElement = suppData.geometry.coordinates;

                    // Try Polygon first
                    try
                    {
                        var rings = coordsElement; // your double[][][] from JSON

                        if (rings != null && rings.Length > 0)
                        {
                            var ring = rings[0]; // first ring of polygon


                            var poly = new Microsoft.Maui.Controls.Maps.Polygon
                            {
                                StrokeColor = Colors.Red,
                                StrokeWidth = 2,
                                FillColor = Color.FromRgba(255, 0, 0, 64)
                            };
                            for (int i = 0; i < ring.Length; i++)
                            {
                                // Each ring[i] should be: [lon, lat]
                                if (ring[i].Length >= 2)
                                {
                                    poly.Geopath.Add(new Microsoft.Maui.Devices.Sensors.Location(ring[i][1], ring[i][0]));

                                    // Convert lon/lat → Mapsui Point (Mercator)
                                    //var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                                    // LinearRing requires Coordinates, not Points
                                    //points.Add(new Coordinate(x, y));

                                }
                            }

                            // Build LinearRing
                            SharedSession.Coordinates = poly;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error parsing geometry coordinates: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing geometry coordinates: {ex}");
            }
        });
    }

    private enum Coverage
    {
        areas,
        brief,
        chance,
        definite,
        few,
        frequent,
        intermittent,
        isolated,
        likely,
        numerous,
        occasional,
        patchy,
        periods,
        scattered,
        slight_chance,
        widespread
    }
    static string CoverageStmt(string coverage, string intensity, string weather)
    {
        string weatherDescription = string.Empty;
        weatherDescription += coverage switch
        {
            "areas" => "areas of ",
            "brief" => "brief ",
            "chance" => "chance of ",
            "definite" => "definite ",
            "few" => "few ",
            "frequent" => "frequent ",
            "intermittent" => "intermittent ",
            "isolated" => "isolated ",
            "likely" => "likely chance of ",
            "numerous" => "numerous ",
            "occasional" => "occasional ",
            "patchy" => "patchy ",
            "periods" => "periods of ",
            "scattered" => "scattered ",
            "slight_chance" => "slight chance of ",
            "widespread" => "widespread ",
            null => "",
            "" => "",
            _ => "",
        };

        weatherDescription += " " + intensity ?? "";
        weatherDescription += " " + weather;
        return weatherDescription;
    }

    public DateTimeOffset ParseNwsInterval(string interval)
    {
        // Splits "2024-05-06T12:00:00+00:00/PT1H" and takes the first part
        var parts = interval.Split('/');
        return DateTimeOffset.Parse(parts[0]);
    }
    public static string GetWindDirection(string windDirection)
    {
        switch (windDirection.ToLowerInvariant())
        {
            case "n":
                return "north";

            case "nne":
                return "north-northeast";

            case "ne":
                return "Northeast";

            case "ene":
                return "east-northeast";

            case "e":
                return "east";

            case "ese":
                return "east-southeast";

            case "s":
                return "south";

            case "ssw":
                return "south-southwest";

            case "sw":
                return "southwest";

            case "wsw":
                return "west-southwest";

            case "w":
                return "west";

            case "wnw":
                return "west-northwest";

            case "nw":
                return "northwest";

            case "nnw":
                return "north-northwest";

            case "sse":
                return "south-southeast";

            default:
                return "unknown";
        }
    }
}

//private async Task<ImageSource> GetHourlyImageSource(string icon)
//{
//    //Modify Uri to local resource
//    var imageName = string.Concat(icon.Split("/").Last().Split("?")[0], ".png");
//    ImageSource imageSource = ImageSource.FromFile(imageName);
//    return imageSource;
//}
//}