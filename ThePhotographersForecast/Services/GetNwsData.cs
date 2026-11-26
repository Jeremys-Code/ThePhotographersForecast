using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Models.ObserverCalc;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Services.ML;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;


namespace ThePhotographersForecast.Services;

public partial class GetNwsData : ObservableObject, IGetNwsData
{
    private readonly HttpClient _client;
    private ISession _session;
    private ISkyMetricsService _skyService;
    private IForecastService _forecastService;
    private PhotographersForecastOnnxService _onnxService;
    [ObservableProperty]
    public partial string DailyUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string HourlyUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string DetailUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial NwsWeatherModel NwsData { get; set; } = new NwsWeatherModel();

    public GetNwsData(HttpClient client, ISession session, IForecastService forecastService, ISkyColorEngine skyEngine, PhotographersForecastOnnxService onnxService, ISkyMetricsService skyService)
    {
        _client = client;
        _session = session;
        _forecastService = forecastService;
        _onnxService = onnxService;
        _skyService = skyService;
    }
    private async Task<T?> GetDataAsync<T>(HttpRequestMessage request, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            // ensure response is disposed so connection returns to pool promptly
            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"ERROR: {_client.BaseAddress} FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                return default;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new NullNumberConverterFactory());

            // pass the cancellation token to DeserializeAsync so the read can be cancelled cleanly
            var result = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
#if DEBUG
            string jsonFileContent = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true // optional: for readable output
            });

            File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, $"nws_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}_{name}.json"), jsonFileContent);
