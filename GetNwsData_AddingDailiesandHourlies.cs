using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;


namespace ThePhotographersForecast.Services;

public partial class GetNwsData : ObservableObject, IGetNwsData
{
    private readonly HttpClient _client;
    private readonly ILoggingService _log;
    private ISession _session;
    [ObservableProperty]
    public partial string DailyUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string HourlyUrl { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string DetailUrl { get; set; } = string.Empty;

    public GetNwsData(IHttpClientFactory client, ISession session, ILoggingService log)
    {
        _client = client.CreateClient("nwsClient");
        _session = session;
        _log = log;
    }
    private async Task<T?> GetDataAsync<T>(HttpRequestMessage request)
    {
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ApplicationException($"ERROR: NWSCLIENT FAILED AT GETTING RESPONSE: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    }
                    Debug.WriteLine($"SUCCESSFULLY RECEIVED JSON FROM NWSCLIENT: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");

                    //  var rawJson = await response.Content.ReadAsStringAsync();
                    //    var cleanedJson = JsonCleaner.RemoveNullProperties(rawJson);
                    await using var stream = await response.Content.ReadAsStreamAsync();
                    // Async deserialization from stream
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = await JsonSerializer.DeserializeAsync<T>(stream, options);
                    if (result != null) return result;
                    return default;
                }
            }
            catch (ApplicationException ex) when (attempt == maxRetries)
            {
                // Log the exception or handle it as needed
                Debug.WriteLine(ex);
                return default;
            }
            catch (System.IO.IOException ex) when (attempt < maxRetries)
            {
                Debug.WriteLine($"I/O error reading from transport (attempt {attempt}): {ex.Message}");
                await Task.Delay(300 * attempt);
                continue;
            }
            catch (System.Net.Http.HttpRequestException ex) when (attempt < maxRetries)
            {
                Debug.WriteLine($"HTTP request error (attempt {attempt}): {ex.Message}");
                await Task.Delay(300 * attempt);
                continue;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Debug.WriteLine(ex);
                return default;
            }
        }

        return default;
    }
    public async Task<NwsWeatherModel> GetWeatherAsync(double latitude, double longitude)
    {

        NwsWeatherModel nwsData = new NwsWeatherModel();
        var data = new CoordinatesToGridData.Rootobject();
        try
        {
            string apiString = $@"https://api.weather.gov/points/{latitude},{longitude}";
            // var data = await Services.JsonDataService.GetJsonDataAsync<CoordinatesToGridData.Rootobject>(apiString);
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
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _session.SelectedLocation = new PhotoLocation
            {
                Latitude = latitude,
                Longitude = longitude,
                City = data.properties.relativeLocation.properties.city,
                State = data.properties.relativeLocation.properties.state,
            };
            _session.SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(data.properties.timeZone);
            _session.SelectedTimeZoneOffset = (int)_session.SelectedTimeZone.GetUtcOffset(DateTime.UtcNow).TotalSeconds;
            Debug.WriteLine($"The Selected Location is now created on Session for Gps users. {_session.SelectedLocation.City}, {_session.SelectedLocation.State}");
        });
        Debug.WriteLine($"Lat:{_session.SelectedLocation.Latitude}");
        Debug.WriteLine($"Lon:{_session.SelectedLocation.Longitude}");
        Debug.WriteLine($"DN:{_session.SelectedLocation.DisplayName}");
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
            var nwsHourlies = await hourlyTask;
            var nwsHourlySupplementals = await supplementalTask;
            nwsData = await BuildTheNwsWeatherModelAsync(nwsDailies, nwsHourlies, nwsHourlySupplementals);
        }
        else
        {
            Debug.WriteLine($"ERROR: One or more tasks failed. Daily Task: {dailyTask.Status}, Hourly Task: {hourlyTask.Status}, Supplemental Task: {supplementalTask.Status}");
            throw new Exception("One or more tasks failed while fetching NWS data.");
        }

        return nwsData;
    }
    private async Task<NwsDailyForecast[]> GetNwsDailyForecastAsync(string dailyForecastUrl)
    {
        var dailyForecast = new GridDailyData.Rootobject();
        try
        {
            using var dailyRequest = new HttpRequestMessage(HttpMethod.Get, dailyForecastUrl);
            dailyForecast = await GetDataAsync<GridDailyData.Rootobject>(dailyRequest);
            if (dailyForecast == null) return default;

            var periods = dailyForecast.properties.periods;
            var nwsDailies = new NwsDailyForecast[periods.Length];

            for (int i = 0; i < periods.Length; i++)
            {
                var icon = periods[i].icon;
                var imgSrc = await GetHourlyImageSource(icon.Replace("medium", "large"));

                nwsDailies[i] = new NwsDailyForecast
                {

                    Number = periods[i].number,
                    Name = periods[i].name,
                    StartTime = periods[i].startTime,
                    EndTime = periods[i].endTime,
                    IsDaytime = periods[i].isDaytime,
                    Temperature = periods[i].temperature,
                    TemperatureUnit = $"°{periods[i].temperatureUnit}",
                    DailyIcon = icon,
                    DailyImageSource = imgSrc,
                    ChanceOfPrecipitation = periods[i].probabilityOfPrecipitation.value ?? 0,
                    PrecipitationUnitCode = periods[i].probabilityOfPrecipitation.unitCode,
                    ShortForecast = periods[i].shortForecast,
                    DetailForecast = periods[i].detailedForecast,
                    WindSpeedDirection = $"Winds from the {GetWindDirection(periods[i].windDirection).ToLowerInvariant()} at {periods[i].windSpeed}.",
                };
            }
            return nwsDailies;
            var dailies = nwsDailies.Select(d => new MappedDaily
            {
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                DailyTemperature = d.Temperature,
                DailyTemperatureUnits = d.TemperatureUnit,
                Name = d.Name,
                DailyHigh = d.
            })
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: {ex.Message}.{ex.InnerException}");
            throw new Exception("There's an error processing the NwsData file", ex);
        }
        return null;
    }

    private async Task<NwsHourlyForecast[]> GetNwsHourlyForecastAsync(string url)
    {
        var hourlyForecasts = new GridHourlyData.Rootobject();
        try
        {
            var hourlyRequest = new HttpRequestMessage(HttpMethod.Get, url);
            hourlyForecasts = await GetDataAsync<GridHourlyData.Rootobject>(hourlyRequest);

            if (hourlyForecasts == null)
                return null;

            var pd = hourlyForecasts.properties.periods;

            var nwsHourlies = new NwsHourlyForecast[pd.Length];
            for (int d = 0; d < pd.Length; d++)
            {
                var icon = pd[d].icon;
                var imgSrc = await GetHourlyImageSource(icon.Replace("medium", "large"));

                nwsHourlies[d] = new NwsHourlyForecast
                {
                    Number = pd[d].number,
                    StartTime = pd[d].startTime,
                    EndTime = pd[d].endTime,
                    IsDaytime = pd[d].isDaytime,
                    Temperature = pd[d].temperature,
                    TemperatureUnit = $"°{pd[d].temperatureUnit}",
                    PrecipitationUnitCode = pd[d].probabilityOfPrecipitation.unitCode,
                    ChanceOfPrecipitation = (int)pd[d].probabilityOfPrecipitation.value,
                    Dewpoint = pd[d].dewpoint.unitCode == "wmoUnit:degC" ? Math.Round(pd[d].dewpoint.value * 1.8 + 32, 0) : Math.Round(pd[d].dewpoint.value, 0),
                    DewpointUnit = pd[d].dewpoint.unitCode == "wmoUnit:degC" ? "°F" : "°C",
                    Humidity = pd[d].relativeHumidity.value,
                    HourlyIcon = icon,
                    WindDirection = pd[d].windDirection,
                    WindSpeed = pd[d].windSpeed,
                    ShortForecast = pd[d].shortForecast,
                    HourlyImageSource = imgSrc
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
    // inside the outer Parallel.ForAsync body (after assigning NwsDataService.HourlyForecasts[d])
    //WR
    private async Task<NwsWeatherModel> BuildTheNwsWeatherModelAsync(NwsDailyForecast[] nwsDailies, NwsHourlyForecast[] nwsHourlies, NwsHourlySupplemental[] supplementals)
    {

        MainThread.BeginInvokeOnMainThread(() =>
         {
             _session.NwsData.HourlyForecasts = new NwsHourlyForecast[nwsHourlies.Length];
             for (int s = 0; s < nwsHourlies.Length; s++)
             {
                 var supplemental = supplementals.Where(d => d.Time == nwsHourlies[s].StartTime).First() ?? new NwsHourlySupplemental();
                 nwsHourlies[s].DetailForecast = CreateHourlyDetailDescriptionAsync(nwsHourlies[s], supplemental);
                 Debug.WriteLine($"{s}Adding DetailedForecast to Hourly Forecast.");
                 nwsHourlies[s].Supplemental = supplemental;
                 Debug.WriteLine($"{s}Adding supplemental to hourly");
             }
         });
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _session.NwsData.DailyForecasts = new NwsDailyForecast[nwsDailies.Length];
            for (int i = 0; i < nwsDailies.Length; i++)
            {
                var hours = nwsHourlies.Where(h => h.StartTime >= nwsDailies[i].StartTime && h.EndTime <= nwsDailies[i].EndTime).OrderBy(o => o.StartTime).ToArray();
                nwsDailies[i].HourlyWeathers = hours.ToArray();
                Debug.WriteLine($"Nws Hourly Detail Forecast created for {nwsHourlies[i].StartTime.ToString("MM/dd HH:MM")}");
            }
        });
        var nwsData = new NwsWeatherModel();
        nwsData.DailyForecasts = new NwsDailyForecast[nwsDailies.Length];
        MainThread.BeginInvokeOnMainThread(() =>
    {

        nwsData.DailyForecasts = [.. nwsDailies];
        nwsData.HourlyForecasts = [.. nwsHourlies];
        nwsData.HourlySupplementals = [.. supplementals];
        _session.NwsData = new NwsWeatherModel(nwsData);

    });
        return nwsData;
    }

    private string CreateHourlyDetailDescriptionAsync(NwsHourlyForecast hourly, NwsHourlySupplemental? hourlyExtra)
    {
        string feelsLike = string.Empty;
        string storm = string.Empty;
        string weather = string.Empty;
        string windGusts = string.Empty;
        string skyCover = string.Empty;
        string adjustedTemp = string.Empty;
        string thunder = string.Empty;
        string temperatureCloudCoverApparentTemp = string.Empty;
        if (hourlyExtra != null)
        {

            if (hourlyExtra?.Weather?.Statement != null)
            {
                weather = string.Join(hourlyExtra.Weather.Statement, " ");
            }

            if (hourlyExtra?.WindGust?.value != null)
            {
                windGusts = $" and gusts up to {Math.Round(hourlyExtra.WindGust.value / 1.609344, 0)} mph. ";
            }

            if (hourlyExtra?.Skycover?.value != null)
            {
                var skyCoverValue = hourlyExtra?.Skycover?.value;
                if (skyCoverValue <= 10) skyCover = " Clear sky, ";
                if (skyCoverValue <= 25) skyCover = " Only a few clouds, ";
                if (skyCoverValue <= 50) skyCover = " Only some breaks in the clouds, ";
                if (skyCoverValue <= 75) skyCover = " Mostly cloudy, ";
                skyCover = "";
            }

            if (hourlyExtra?.ProbabilityOfThunder?.value != null) thunder = hourlyExtra.ProbabilityOfThunder.description;


            if (weather != string.Empty) storm = weather;

            if (thunder != string.Empty && !(storm.Contains("thunder"))) storm += $" {thunder} ";



            //Part 2 Done Storm/conditions
            if (!(storm.ToLowerInvariant().Contains(hourly.ShortForecast.ToLowerInvariant())))
            {
                storm += hourly.ShortForecast.ToLowerInvariant();

            }
        }
        if (storm == string.Empty) storm = hourly.ShortForecast.ToLowerInvariant() + ", ";

        if (hourly.Temperature > 70)
        {
            var h = hourly.Temperature;
            switch (hourly.Dewpoint)
            {
                case <= 50:
                    feelsLike = "pleasant";
                    break;
                case > 50 and <= 60:
                    feelsLike = "comfortable.";
                    break;
                case (> 60 and <= 70) when h >= 70 && h < 75:
                    feelsLike = "humid";
                    break;
                case (> 60 and <= 70) when h > 75:
                    feelsLike = "mostly conformtable.";
                    break;
                case > 70 and <= 75:
                    feelsLike = "humid.";
                    break;
                case > 75:
                    //Dangerous when paired with high temperatures. Heat-illness risk for outdoor activity, athletes, elderly, and infants
                    feelsLike = "tropical.";
                    break;
            }
        }

        string dp = string.Empty;
        var d = hourly.Dewpoint;
        if (d < 30) dp = "less than 30";
        if (30 <= d && d < 34) dp = "low 30's,";
        if (34 <= d && d <= 36) dp = "mid 30's,";
        if (37 <= d && d < 40) dp = "high 30's";
        if (40 <= d && d < 44) dp = "low 40's";
        if (44 <= d && d <= 46) dp = "mid 40's,";
        if (47 <= d && d < 50) dp = "high 40's,";
        if (50 <= d && d < 54) dp = "low 50's";
        if (54 <= d && d <= 56) dp = "mid 50's,";
        if (57 <= d && d < 60) dp = "high 50's";
        if (60 <= d && d < 64) dp = "low 60's,";
        if (64 <= d && d <= 66) dp = "mid 60's,";
        if (67 <= d && d < 70) dp = "high 60's";
        if (70 <= d && d < 75) dp = "low 70's";
        if (d > 75) dp = d.ToString();

        string openerWelcome = hourly.IntroTimeOfDay;
        string openerLine = "! Here's The Photographer's Forecast for ";
        string openerTime = $"  {hourly.StartTime.ToString("h tt")} to {hourly.EndTime.ToString("h tt")}: ";
        // Part 1 Done Opener
        string opener = $"{openerWelcome}{openerLine}{openerTime}";
        string apparentTemp = hourlyExtra?.ApparentTemperature?.value != null ? $"{Math.Round((double)hourlyExtra.ApparentTemperature.value * 1.8 + 32, 0)}°F" : string.Empty;

        string temp = $" and {hourly.Temperature}{hourly.TemperatureUnit}.";
        string wind = $" Winds from the {GetWindDirection(hourly.WindDirection).ToLowerInvariant()} at {hourly.WindSpeed}{(windGusts != string.Empty ? windGusts : ".")}";
        string dewpoint = $"Dew point in the {dp}, ";
        string apparent = apparentTemp != string.Empty ? $" makes the outdoor temperature feel like {apparentTemp}." : ".";


        return $"{opener}{storm}{temp}{wind}";
    }

    private async Task<NwsHourlySupplemental[]> GetNwsHourlySupplementalAsync(string gridUrl)
    {
        List<NwsHourlySupplemental> supp = new List<NwsHourlySupplemental>();
        var suppData = new GridDetailData.Rootobject();
        try
        {
            var hourlySuppRequest = new HttpRequestMessage(HttpMethod.Get, gridUrl);
            suppData = await GetDataAsync<GridDetailData.Rootobject>(hourlySuppRequest);

            if (suppData == null)
                return null;

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
                            ApparentTemperature = new ApparentTemperatureValue { value = at, uom = apparentTemp.uom }
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
                    if (skyCover.values[a].value is int sc)
                    {
                        supp.Add(new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(skyCover.values[a].validTime).DateTime,
                            Skycover = new SkyCoverValue { value = sc, uom = skyCover.uom }
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
                            WindGust = new WindGustValue { value = gust, uom = gusts.uom }
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
                            QuantitativePrecipitation = new QuantitativePrecipitationValue { value = quanPrec, uom = qp.uom }
                        });
                    }
                }
            }

            // 6. HeatRisk (This Feels like temperature with a humidity factor
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
                            HeatRisk = new HeatRiskValue { value = heatrisknbr, Time = ParseNwsInterval(heat.values[h].validTime).DateTime }
                        };
                        supp.Add(heatrisk);
                    }
                }
            }
            else
            {
                var noHeatRisk = new NwsHourlySupplemental
                {
                    Time = ParseNwsInterval(suppData.properties.updateTime).DateTime,
                    HeatRisk = new HeatRiskValue { value = 0, Time = ParseNwsInterval(suppData.properties.updateTime).DateTime }
                };
                supp.Add(noHeatRisk);
            }

            // 7. HeatIndex Categories 0-11+ grouped into 4 color categories
            var heatIndex = suppData.properties.heatIndex;

            if (heatIndex.values?.Length > 0 && heatIndex.values[0]?.value != null)
            {
                for (int h = 0; h < heatIndex.values?.Length; h++)
                {
                    if (heatIndex.values[h].value is double heatindexdb)
                    {
                        var heatrisk = new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(heatIndex.values[h].validTime).DateTime,
                            HeatIndex = new HeatIndexValue { value = heatindexdb, uom = heatIndex.uom }
                        };
                        supp.Add(heatrisk);
                    }
                }
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
                            WindChill = new WindchillValue { value = wc, uom = windchill.uom }
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
                            MinTemperature = new MinTemperatureValue { value = mt, uom = mintemp.uom }
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
                            MaxTemperature = new MaxTemperatureValue { value = mt, uom = maxtemp.uom }
                        };
                        supp.Add(maxTemp);
                    }
                }
            }

            // 11. Relative Humidity
            var rh = suppData.properties.relativeHumidity;

            if (rh.values?.Length > 0 && rh.values[0]?.value != null)
            {
                for (int h = 0; h < rh.values?.Length; h++)
                {
                    if (rh.values[h].value is int relHum)
                    {
                        var rhValue = new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(rh.values[h].validTime).DateTime,
                            RelativeHumidity = new RelativeHumidityValue { value = relHum, uom = rh.uom }
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
                            DewPoint = new DewPointValue { value = dew, uom = dp.uom }
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
                            Temperature = new TemperatureValue { value = tmp, uom = temp.uom }
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
                    if (thunder.values[h].value is int ts)
                    {
                        var tstorm = new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(thunder.values[h].validTime).DateTime,
                            ProbabilityOfThunder = new ProbabilityOfThunderValue
                            {
                                value = ts,
                                uom = "%",
                                description = ts switch
                                {
                                    > 50 => "Prepare for a very high chance of thunderstorms and strong winds.",
                                    50 => "Prepare for a high chance of thunderstorms and strong winds. Make well informed traveling plans if you must go out.",
                                    >= 31 and <= 50 => "Prepare for a moderate chance of thunderstorms and strong winds.",
                                    >= 1 and <= 10 => "Low chance of thunderstorms/lightning.",
                                    _ => "No thunderstorms expected."
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
                    if (lvori.values[h].value is int vis)
                    {
                        var lvoriValue = new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(lvori.values[h].validTime).DateTime,
                            LowVisibilityOccurrenceRiskIndex = new LowVisibilityOccurrenceRiskIndexValue
                            {
                                value = vis,
                                description = vis switch
                                {
                                    1 or 2 or 3 => "Low risk of very low visibility.",
                                    4 or 5 or 6 => "Moderate risk of low visibility.",
                                    7 or 8 or 9 => "High risk of low visibility.",
                                    10 => "Exreme risk - very low visibility expected.",
                                    _ => "No low visibility risk issued."
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
                            IceAccumulation = new IceAccumulationValue { value = iceAccum, uom = ice.uom }
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
                            SnowfallAmount = new SnowFallAmountValue { value = snowy, uom = snow.uom }
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
                    if (precip.values[h].value is int rain)
                    {
                        var probPrecip = new NwsHourlySupplemental
                        {
                            Time = ParseNwsInterval(precip.values[h].validTime).DateTime,
                            ProbabilityOfPrecipitation = new ProbabilityOfPrecipitationValue { value = rain, uom = precip.uom }
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
                            WetBulbGlobeTemperature = new WetBulbGlobeTemperatureValue { value = wet, uom = wbgt.uom }
                        };
                        supp.Add(wetBulb);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Nws Details Failed: {ex}");
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

        //if (NwsHourlies.All(x => x != null))
        //    //await MainThread.InvokeOnMainThreadAsync(async () =>
        //    //{
        //    //    for (int i = 0; i < NwsHourlies.Length; i++)
        //    //    {
        //    //        var hourlyExtra = supplementals.Where(e => e.Time == NwsHourlies[i].StartTime).First() ?? new NwsHourlySupplemental();
        //        NwsHourlies[i].DetailForecast = await CreateHourlyDetailDescriptionAsync(NwsHourlies[i], hourlyExtra);
        //        Debug.WriteLine($"Nws Hourly Detail Forecast created for {NwsHourlies[i].StartTime.ToString("MM/dd HH:MM")}");
        //    }
        //});


        return supplementals;
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
    public string GetWindDirection(string windDirection)
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
}