#endif
            return result;
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Request cancelled.");
            return default;
        }
        catch (Exception ex) //handle transient IO/HTTP/Socket/ObjectDisposed etc.
        {
            Debug.WriteLine($"Transient error: {ex.GetType().Name} - {ex.Message}");
            return default;
        }
    }

    public async Task<NwsWeatherModel> GetWeatherAsync(double latitude, double longitude)
    {

        NwsData = new NwsWeatherModel();
        var data = new CoordinatesToGridData.Rootobject();
        try
        {
            string apiString = $"https://api.weather.gov/points/{Math.Round(latitude, 5)},{Math.Round(longitude, 5)}";

            var request = new HttpRequestMessage(HttpMethod.Get, apiString);
            data = await GetDataAsync<CoordinatesToGridData.Rootobject>(request, "urls");
            if (data == null) return NwsData;

        }
        catch (ApplicationException ex)
        {
            Debug.WriteLine($"ERROR WHILE FETCHING NWS URLS: {ex.Message}.{ex.InnerException}");
            return NwsData;
        }
        DailyUrl = data.properties.forecast;
        HourlyUrl = data.properties.forecastHourly;
        DetailUrl = data.properties.forecastGridData;

        // GOTO SelectLocationViewModel Line: 411
        var selectedLocation = new PhotoLocation
        {
            Latitude = latitude,
            Longitude = longitude,
            City = data.properties.relativeLocation.properties.city,
            State = data.properties.relativeLocation.properties.state,

        };
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(data.properties.timeZone);
        var timezoneOffsetSeconds = (int)timezone.GetUtcOffset(DateTime.UtcNow).TotalSeconds;

        Debug.WriteLine($"Lat:{selectedLocation.Latitude}");
        Debug.WriteLine($"Lon:{selectedLocation.Longitude}");
        Debug.WriteLine($"DN:{selectedLocation.DisplayName}");
        // Fetch independent endpoints concurrently to eliminate round-trip latency
        Debug.WriteLine($"{_session.StartDate.ToString("F")}");
        Debug.WriteLine($"{_session.EndDate.ToString("F")}");
        Task<NwsDailyForecast[]> dailyTask = GetNwsDailyForecastAsync(DailyUrl);
        Task<NwsHourlyForecast[]> hourlyTask = GetNwsHourlyForecastAsync(HourlyUrl);
        Task<NwsHourlySupplemental[]> supplementalTask = GetNwsHourlySupplementalAsync(DetailUrl);
        await Task.WhenAll(dailyTask, hourlyTask, supplementalTask);

        if (dailyTask.IsCompletedSuccessfully && hourlyTask.IsCompletedSuccessfully && supplementalTask.IsCompletedSuccessfully)
        {
            var nwsDailies = await dailyTask;
            NwsData.DailyForecasts = nwsDailies.ToArray();
            var nwsHourlies = await hourlyTask;
            NwsData.HourlyForecasts = nwsHourlies.ToArray();
            var nwsHourlySupplementals = await supplementalTask;
            NwsData.HourlySupplementals = nwsHourlySupplementals.ToArray();

            //var nwsDailies = dailyTask.Result;
            //var nwsHourlies = hourlyTask.Result;
            //var nwsHourlySupplementals = supplementalTask.Result;
            await BuildTheNwsWeatherModelAsync();

        }
        else
        {
            Debug.WriteLine($"ERROR: One or more tasks failed. Daily Task: {dailyTask.Status}, Hourly Task: {hourlyTask.Status}, Supplemental Task: {supplementalTask.Status}");
            throw new Exception("One or more tasks failed while fetching NWS data.");
        }

        //await Parallel.ForAsync(0, nwsData.DailyForecasts.Length, async (i, cancellationToken) =>
        //{
        //    var date = nwsData.DailyForecasts[i].StartTime.Date;
        //    var hourlies = nwsData.DailyForecasts[i].HourlyWeathers;
        //    var highTemp = new NwsHourlyForecast();
        //    var lowTemp = new NwsHourlyForecast();
        //    if (hourlies.Where(s => s.IsDaytime == true).Any())
        //    {
        //        highTemp = nwsData.DailyForecasts[i].HourlyWeathers.Where(s => s.IsDaytime).MaxBy(t => t.Temperature.Value);
        //        int highIndex = nwsData.DailyForecasts[i].HourlyWeathers.IndexOf(highTemp);
        //        nwsData.DailyForecasts[i].HourlyWeathers[highIndex].IsHigh = true;
        //        nwsData.DailyForecasts[i].HighTemperature = $"{Math.Round(highTemp.Temperature.Value, 0)}{highTemp.Temperature.UnitCode} {Environment.NewLine}@ {highTemp.StartTime.ToString("h tt")}.";
        //    }
        //    else
        //    {
        //        nwsData.DailyForecasts[i].HighTemperature = $"No Temperature{Environment.NewLine}to report.";
        //    }
        //    int lowIndex = nwsData.DailyForecasts[i].HourlyWeathers.IndexOf(lowTemp);
        //    nwsData.DailyForecasts[i].HourlyWeathers[lowIndex].IsLow = true;
        //    nwsData.DailyForecasts[i].LowTemperature = $"{Math.Round(lowTemp.Temperature.Value, 0)}{lowTemp.Temperature.UnitCode} {Environment.NewLine}@ {lowTemp.StartTime.ToString("h tt")}";

        //});
        NwsData.HourlyTemperatureChangeRate = CreateAverageHourlyTemperatureChangeRate();
        NwsData.SelectedLocation = new PhotoLocation(selectedLocation);
        NwsData.Timezone = timezone;
        NwsData.TimezoneOffsetSeconds = timezoneOffsetSeconds;

        return NwsData;
    }

    //private async Task BuildTheTimeOfDayForecastModelAsync()
    //{
    //    try
    //    {
    //        // Build time-of-day forecasts from the hourly data

    //        if (timeOfDayForecasts != null && timeOfDayForecasts.Length > 0)
    //        {
    //            NwsData.TimeOfDayForecasts = timeOfDayForecasts;
    //        }
    //        else
    //        {
    //            NwsData.TimeOfDayForecasts = Array.Empty<NwsTimeOfDayForecast>();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Error building time-of-day forecasts: {ex.Message}");
    //        NwsData.TimeOfDayForecasts = Array.Empty<NwsTimeOfDayForecast>();
    //    }
    //}

    private async Task<NwsDailyForecast[]> GetNwsDailyForecastAsync(string url)
    {
        var dailyForecast = new GridDailyData.Rootobject();
        try
        {
            var usUrl = String.Concat(url, "?units=us");
            var dailyRequest = new HttpRequestMessage(HttpMethod.Get, usUrl);
            // Adding the new Quantititative Features
            dailyRequest.Headers.Add("Feature-Flags", "forecast_temperature_qv,forecast_wind_speed_qv");
            dailyForecast = await GetDataAsync<GridDailyData.Rootobject>(dailyRequest, "daily");
            if (dailyForecast == null) return default;

            _session.SelectedLocation.Elevation = new ElevationValue { Elevation = dailyForecast.properties.elevation.value * 3.2808399, UnitCode = "ft" };


            var periods = dailyForecast.properties.periods;
            var nwsDailies = new NwsDailyForecast[periods.Length];

            for (int i = 0; i < periods.Length; i++)
            {
                var icon = periods[i].icon;
                var lotties = MeteoconsResolver.ResolveNwsDaily(icon);
                var windGusts = periods[i].windGust != null ? $" and gusts up to {periods[i].windGust.value / 1.609344} mph." : ".";
                // var imgSrc = await GetHourlyImageSource(icon.Replace("medium", "large"));

                nwsDailies[i] = new NwsDailyForecast
                {

                    Number = periods[i].number,
                    Name = periods[i].name,
                    StartTime = periods[i].startTime,
                    EndTime = periods[i].endTime,
                    IsDaytime = periods[i].isDaytime,
                    Temperature = periods[i].temperature.value,
                    DailyIcon = icon,
                    ChanceOfPrecipitation = periods[i].probabilityOfPrecipitation.value,
                    PrecipitationUnitCode = periods[i].probabilityOfPrecipitation.unitCode,
                    ShortForecast = periods[i].shortForecast,
                    DetailForecast = periods[i].detailedForecast,
                    WindSpeed = new DailyWindSpeedValue { Value = periods[i].windSpeed.value, Max = periods[i].windSpeed.maxValue, Min = periods[i].windSpeed.minValue },
                    WindDirection = GetWindDirection(periods[i].windDirection),
                    WindGusts = periods[i].windGust != null ? new WindGustValue { Value = periods[i].windGust.value } : null,
                    WindSpeedStatement = $"Winds from the {GetWindDirection(periods[i].windDirection).ToLowerInvariant()} at {Math.Round(periods[i].windSpeed.minValue / 1.609344, 0)} to {Math.Round(periods[i].windSpeed.maxValue / 1.609344, 0)} mph{windGusts}",
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

            hourlyForecasts = await GetDataAsync<GridHourlyData.Rootobject>(hourlyRequest, "hourly");

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
                    Temperature = new NwsHourlyForecast.TemperatureHourly { Value = pd[d].temperature.value },
                    ProbabilityOfPrecipitation = new NwsHourlyForecast.ProbabilityOfPrecipitationHourly { Value = pd[d].probabilityOfPrecipitation.value, UnitCode = "%" },
                    DewPoint = new NwsHourlyForecast.DewPointHourly { Value = pd[d].dewpoint.value },
                    RelativeHumidity = new NwsHourlyForecast.RelativeHumidityHourly { Value = pd[d].relativeHumidity.value },
                    Icon = icon,
                    WindDirection = GetWindDirection(pd[d].windDirection),
                    WindSpeed = new NwsHourlyForecast.WindSpeedHourly { Value = pd[d].windSpeed.value },
                    WindGust = pd[d].windGust != null ? new NwsHourlyForecast.WindGustHourly { Value = pd[d].windGust.value } : null,
                    ShortForecast = pd[d].shortForecast,
                    SkHourlyAnimation = lotties,
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

    private async Task<NwsWeatherModel> BuildTheNwsWeatherModelAsync()
    {

        for (int s = 0; s < NwsData.HourlyForecasts.Length; s++)
        {
            var supplemental = NwsData.HourlySupplementals.Where(d => d.Time == NwsData.HourlyForecasts[s].StartTime).FirstOrDefault() ?? new NwsHourlySupplemental();
            Debug.WriteLine($"{s}Adding DetailedForecast to Hourly Forecast.");
            NwsData.HourlyForecasts[s].Supplemental = supplemental;
            Debug.WriteLine($"{s}Adding supplemental to hourly");
            NwsData.HourlyForecasts[s].DetailedForecast = CreateHourlyDetailDescription(NwsData.HourlyForecasts[s]);
        }


        for (int i = 0; i < NwsData.DailyForecasts.Length; i++)
        {
            var hours = NwsData.HourlyForecasts.Where(h => h.StartTime >= NwsData.DailyForecasts[i].StartTime && h.EndTime <= NwsData.DailyForecasts[i].EndTime).OrderBy(o => o.StartTime).ToArray();
            NwsData.DailyForecasts[i].HourlyWeathers = hours.ToArray();

            Debug.WriteLine($"Nws Hourly Detail Forecast created for {NwsData.DailyForecasts[i].StartTime.ToString("MM/dd HH:MM")}");
        }

        return NwsData;
    }


    private string CreateHourlyDetailDescription(NwsHourlyForecast nwsHourly)
    {
        var detailForecast = new StringBuilder();
        detailForecast.Append($"Hello, this is the forecast for {nwsHourly.StartTime:h tt}. ");
        detailForecast.Append($"{nwsHourly.ShortForecast} and {NumberGroups.TempCategory(nwsHourly.Temperature.Value, nwsHourly.DewPoint.Value, nwsHourly.RelativeHumidity.Value)} ");
        detailForecast.Append($"There is a {NumberGroups.WindsToString(nwsHourly.WindSpeed.Value)},");
        detailForecast.Append($" about {nwsHourly.WindSpeed.Value}mph, from the {nwsHourly.WindDirection}. ");

        if (nwsHourly.WindGust != null)
        {
            detailForecast.Append($"May the wind gusts, with speeds up to {nwsHourly.WindGust.Value}mph, work in your favor. ");
        }
        detailForecast.AppendLine($" Dew Point: {nwsHourly.DewPoint.Value}{nwsHourly.DewPoint.UnitCode} Relative Humidity: {nwsHourly.RelativeHumidity.Value}%");

        return detailForecast.ToString();
    }

    private int RoundToNearest10(int number)
    {
        return (int)(Math.Round(number / 10.0, MidpointRounding.AwayFromZero) * 10);
    }

    private async Task<IReadOnlyList<DailyForecastSummary>> CreateHourlyNwsInputAsync(GridDetailData.Rootobject suppData)
    {
        var skyDataPoints = await _skyService.GetSkyModelDataAsync() ?? Array.Empty<SkyHourData>();
        var skyByHour = new Dictionary<DateTime, SkyHourData>();
        foreach (var s in skyDataPoints)
        {
            if (DateTime.TryParse(s.Time, out var dt))
            {
                var key = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                skyByHour.TryAdd(key, s);
            }
        }

        var supplementals = await ParseSupplementalDataAsync(suppData) ?? Array.Empty<NwsHourlySupplemental>();
        List<HourlyNwsInput> hours = new List<HourlyNwsInput>(supplementals.Length);

        double lat = _session.SelectedLocation?.Latitude ?? 0;
        double lon = _session.SelectedLocation?.Longitude ?? 0;

        for (int i = 0; i < supplementals.Length; i++)
        {
            var supp = supplementals[i];
            var hourKey = new DateTime(supp.Time.Year, supp.Time.Month, supp.Time.Day, supp.Time.Hour, 0, 0);
            skyByHour.TryGetValue(hourKey, out var sky);

            float tempC = supp.Temperature != null
                ? (float)Math.Round((supp.Temperature.Value - 32) / 1.8, 8)
                : (sky?.Temperature2m?.Value ?? 0f);
            float apparentC = supp.ApparentTemperature != null
                ? (float)Math.Round((supp.ApparentTemperature.Value - 32) / 1.8, 8)
                : (sky?.ApparentTemperature?.Value ?? tempC);
            float dewpointC = supp.DewPoint != null
                ? (float)Math.Round((supp.DewPoint.Value - 32) / 1.8, 8)
                : tempC - 5f;
            float rhPct = supp.RelativeHumidity != null
                ? (float)supp.RelativeHumidity.Value
                : (sky?.RelativeHumidity2m?.Value ?? 50f);

            float windSpeedKmh = supp.WindSpeed != null
                ? (float)(supp.WindSpeed.Value * 1.609344)
                : 0f;
            float? windGustKmh = supp.WindGust != null
                ? (float)(supp.WindGust.Value * 1.609344)
                : sky?.WindGusts10m?.Value;
            float windDirDeg = supp.WindDirection != null
                ? (float)supp.WindDirection.Value
                : (sky?.WindDirection120m?.Value ?? 0f);
            float skyCoverPct = supp.Skycover != null
                ? (float)supp.Skycover.Value
                : (sky?.CloudCover?.Value ?? 0f);
            float precipProbPct = supp.ProbabilityOfPrecipitation != null
                ? (float)supp.ProbabilityOfPrecipitation.Value
                : (sky?.PrecipitationProbability?.Value ?? 0f);

            float solarElevDeg = (float)(SunCalc.GetPosition(supp.Time, lat, lon).Altitude * (180.0 / Math.PI));
            float ceilingM = supp.CeilingHeight != null
                ? (float)Math.Round(supp.CeilingHeight.Value / 3.2808, 8)
                : 10000f;

            float cloudLow = sky?.CloudCoverLow?.Value ?? skyCoverPct;
            float cloudMid = sky?.CloudCoverMid?.Value ?? skyCoverPct;
            float cloudHigh = sky?.CloudCoverHigh?.Value ?? skyCoverPct;
            float visibilityKm = sky?.Visibility != null ? sky.Visibility.Value / 1000f : 16f;
            float wind120Kmh = sky?.WindSpeed120m?.Value ?? (windSpeedKmh * 1.25f);
            float precipMm = sky?.Precipitation?.Value ?? 0f;

            float moonElevDeg = (float)(MoonCalc.GetMoonPosition(supp.Time, lat, lon).Altitude * (180.0 / Math.PI));
            float moonIllum = (float)MoonCalc.GetMoonIllumination(supp.Time).Fraction;

            var hourly = new HourlyNwsInput(
                (DateTimeOffset)supp.Time,
                tempC,
                apparentC,
                dewpointC,
                rhPct,
                windSpeedKmh,
                windGustKmh,
                windDirDeg,
                skyCoverPct,
                precipProbPct,
                solarElevDeg,
                ceilingM,
                cloudLow,
                cloudMid,
                cloudHigh,
                visibilityKm,
                wind120Kmh,
                precipMm,
                moonElevDeg,
                moonIllum);

            hours.Add(hourly);
        }

        IReadOnlyList<HourlyNwsInput> hour = hours;
        var summary = await _onnxService.PredictForecastAsync(hour);
        _session.DailyPhotoScoringSummary = summary;
        return summary;
    }

    private async Task<NwsHourlySupplemental[]> GetNwsHourlySupplementalAsync(string gridUrl)
    {
        List<NwsHourlySupplemental> supp = new List<NwsHourlySupplemental>();
        var suppData = new GridDetailData.Rootobject();

        var hourlySuppRequest = new HttpRequestMessage(HttpMethod.Get, gridUrl);
        suppData = await GetDataAsync<GridDetailData.Rootobject>(hourlySuppRequest, "supplemental");

        if (suppData == null) return null;

        await GetNwsGridCoordinates(suppData);


        if (_session.Coordinates != null)
        {
            Debug.WriteLine($"The mapsui coordinates where unable to be captured");
        }

        var supplementals = await ParseSupplementalDataAsync(suppData);

        _session.DailyPhotoScoringSummary = await CreateHourlyNwsInputAsync(suppData);

        return supplementals;

    }

    private async Task<NwsHourlySupplemental[]> ParseSupplementalDataAsync(GridDetailData.Rootobject suppData)
    {
        var lookup = new Dictionary<DateTime, NwsHourlySupplemental>();

        NwsHourlySupplemental GetOrCreate(DateTime time)
        {
            if (!lookup.TryGetValue(time, out var item))
            {
                item = new NwsHourlySupplemental { Time = time };
                lookup[time] = item;
            }
            return item;
        }

        // 1. Weather phenomena and coverage statements

        var wx = suppData.properties.weather;
        bool hasWeatherData = false;

        if (wx?.values != null && wx.values.Length > 0)
        {
            for (int i = 0; i < wx.values.Length; i++)
            {
                var valEntry = wx.values[i];
                if (valEntry?.value == null || valEntry.value.Length == 0)
                    continue;

                var validLayers = valEntry.value
                    .Where(v => v != null && !string.IsNullOrWhiteSpace(v.weather))
                    .ToArray();

                if (validLayers.Length == 0)
                    continue;

                hasWeatherData = true;
                var (startTime, duration) = ParseNwsInterval(valEntry.validTime);

                for (int dur = 0; dur < duration; dur++)
                {
                    DateTime targetTime = startTime.AddHours(dur);
                    int remainingDuration = duration - dur;

                    var statements = new List<string>();
                    var weatherStmts = new WeatherStatementsValue[validLayers.Length];

                    for (int x = 0; x < validLayers.Length; x++)
                    {
                        var layer = validLayers[x];
                        string stmt = CoverageStmt(layer.coverage, layer.weather, layer.intensity);

                        weatherStmts[x] = new WeatherStatementsValue
                        {
                            Weather = layer.weather ?? string.Empty,
                            Coverage = layer.coverage ?? string.Empty,
                            Intensity = layer.intensity ?? string.Empty,
                            Time = targetTime,
                            Duration = remainingDuration,
                            Statement = stmt
                        };

                        if (!string.IsNullOrWhiteSpace(stmt))
                        {
                            statements.Add(stmt);
                        }
                    }

                    string finishedWeather = string.Empty;
                    if (statements.Count > 0)
                    {
                        finishedWeather = statements.Count == 1
                            ? $"At {targetTime:h tt}, {statements[0]}."
                            : $"At {targetTime:h tt}, {string.Join(", ", statements.Take(statements.Count - 1))} and {statements.Last()}.";
                    }

                    var record = GetOrCreate(targetTime);
                    record.Weather = weatherStmts;
                    record.SupplementalWeatherStatement = finishedWeather;
                }
            }
        }

        if (!hasWeatherData && !string.IsNullOrEmpty(suppData.properties.validTimes))
        {
            var (startTime, _) = ParseNwsInterval(suppData.properties.validTimes);
            var record = GetOrCreate(startTime);
            record.SupplementalWeatherStatement = "There are no storms to report.";
        }

        // 2. Apparent Temperature
        ExpandMetric<double>(suppData.properties.apparentTemperature, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.ApparentTemperature = new ApparentTemperatureValue { Value = val, Duration = dur });

        // 3. Wind Speed
        ExpandMetric<double>(suppData.properties.windSpeed, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.WindSpeed = new WindSpeedValue { Value = val, Duration = dur });

        // 4. Wind Direction (int)
        ExpandMetric<int>(suppData.properties.windDirection, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.WindDirection = new WindDirectionValue { Value = val, Duration = dur });

        // 5. Wind Gust (value > 1.0)
        ExpandMetric<double>(suppData.properties.windGust, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.WindGust = new WindGustValue { Value = val, Duration = dur },
            predicate: val => val > 1.0);

        // 6. Sky Cover
        ExpandMetric<double>(suppData.properties.skyCover, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.Skycover = new SkyCoverValue { Value = val, Duration = dur });

        // 7. Mixing Height
        ExpandMetric<double>(suppData.properties.mixingHeight, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.MixingHeight = new MixingHeightValue { Value = val, Duration = dur });

        // 8. Quantitative Precipitation
        ExpandMetric<double>(suppData.properties.quantitativePrecipitation, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.QuantitativePrecipitation = new QuantitativePrecipitationValue { Value = val, Time = rec.Time, Duration = dur });

        // 9. Forecast Heat Risk (int, value > 0)
        ExpandMetric<int>(suppData.properties.heatRisk, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.HeatRisk = new HeatRiskValue { Value = val, Duration = dur },
            predicate: val => val > 0);

        // 10. Heat Index (value > 0.0)
        ExpandMetric<double>(suppData.properties.heatIndex, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.HeatIndex = new HeatIndexValue { Value = val, Duration = dur },
            predicate: val => val > 0.0);

        // 11. Wind Chill (value > -60.0)
        ExpandMetric<double>(suppData.properties.windChill, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.WindChill = new WindchillValue { Value = val, Duration = dur },
            predicate: val => val > -60.0 && val != default && val != null);

        // 12. Daily Minimum Temperature
        ExpandMetric<double>(suppData.properties.minTemperature, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.MinTemperature = new MinTemperatureValue { Value = val, Duration = dur });

        // 13. Daily Maximum Temperature
        ExpandMetric<double>(suppData.properties.maxTemperature, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.MaxTemperature = new MaxTemperatureValue { Value = val, Duration = dur });

        // 14. Relative Humidity
        ExpandMetric<double>(suppData.properties.relativeHumidity, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.RelativeHumidity = new RelativeHumidityValue { Value = val, Duration = dur });

        // 15. Dew Point
        ExpandMetric<double>(suppData.properties.dewpoint, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.DewPoint = new DewPointValue { Value = val, Duration = dur });

        // 16. Temperature
        ExpandMetric<double>(suppData.properties.temperature, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.Temperature = new TemperatureValue { Value = val, Duration = dur });

        // 17. Probability Of Thunder
        ExpandMetric<double>(suppData.properties.probabilityOfThunder, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.ProbabilityOfThunder = new ProbabilityOfThunderValue { Value = val, Time = rec.Time, Duration = dur });

        // 18. Low Visibility Occurrence Risk Index
        ExpandMetric<double>(suppData.properties.lowVisibilityOccurrenceRiskIndex, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.LowVisibilityOccurrenceRiskIndex = new LowVisibilityOccurrenceRiskIndexValue { Value = val, Duration = dur });

        // 19. Ice Accumulation
        ExpandMetric<double>(suppData.properties.iceAccumulation, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.IceAccumulation = new IceAccumulationValue { Value = val, Duration = dur });

        // 20. Snowfall Amount
        ExpandMetric<double>(suppData.properties.snowfallAmount, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.SnowfallAmount = new SnowFallAmountValue { Value = val, Duration = dur });

        // 21. Probability Of Precipitation
        ExpandMetric<double>(suppData.properties.probabilityOfPrecipitation, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.ProbabilityOfPrecipitation = new ProbabilityOfPrecipitationValue { Value = val, Duration = dur });

        // 22. Wet Bulb Globe Temperature
        ExpandMetric<double>(suppData.properties.wetBulbGlobeTemperature, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.WetBulbGlobeTemperature = new WetBulbGlobeTemperatureValue { Value = val, Duration = dur });
        // 23. Transport Wind Speed
        ExpandMetric<double>(suppData.properties.transportWindSpeed, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.TransportWindSpeed = new TransportWindSpeedValue { Value = val, Duration = dur });
        //24. Transport Wind Direction
        ExpandMetric<int>(suppData.properties.transportWindDirection, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.TransportWindDirection = new TransportWindDirectionValue { Value = val, Duration = dur });
        //25. Ceiling Height
        ExpandMetric<double>(suppData.properties.ceilingHeight, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.CeilingHeight = new CeilingHeightValue { Value = val, Duration = dur });
        //26. Visibility
        ExpandMetric<double>(suppData.properties.visibility, lookup, ParseNwsInterval,
            (rec, val, dur, _) => rec.Visibility = new VisibilitiesValue { Value = val, Duration = dur });

        return lookup.Values.OrderBy(x => x.Time).ToArray();
        //Daily Scoring for outdoor photography
        //var dailyForecastSummaries = await CreateHourlyNwsInputAsync(suppData);
        //if (dailyForecastSummaries != null)
        //{
        //    for (int y = 0; y < supplementals.Length; y++)
        //    {
        //        var dailyResults = dailyForecastSummaries.Where(x => x.Date == DateOnly.FromDateTime(supplementals[y].Time)).FirstOrDefault();
        //        supplementals[y]
        //    }
        //}
    }

    private void ExpandMetric<T>(
    NwsGridpointProperty<T>? properties,
    Dictionary<DateTime, NwsHourlySupplemental> lookup,
    Func<string, (DateTime time, int duration)> parseInterval,
    Action<NwsHourlySupplemental, T, int, string?> assign,
    Func<T, bool>? predicate = null) where T : struct
    {
        if (properties?.Values == null || properties.Values.Length == 0)
            return;

        for (int i = 0; i < properties.Values.Length; i++)
        {
            var entry = properties.Values[i];

            // HasValue handles null checking cleanly for Nullable<T> structs
            if (!entry.Value.HasValue)
                continue;

            T actualValue = entry.Value.Value;

            if (predicate != null && !predicate(actualValue))
                continue;

            var (startTime, duration) = parseInterval(entry.ValidTime);

            for (int dur = 0; dur < duration; dur++)
            {
                if (startTime.Date > _session.EndDate.Date) break;
                    DateTime targetTime = startTime.AddHours(dur);
                    if (targetTime.Date > _session.EndDate.Date) break;
                    int remainingDuration = duration - dur;

                    if (!lookup.TryGetValue(targetTime, out var item))
                    {
                        item = new NwsHourlySupplemental { Time = targetTime };
                        lookup[targetTime] = item;
                    }

                    assign(item, actualValue, remainingDuration, properties.Uom);

                
        } }
    }



    //try
    //{
    //    var wx = suppData.properties.weather;
    //    if (wx.values?.Length > 0 && wx.values[0]?.value[0]?.weather != null)
    //    {
    //        for (int i = 0; i < wx.values?.Length; i++)
    //        {
    //            string finishedWeather = string.Empty;
    //            List<string> hourlyWeatherDescription = new();
    //            var weatherStmts = new WeatherStatementsValue[wx.values[i].value.Length];
    //            var date = ParseNwsInterval(wx.values[i].validTime);
    //            for (int x = 0; x < wx.values[i].value?.Length; x++)
    //            {
    //                for (int dur = 0; dur < date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);

    //                    if (wx.values[i].value[x].weather != null)
    //                    {


    //                        weatherStmts[x] = new WeatherStatementsValue
    //                        {
    //                            Weather = wx.values[i].value[x].weather ?? string.Empty,
    //                            Coverage = wx.values[i].value[x].coverage ?? string.Empty,
    //                            Intensity = wx.values[i].value[x].intensity ?? string.Empty,
    //                            Time = newTime,
    //                            Duration = date.duration - dur,
    //                            Statement = CoverageStmt(wx.values[i].value[x].coverage, wx.values[i].value[x].intensity, wx.values[i].value[x].weather)
    //                        };


    //                        hourlyWeatherDescription.Add(weatherStmts[x].Statement);
    //                    }


    //                    var stmt = hourlyWeatherDescription.ToArray();
    //                    finishedWeather = $"At " + newTime.ToString("h tt") + ", ";
    //                    for (int s = 0; s < stmt.Length; s++)
    //                    {
    //                        if (s == stmt.Length - 1)
    //                        {
    //                            finishedWeather += stmt[s] + ". ";
    //                        }

    //                        else if (stmt.Length - 2 == s)
    //                        {
    //                            finishedWeather += stmt[s] + " and ";
    //                        }
    //                        else
    //                        {
    //                            finishedWeather += stmt[s] + ", ";
    //                        }
    //                    }
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        SupplementalWeatherStatement = finishedWeather,
    //                        Weather = weatherStmts
    //                    });
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        var date = ParseNwsInterval(suppData.properties.validTimes);
    //        supp.Add(new NwsHourlySupplemental
    //        {
    //            Time = date.time,
    //            SupplementalWeatherStatement = "There are no storms to report."
    //        });
    //    }

    //    // 2. Apparent Temperature
    //    var apparentTemp = suppData.properties.apparentTemperature;
    //    if (apparentTemp.values?.Length > 0 && apparentTemp.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < apparentTemp.values?.Length; a++)
    //        {
    //            var date = ParseNwsInterval(apparentTemp.values[a].validTime);
    //            for (int dur = 0; dur < date.duration; dur++)
    //            {
    //                DateTime newTime = date.time.AddHours(dur);

    //                if (apparentTemp.values[a].value is double at)
    //                {
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        ApparentTemperature = new ApparentTemperatureValue { Value = at, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }
    //    var windspeed = suppData.properties.windSpeed;
    //    if (windspeed.values?.Length > 0 && windspeed.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < windspeed.values?.Length; a++)
    //        {
    //            var date = ParseNwsInterval(windspeed.values[a].validTime);
    //            for (int dur = 0; dur < date.duration; dur++)
    //            {
    //                DateTime newTime = date.time.AddHours(dur);
    //                if (windspeed.values[a].value is double at)
    //                {

    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        WindSpeed = new WindSpeedValue { Value = at, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }
    //    var direction = suppData.properties.windDirection;
    //    if (direction.values?.Length > 0 && direction.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < direction.values?.Length; a++)
    //        {
    //            var date = ParseNwsInterval(direction.values[a].validTime);
    //            for (int dur = 0; dur < date.duration; dur++)
    //            {
    //                DateTime newTime = date.time.AddHours(dur);
    //                if (direction.values[a].value is int d)
    //                {
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        WindDirection = new WindDirectionValue { Value = d, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }
    //    var gust = suppData.properties.windGust;
    //    if (gust != null && gust.values.Length > 0 && gust.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < gust.values.Length; a++)
    //        {
    //            var date = ParseNwsInterval(gust.values[a].validTime);
    //            for (int dur = 0; dur < date.duration; dur++)
    //            {
    //                DateTime newTime = date.time.AddHours(dur);
    //                if (gust.values[a].value > 1 && gust.values[a].value is double at)
    //                {
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        WindGust = new WindGustValue { Value = at, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }

    //    // 3. SkyCover
    //    var skyCover = suppData.properties.skyCover;
    //    if (skyCover.values?.Length > 0 && skyCover.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < skyCover.values?.Length; a++)
    //        {
    //            var date = ParseNwsInterval(skyCover.values[a].validTime);
    //            for (int dur = 0; dur < date.duration; dur++)
    //            {
    //                DateTime newTime = date.time.AddHours(dur);
    //                if (skyCover.values[a].value is double sc)
    //                {
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        Skycover = new SkyCoverValue { Value = sc, UnitCode = skyCover.uom, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }

    //    // 3. Mixing Height
    //    var mixingHeight = suppData.properties.mixingHeight;
    //    if (mixingHeight.values?.Length > 0 && mixingHeight.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < mixingHeight.values?.Length; a++)
    //        {
    //            var date = ParseNwsInterval(mixingHeight.values[a].validTime);
    //            for (int dur = 0; dur < date.duration; dur++)
    //            {
    //                DateTime newTime = date.time.AddHours(dur);
    //                if (mixingHeight.values[a].value is double mh)
    //                {
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        MixingHeight = new MixingHeightValue { Value = mh, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }


    //    // 5. Quantitative Precipitation
    //    var qp = suppData.properties.quantitativePrecipitation;
    //    if (qp.values?.Length > 0 && qp.values[0]?.value != null)
    //    {
    //        for (int a = 0; a < qp.values?.Length; a++)
    //        {
    //            if (qp.values[a].value is double quanPrec)
    //            {
    //                var date = ParseNwsInterval(qp.values[a].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    supp.Add(new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        QuantitativePrecipitation = new QuantitativePrecipitationValue { Value = quanPrec, Time = newTime, Duration = date.duration - dur }
    //                    });
    //                }
    //            }
    //        }
    //    }

    //    // 6. ForecastHeatRisk (This Feels like temperature with a humidity factor
    //    var heat = suppData.properties.heatRisk;

    //    if (heat != null && heat.values?.Length > 0 && heat.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < heat.values?.Length; h++)
    //        {
    //            if (heat.values[h].value > 0 && heat.values[h].value is int heatrisknbr)
    //            {
    //                var date = ParseNwsInterval(heat.values[h].validTime);
    //                for (int dur = 0; dur < date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var heatrisk = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        HeatRisk = new HeatRiskValue { Value = heatrisknbr, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(heatrisk);
    //                }
    //            }
    //        }
    //    }

    //    // 7. ApparentTemperature and humidity
    //    var heatIndex = suppData.properties.heatIndex;
    //    if (heatIndex != null && heatIndex.values?.Length > 0 && heatIndex.values[0]?.value != default)
    //    {
    //        for (int h = 0; h < heatIndex.values?.Length; h++)
    //        {
    //            if (heatIndex.values[h].value > 0 && heatIndex.values[h].value is double heatindexdb)
    //            {
    //                var date = ParseNwsInterval(heatIndex.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var nwsHeatIndex = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        HeatIndex = new HeatIndexValue { Value = heatindexdb, UnitCode = heatIndex.uom, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(nwsHeatIndex);
    //                }
    //            }
    //        }
    //    }

    //    // 8. WindChill
    //    var windchill = suppData.properties.windChill;
    //    if (windchill != null && windchill.values?.Length > 0 && windchill.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < windchill.values?.Length; h++)
    //        {
    //            if (windchill.values[h].value > -60 && windchill.values[h].value is double wc)
    //            {
    //                var date = ParseNwsInterval(windchill.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var windChill = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        WindChill = new WindchillValue { Value = wc, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(windChill);
    //                }
    //            }
    //        }
    //    }

    //    // 9. Daily Minimum Temperature
    //    var mintemp = suppData.properties.minTemperature;

    //    if (mintemp.values?.Length > 0 && mintemp.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < mintemp.values?.Length; h++)
    //        {
    //            if (mintemp.values[h].value is double mt)
    //            {
    //                var date = ParseNwsInterval(mintemp.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var minTemp = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        MinTemperature = new MinTemperatureValue { Value = mt, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(minTemp);
    //                }
    //            }
    //        }
    //    }

    //    // 10. Daily Maximum Temperature
    //    var maxtemp = suppData.properties.maxTemperature;

    //    if (maxtemp.values?.Length > 0 && maxtemp.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < maxtemp.values?.Length; h++)
    //        {
    //            if (maxtemp.values[h].value is double mt)
    //            {
    //                var date = ParseNwsInterval(maxtemp.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var maxTemp = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        MaxTemperature = new MaxTemperatureValue { Value = mt, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(maxTemp);
    //                }
    //            }
    //        }
    //    }

    //    // 11. Relative RelativeHumidity
    //    var rh = suppData.properties.relativeHumidity;

    //    if (rh.values?.Length > 0 && rh.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < rh.values?.Length; h++)
    //        {
    //            if (rh.values[h].value is double relHum)
    //            {
    //                var date = ParseNwsInterval(rh.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var rhValue = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        RelativeHumidity = new RelativeHumidityValue { Value = relHum, UnitCode = rh.uom, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(rhValue);
    //                }
    //            }
    //        }
    //    }

    //    // 12: Dew Point
    //    var dp = suppData.properties.dewpoint;

    //    if (dp.values?.Length > 0 && dp.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < dp.values?.Length; h++)
    //        {
    //            if (dp.values[h].value is double dew)
    //            {
    //                var date = ParseNwsInterval(dp.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var dewPoint = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        DewPoint = new DewPointValue { Value = dew, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(dewPoint);
    //                }
    //            }
    //        }
    //    }
    //    // 13. Temperature
    //    var temp = suppData.properties.temperature;

    //    if (temp.values?.Length > 0 && temp.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < temp.values?.Length; h++)
    //        {
    //            if (temp.values[h].value is double tmp)
    //            {
    //                var date = ParseNwsInterval(temp.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var temperature = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        Temperature = new TemperatureValue { Value = tmp, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(temperature);
    //                }
    //            }
    //        }
    //    }

    //    // 14. Thunder
    //    var thunder = suppData.properties.probabilityOfThunder;

    //    if (thunder.values?.Length > 0 && thunder.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < thunder.values?.Length; h++)
    //        {
    //            if (thunder.values[h].value is double ts)
    //            {
    //                var date = ParseNwsInterval(thunder.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var tstorm = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        ProbabilityOfThunder = new ProbabilityOfThunderValue
    //                        {
    //                            Time = newTime,
    //                            Duration = date.duration - dur,
    //                            Value = ts,
    //                        }
    //                    };
    //                    supp.Add(tstorm);
    //                }
    //            }
    //        }
    //    }

    //    // 15. Low DistanceVisible Occurence Risk Index
    //    var lvori = suppData.properties.lowVisibilityOccurrenceRiskIndex;

    //    if (lvori.values?.Length > 0 && lvori.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < lvori.values?.Length; h++)
    //        {
    //            if (lvori.values[h].value is double vis)
    //            {
    //                var date = ParseNwsInterval(lvori.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var lvoriValue = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        LowVisibilityOccurrenceRiskIndex = new LowVisibilityOccurrenceRiskIndexValue
    //                        {
    //                            Duration = date.duration - dur,
    //                            Value = vis,

    //                        }
    //                    };
    //                    supp.Add(lvoriValue);
    //                }
    //            }
    //        }
    //    }

    //    // 16. Ice Accumulation
    //    var ice = suppData.properties.iceAccumulation;

    //    if (ice.values?.Length > 0 && ice.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < ice.values?.Length; h++)
    //        {
    //            if (ice.values[h].value is double iceAccum)
    //            {
    //                var date = ParseNwsInterval(ice.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var iceAccumulate = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        IceAccumulation = new IceAccumulationValue { Value = iceAccum, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(iceAccumulate);
    //                }
    //            }
    //        }
    //    }

    //    // 17. Snowfall Amount
    //    var snow = suppData.properties.snowfallAmount;

    //    if (snow.values?.Length > 0 && snow.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < snow.values?.Length; h++)
    //        {
    //            if (snow.values[h].value is double snowy)
    //            {
    //                var date = ParseNwsInterval(snow.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var snowFall = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        SnowfallAmount = new SnowFallAmountValue { Value = snowy, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(snowFall);
    //                }
    //            }
    //        }
    //    }

    //    // 18. Probability Of Precipitation
    //    var precip = suppData.properties.probabilityOfPrecipitation;

    //    if (precip.values?.Length > 0 && precip.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < precip.values?.Length; h++)
    //        {
    //            if (precip.values[h].value is double rain)
    //            {
    //                var date = ParseNwsInterval(precip.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var probPrecip = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        ProbabilityOfPrecipitation = new ProbabilityOfPrecipitationValue { Value = rain, UnitCode = precip.uom, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(probPrecip);
    //                }
    //            }
    //        }
    //    }
    //    var wbgt = suppData.properties.wetBulbGlobeTemperature;

    //    if (wbgt.values?.Length > 0 && wbgt.values[0]?.value != null)
    //    {
    //        for (int h = 0; h < wbgt.values?.Length; h++)
    //        {
    //            if (wbgt.values[h].value is double wet)
    //            {
    //                var date = ParseNwsInterval(wbgt.values[h].validTime);
    //                for (int dur = 0; dur <= date.duration; dur++)
    //                {
    //                    DateTime newTime = date.time.AddHours(dur);
    //                    var wetBulb = new NwsHourlySupplemental
    //                    {
    //                        Time = newTime,
    //                        WetBulbGlobeTemperature = new WetBulbGlobeTemperatureValue { Value = wet, Duration = date.duration - dur }
    //                    };
    //                    supp.Add(wetBulb);
    //                }
    //            }
    //        }
    //    }

    //    NwsHourlySupplemental[] supplementals = supp.OrderBy(o => o.Time).GroupBy(g => g.Time).Select(w => new NwsHourlySupplemental
    //    {
    //        Time = w.Key,
    //        ApparentTemperature = w.Where(x => x.ApparentTemperature != null).Max(y => y.ApparentTemperature) as ApparentTemperatureValue,
    //        IceAccumulation = w.Where(x => x.IceAccumulation != null).Max(y => y.IceAccumulation) as IceAccumulationValue,
    //        SnowfallAmount = w.Where(x => x.SnowfallAmount != null).Max(y => y.SnowfallAmount) as SnowFallAmountValue,
    //        DewPoint = w.Where(x => x.DewPoint != null).Max(y => y.DewPoint) as DewPointValue,
    //        HeatIndex = w.Where(x => x.HeatIndex != null).Max(y => y.HeatIndex) as HeatIndexValue,
    //        HeatRisk = w.Where(x => x.HeatRisk != null).Max(y => y.HeatRisk) as HeatRiskValue,
    //        LowVisibilityOccurrenceRiskIndex = w.Where(x => x.LowVisibilityOccurrenceRiskIndex != null).Max(y => y.LowVisibilityOccurrenceRiskIndex) as LowVisibilityOccurrenceRiskIndexValue,
    //        MaxTemperature = w.Where(x => x.MaxTemperature != null).Max(y => y.MaxTemperature) as MaxTemperatureValue,
    //        MinTemperature = w.Where(x => x.MinTemperature != null).Max(y => y.MinTemperature) as MinTemperatureValue,
    //        ProbabilityOfPrecipitation = w.Where(x => x.ProbabilityOfPrecipitation != null).Max(y => y.ProbabilityOfPrecipitation) as ProbabilityOfPrecipitationValue,
    //        ProbabilityOfThunder = w.Where(x => x.ProbabilityOfThunder != null).Max(y => y.ProbabilityOfThunder) as ProbabilityOfThunderValue,
    //        QuantitativePrecipitation = w.Where(x => x.QuantitativePrecipitation != null).Max(y => y.QuantitativePrecipitation) as QuantitativePrecipitationValue,
    //        RelativeHumidity = w.Where(x => x.RelativeHumidity != null).Max(y => y.RelativeHumidity) as RelativeHumidityValue,
    //        Skycover = w.Where(x => x.Skycover != null).Max(y => y.Skycover) as SkyCoverValue,
    //        Temperature = w.Where(x => x.Temperature != null).Max(y => y.Temperature) as TemperatureValue,
    //        Weather = w.Where(x => x.Weather != null).Max(y => y.Weather.ToArray()) as WeatherStatementsValue[],
    //        WetBulbGlobeTemperature = w.Where(x => x.WetBulbGlobeTemperature != null).Max(y => y.WetBulbGlobeTemperature) as WetBulbGlobeTemperatureValue,
    //        WindChill = w.Where(x => x.WindChill != null).Max(y => y.WindChill) as WindchillValue,
    //        WindGust = w.Where(x => x.WindGust != null).Max(y => y.WindGust) as WindGustValue,
    //        WindSpeed = w.Where(x => x.WindSpeed != null).Max(y => y.WindSpeed) as WindSpeedValue,
    //        WindDirection = w.Where(x => x.WindDirection != null).Max(y => y.WindDirection) as WindDirectionValue
    //    }).ToArray();

    //    return supplementals;
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Error while processing NwsData Supplementals: {ex}");
    //    }
    //    return default;
    //}

    private async Task GetNwsGridCoordinates(GridDetailData.Rootobject suppData)
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
                        _session.Coordinates = poly;
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
    }

    private double CreateAverageHourlyTemperatureChangeRate()
    {
        var dates = NwsData.DailyForecasts.Select(x => x.StartTime).Distinct().ToList();
        List<double> tempChangeRate = new List<double>();
        foreach (var d in dates)
        {
            var high = NwsData.HourlyForecasts.Where(x => x.StartTime.Date == d.Date && x.IsDaytime == true).MaxBy(y => y.Temperature.Value);
            var low = NwsData.HourlyForecasts.Where(x => x.StartTime.Date == d.Date && x.IsDaytime == false).MinBy(y => y.Temperature.Value);
            if (high != null && low != null)
            {
                tempChangeRate.Add((high.Temperature.Value - low.Temperature.Value) / low.StartTime.Subtract(high.StartTime).Hours);
            }
        }
        double tempRate = tempChangeRate.Average();
        return tempRate;
    }

    public static string CoverageStmt(string coverage, string weather, string intensity = "")
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
        if (intensity == null) { intensity = ""; }


        weatherDescription += " " + intensity.Replace("_", " ").ToLowerInvariant() ?? "";

        weatherDescription += " " + weather.Replace("_", " ").ToLowerInvariant();
        return weatherDescription;
    }

    public (DateTime time, int duration) ParseNwsInterval(string interval)
    {
        // Example: "2024-05-06T12:00:00+00:00/PT1H" or "2024-05-06T00:00:00+00:00/P1D"
        if (string.IsNullOrWhiteSpace(interval))
            return (DateTime.MinValue, 0);

        var parts = interval.Split('/');
        var time = DateTimeOffset.TryParse(parts[0], out var dto) ? dto.LocalDateTime : DateTime.MinValue;
        int duration = 0;

        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            try
            {
                var ts = System.Xml.XmlConvert.ToTimeSpan(parts[1]);
                duration = (int)Math.Max(1, Math.Round(ts.TotalHours));
            }
            catch
            {
                var match = System.Text.RegularExpressions.Regex.Match(parts[1], @"P(?:(\d+)D)?(?:T(?:(\d+)H)?)?");
                if (match.Success)
                {
                    int days = int.TryParse(match.Groups[1].Value, out int d) ? d : 0;
                    int hours = int.TryParse(match.Groups[2].Value, out int h) ? h : 0;
                    duration = days * 24 + hours;
                }
            }
        }

        return (time, duration);
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