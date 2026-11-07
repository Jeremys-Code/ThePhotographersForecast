using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    public partial class GetWeatherService : ObservableObject, IGetWeatherService
    {
        private readonly HttpClient _client;
        private IWeatherReporter _weatherReporter;

        private ISession _session;

        public GetWeatherService(HttpClient client, IWeatherReporter weatherReporter, ISession session)
        {
            _client = client;
            _weatherReporter = weatherReporter;
            _session = session;

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
                string jsonFileContent = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true // optional: for readable output
                });

                File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, $"meteo_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}_{name}.json"), jsonFileContent);
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

        public async Task<WeatherForecast> BuildWeatherForecastsAsync()
        {
            var weatherForecast = new WeatherForecast();
            var daysList = new List<DayViewModel>();



            var item = new GfsDataService.Rootobject();
            string temperatureUnit = Preferences.Get("TemperatureUnit", "fahrenheit");
            string precipitationUnit = Preferences.Get("PrecipitationUnit", "inch");
            string windUnit = Preferences.Get("WindSpeedUnit", "mph");
            string apiString = $@"https://api.open-meteo.com/v1/forecast?latitude={_session.SelectedLocation.Latitude}&longitude={_session.SelectedLocation.Longitude}&daily=sunrise,sunset,daylight_duration,sunshine_duration,weather_code,temperature_2m_max,temperature_2m_min,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant,precipitation_probability_max,precipitation_probability_min,rain_sum,showers_sum,snowfall_sum,precipitation_sum,dew_point_2m_mean,uv_index_max,apparent_temperature_max,apparent_temperature_min,wet_bulb_temperature_2m_min,wet_bulb_temperature_2m_max&hourly=temperature_2m,dew_point_2m,precipitation_probability,precipitation,rain,showers,snowfall,snow_depth,weather_code,cloud_cover,visibility,wind_speed_10m,wind_direction_10m,wind_gusts_10m,is_day,sunshine_duration,relative_humidity_2m,apparent_temperature,uv_index,wet_bulb_temperature_2m&models=gfs_seamless&timezone=auto&wind_speed_unit={windUnit}&temperature_unit={temperatureUnit}&precipitation_unit={precipitationUnit}";
            string dateParam = $@"&start_date={_session.StartDate.ToString("yyyy-MM-dd")}&end_date={_session.EndDate.ToString("yyyy-MM-dd")}";


            apiString += dateParam;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(apiString));
                item = await GetDataAsync<GfsDataService.Rootobject>(request, "meteo");
                if (item == null) return new WeatherForecast();

                var allHourlyForecasts = await GetHourlyAsync(item);
                if (allHourlyForecasts == null) return new WeatherForecast();

                //var eventModelsWeather = await AddForecastToEventsAsync(eventModels, allHourlyForecasts);

                Debug.WriteLine($"WEATHERSERVICE PROCESSED:: Hourly count: {allHourlyForecasts.Length};");

                var timeZone = item.timezone;
                try
                {

                    _session.SelectedTimeZoneOffset = item.utc_offset_seconds;
                    _session.SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(item.timezone) ?? TimeZoneInfo.Local;

                }
                catch (Exception tzEx)
                {
                    Debug.WriteLine($"TimeZone '{timeZone}' not found, falling back to Local: {tzEx.Message}");
                    _session.SelectedTimeZone = TimeZoneInfo.Local;
                }



                weatherForecast.DailyForecasts = new DailyForecast[item.daily.time.Length];
                for (int w = 0; w < item.daily.time.Length; w++)
                {

                    var periodDate = DateOnly.TryParse(item.daily.time[w], out var dt);
                    if (periodDate == false)
                    {
                        Debug.WriteLine("The Hourly date was not parsable.");
                        return new WeatherForecast();
                    }




                    weatherForecast.DailyForecasts[w] = new DailyForecast
                    {
                        Date = dt,
                        Sunrise = DateTime.TryParse(item.daily.sunrise[w], out var sr) ? sr : default,
                        Sunset = DateTime.TryParse(item.daily.sunset[w], out var ss) ? ss : default,
                        DayLightDuration = new DoubleUnitValue { Value = item.daily.daylight_duration[w], Unit = item.daily_units.daylight_duration },
                        SunshineDuration = new DoubleUnitValue { Value = item.daily.sunshine_duration[w], Unit = item.daily_units.sunshine_duration },
                        WeatherCode = item.daily.weather_code[w],
                        WeatherCodeDescription = WmoLookup.GetWeather(item.daily.weather_code[w]).Description,
                        DailyIcon = WmoLookup.GetWeather(item.daily.weather_code[w]).IconImage,
                        Temperature2mMax = new DoubleUnitValue { Value = item.daily.temperature_2m_max[w], Unit = item.daily_units.temperature_2m_max },
                        Temperature2mMin = new DoubleUnitValue { Value = item.daily.temperature_2m_min[w], Unit = item.daily_units.temperature_2m_min },
                        WindSpeed10mMax = new DoubleUnitValue { Value = item.daily.wind_speed_10m_max[w], Unit = item.daily_units.wind_speed_10m_max },
                        WindGusts10mMax = new DoubleUnitValue { Value = item.daily.wind_gusts_10m_max[w], Unit = item.daily_units.wind_gusts_10m_max },
                        WindDirection10mDominant = new IntUnitValue { Value = item.daily.wind_direction_10m_dominant[w], Description = WindDirection(item.daily.wind_direction_10m_dominant[w]), Unit = item.daily_units.wind_direction_10m_dominant },
                        PrecipitationProbabilityMax = new DoubleUnitValue { Value = item.daily.precipitation_probability_max[w], Unit = item.daily_units.precipitation_probability_max },
                        PrecipitationProbabilityMin = new DoubleUnitValue { Value = item.daily.precipitation_probability_min[w], Unit = item.daily_units.precipitation_probability_min },
                        RainSum = new DoubleUnitValue { Value = item.daily.rain_sum[w], Unit = item.daily_units.rain_sum },
                        ShowersSum = new DoubleUnitValue { Value = item.daily.showers_sum[w], Unit = item.daily_units.showers_sum },
                        SnowfallSum = new DoubleUnitValue { Value = item.daily.snowfall_sum[w], Unit = item.daily_units.snowfall_sum },
                        PrecipitationSum = new DoubleUnitValue { Value = item.daily.precipitation_sum[w], Unit = item.daily_units.precipitation_sum },
                        DewPoint2mMean = new DoubleUnitValue { Value = item.daily.dew_point_2m_mean[w], Unit = item.daily_units.dew_point_2m_mean },
                        ApparentTemperatureMax = new DoubleUnitValue { Value = item.daily.apparent_temperature_max[w], Unit = item.daily_units.apparent_temperature_max },
                        ApparentTemperatureMin = new DoubleUnitValue { Value = item.daily.apparent_temperature_min[w], Unit = item.daily_units.apparent_temperature_min },
                        UvIndexMax = new DoubleUnitValue
                        {
                            Value = item.daily.uv_index_max[w],
                            Description = item.daily.uv_index_max[w] switch
                            {
                                1 or 2 => "Green - No protection needed.You can safely stay outside using minimal sun protection.",
                                3 or 4 or 5 => "Yellow - Moderate Protection needed. Seek shade during late morning through mid-afternoon. When outside, generously apply broad-spectrum SPF-15 or higher sunscreen on exposed skin, and wear protective clothing, a wide-brimmed hat, and sunglasses.",
                                6 or 7 => "Orange - Protection needed. Seek shade during late morning through mid-afternoon. When outside, generously apply broad-spectrum SPF-15 or higher sunscreen on exposed skin, and wear protective clothing, a wide-brimmed hat, and sunglasses.",
                                8 or 9 or 10 => "Red - Extra protection needed. Be careful outside, especially during late morning through mid-afternoon. If your shadow is shorter than you, seek shade and wear protective clothing, a wide-brimmed hat, and sunglasses, and generously apply a minimum of  SPF-15, broad-spectrum sunscreen on exposed skin.",
                                11 => "Purple - Extra protection needed. Be careful outside, especially during late morning through mid-afternoon. If your shadow is shorter than you, seek shade and wear protective clothing, a wide-brimmed hat, and sunglasses, and generously apply a minimum of  SPF-15, broad-spectrum sunscreen on exposed skin.",
                                > 11 => "Purple - Extra protection needed. Be careful outside, especially during late morning through mid-afternoon. If your shadow is shorter than you, seek shade and wear protective clothing, a wide-brimmed hat, and sunglasses, and generously apply a minimum of  SPF-15, broad-spectrum sunscreen on exposed skin.",
                                _ => "Green - No protection needed.You can safely stay outside using minimal sun protection."
                            },
                            Unit = item.daily_units.uv_index_max
                        },
                        WetBulbTemperature2mMin = new DoubleUnitValue { Value = item.daily.wet_bulb_temperature_2m_min[w], Unit = item.daily_units.wet_bulb_temperature_2m_min },
                        WetBulbTemperature2mMax = new DoubleUnitValue { Value = item.daily.wet_bulb_temperature_2m_max[w], Unit = item.daily_units.wet_bulb_temperature_2m_max },
                        SkDailyMeteocon = MeteoconsResolver.ResolveIcon(item.daily.weather_code[w], true)
                    };


                    if (allHourlyForecasts != null)
                    {
                        var hourlyForecasts = allHourlyForecasts.Where(h => DateOnly.FromDateTime(h.HourlyTime.Date) == dt).ToArray();

                        weatherForecast.DailyForecasts[w].DailyForecastSummary = _weatherReporter.GetDailyWeatherSummary(weatherForecast.DailyForecasts[w], hourlyForecasts);

                        //var resultEvent = eventModelsWeather.Where(e => e.Date == dt).SingleOrDefault<EventModel>();

                        // if (resultEvent != null) weatherForecasts[w].DailyForecasts.DailyEvents = resultEvent;

                        weatherForecast.DailyForecasts[w].HourlyForecasts = hourlyForecasts.OrderBy(o => o.HourlyTime).ToArray();

                        //                       var day = CreateDayViewModel(weatherForecasts[w].DailyForecasts, resultEvent);
                        //                        MainThread.BeginInvokeOnMainThread(() => _session.Days.Add(day));
                        //  MainThread.BeginInvokeOnMainThread(() => _session.WeatherForecasts.Add(weatherForecasts[w]));
                        //  daysList.Add(day);
                        //f    Debug.WriteLine($"The weather service created another day's worth of Gfs Weather Count: {weatherForecasts.Length} and Day VM Count: {daysList.Count}");
                    }
                }
                //          var eventTimesArray = eventTimesWeather.OrderBy(o => o.OrderTime).ToArray();
                // var eventModelsArray = eventModelsWeather.OrderBy(o => o.Date).ToArray();

                //MainThread.BeginInvokeOnMainThread(() => _session.EventTimes = eventTimesArray.ToObservableCollection());
                //Debug.WriteLine($"SO LET IT BE WRITTEN:  From inside the weather service; ISession.EventTimes.CollectionChanged;count:{_session.EventTimes.Count}");


                //  _session.EventsData = [.. eventModelsArray];
                // Debug.WriteLine($"ISession.EventData created and filled with {_session.EventsData.Length} event models.");
                _session.WeatherForecasts = new WeatherForecast(weatherForecast);
                Debug.WriteLine($"ISession.WeatherForecasts created and filled with {_session.WeatherForecasts.DailyForecasts.Length} daily weather forecasts and {_session.WeatherForecasts.HourlyForecasts.Length} hourly weather forecasts.");




                return weatherForecast;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL ERROR IN BuildWeatherForecastsAsync: {ex}");
                return weatherForecast;
            }
        }

        // 1 Get Hourlys Retreive all Weather.SelectedDailyHourlyForecasts for the time periods requested
        private async Task<HourlyForecast[]> GetHourlyAsync(GfsDataService.Rootobject item)
        {
            try
            {
                int cnt = item.hourly.time.Length;
                var hourly = item.hourly;
                var hourlyUnits = item.hourly_units;
                HourlyForecast[] hours = new HourlyForecast[cnt];
                for (int h = 0; h < cnt; h++)
                {
                    var forecastTime = DateTime.TryParse(hourly.time[h], out var ft);
                    if (!forecastTime)
                    {
                        Debug.WriteLine($"Error Open-Meteo Hourly API is not producing a valid date time.{hourly.time[h]}");
                    }
                    var date = DateOnly.FromDateTime(ft);
                    hours[h] = new HourlyForecast
                    {
                        HourlyTime = ft,
                        Temperature2m = new DoubleUnitValue { Value = Math.Round(item.hourly.temperature_2m[h], 0), Unit = hourlyUnits.temperature_2m },
                        DewPoint2m = new DoubleUnitValue { Value = item.hourly.dew_point_2m[h], Unit = hourlyUnits.dew_point_2m },
                        PrecipitationProbability = new DoubleUnitValue { Value = item.hourly.precipitation_probability[h], Unit = hourlyUnits.precipitation_probability },
                        Precipitation = new DoubleUnitValue { Value = item.hourly.precipitation[h], Unit = hourlyUnits.precipitation },
                        Rain = new DoubleUnitValue { Value = item.hourly.rain[h], Unit = hourlyUnits.rain },
                        Showers = new DoubleUnitValue { Value = item.hourly.showers[h], Unit = hourlyUnits.showers },
                        Snowfall = new DoubleUnitValue { Value = item.hourly.snowfall[h], Unit = hourlyUnits.snowfall },
                        SnowDepth = new DoubleUnitValue { Value = item.hourly.snow_depth[h], Unit = hourlyUnits.snow_depth },
                        WeatherCode = item.hourly.weather_code[h],
                        WeatherCodeDescription = WmoLookup.GetWeather(item.hourly.weather_code[h], item.hourly.is_day[h]).Description,
                        HourlyIcon = WmoLookup.GetWeather(item.hourly.weather_code[h], item.hourly.is_day[h]).IconImage,
                        CloudCover = new SkyCoverValue { Value = (int)item.hourly.cloud_cover[h], UnitCode = hourlyUnits.cloud_cover, Time = ft },
                        DistanceVisible = new DoubleUnitValue { Value = (hourlyUnits.visibility.ToLowerInvariant() == "ft" ? Math.Round(item.hourly.visibility[h] / 5280, 0) : Math.Round(item.hourly.visibility[h] / 1000)), Unit = (hourlyUnits.visibility.ToLowerInvariant() == "ft" ? "mi" : "km") },
                        WindSpeed10m = new DoubleUnitValue { Value = item.hourly.wind_speed_10m[h], Unit = hourlyUnits.wind_speed_10m },
                        WindDirection10m = new IntUnitValue { Value = item.hourly.wind_direction_10m[h], Description = WindDirection(item.hourly.wind_direction_10m[h]), Unit = hourlyUnits.wind_direction_10m },
                        WindGusts10m = new DoubleUnitValue { Value = item.hourly.wind_gusts_10m[h], Unit = hourlyUnits.wind_gusts_10m },
                        IsDay = item.hourly.is_day[h],
                        SunshineDuration = new DoubleUnitValue { Value = ((item.hourly.sunshine_duration[h] / 3600) * 100), Unit = "%" },
                        RelativeHumidity2m = new DoubleUnitValue { Value = item.hourly.relative_humidity_2m[h], Unit = hourlyUnits.relative_humidity_2m },
                        ApparentTemperature = new DoubleUnitValue { Value = item.hourly.apparent_temperature[h], Unit = hourlyUnits.apparent_temperature },
                        UvIndex = new DoubleUnitValue
                        {
                            Value = item.hourly.uv_index[h],
                            Description = item.hourly.uv_index[h] switch
                            {
                                1 or 2 => "Green - No protection needed.You can safely stay outside using minimal sun protection.",
                                3 or 4 or 5 => "Yellow - Moderate Protection needed. Seek shade during late morning through mid-afternoon. When outside, generously apply broad-spectrum SPF-15 or higher sunscreen on exposed skin, and wear protective clothing, a wide-brimmed hat, and sunglasses.",
                                6 or 7 => "Orange - Protection needed. Seek shade during late morning through mid-afternoon. When outside, generously apply broad-spectrum SPF-15 or higher sunscreen on exposed skin, and wear protective clothing, a wide-brimmed hat, and sunglasses.",
                                8 or 9 or 10 => "Red - Extra protection needed. Be careful outside, especially during late morning through mid-afternoon. If your shadow is shorter than you, seek shade and wear protective clothing, a wide-brimmed hat, and sunglasses, and generously apply a minimum of  SPF-15, broad-spectrum sunscreen on exposed skin.",
                                11 => "Purple - Extra protection needed. Be careful outside, especially during late morning through mid-afternoon. If your shadow is shorter than you, seek shade and wear protective clothing, a wide-brimmed hat, and sunglasses, and generously apply a minimum of  SPF-15, broad-spectrum sunscreen on exposed skin.",
                                > 11 => "Purple - Extra protection needed. Be careful outside, especially during late morning through mid-afternoon. If your shadow is shorter than you, seek shade and wear protective clothing, a wide-brimmed hat, and sunglasses, and generously apply a minimum of  SPF-15, broad-spectrum sunscreen on exposed skin.",
                                _ => "Green - No protection needed.You can safely stay outside using minimal sun protection."
                            },
                            Unit = hourlyUnits.uv_index
                        },
                        WetBulbTemperature2m = new DoubleUnitValue { Value = item.hourly.wet_bulb_temperature_2m[h], Unit = hourlyUnits.wet_bulb_temperature_2m },
                        SkHourlyMeteocon = MeteoconsResolver.ResolveIcon(item.hourly.weather_code[h], item.hourly.is_day[h] == 1 ? true : false)
                    };

                    var result = _weatherReporter.GetHourlyWeatherSummary(hours[h]);
                    if (result != null)
                    {
                        hours[h].HourlyForecastSummary = result;
                        Debug.WriteLine($"Hourly Summary result includes: health {result.HealthSafetySummary != null}, weatther {result.WeatherSummary != null}, storm {result.StormSummary != null}");
                    }
                    else
                    {
                        Debug.WriteLine($"Hourly Summary failed: health {result?.HealthSafetySummary == null}, weatther {result?.WeatherSummary == null}, storm {result?.StormSummary == null} ");
                    }

                }// End of HourlyForecast For Loop
                return hours;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HOURLYSUMMARIES FAILED AND CAUGHT THE FOLLOW EXCEPTION {ex}");
                return default;
            }
        }
        // 2 use hourlies with the passed in eventModels while daily's run.
        //private async Task<EventModel[]> AddForecastToEventsAsync(EventModel[] eventModels, HourlyForecast[] hourlyForecasts)
        //{

        //    for (int e = 0; e < eventModels.Length; e++)
        //    {
        //        for (int i = 0; i < eventModels[e].EventTimes.Length; i++)
        //        {
        //            eventModels[e].EventTimes[i].Forecast = hourlyForecasts.Where(f => f.HourlyTime == eventModels[e].EventTimes[i].WeatherHour).SingleOrDefault();
        //        }
        //    }
        //    return eventModels;
        //}
        //3 create dayviewmodels
        //private DayViewModel CreateDayViewModel(DailyForecasts dailyForecast, EventModel eventModelWeather)
        //{
        //    DateTime date = new DateTime(dailyForecast.Date, TimeOnly.MinValue);
        //    var eventTimes = dailyForecast.DailyEvents?.EventTimes;
        //    var hourly = dailyForecast.SelectedDailyHourlyForecasts;
        //    DayViewModel detailsViewModel = new DayViewModel
        //    {
        //        Date = date,
        //        IsToday = false,
        //        IsEmpty = false,
        //        IsEventDay = true,
        //        IsEventWindowDay = false,
        //        Day = date.Day,
        //        IsFlipped = false,
        //        Events = eventModelWeather,
        //        WeatherDetails = hourly,

        //    }).ToArray()
        ////          };

        //              return detailsViewModel;
        //}





        public string WindDirection(int windDirection)
        {
            string direction = string.Empty;
            switch (windDirection)
            {
                case >= 349 or < 11:
                    direction = "North";
                    break;
                case >= 11 and < 34:
                    direction = "North-Northeast";
                    break;
                case >= 34 and < 56:
                    direction = "Northeast";
                    break;
                case >= 56 and < 79:
                    direction = "East-Northeast";
                    break;
                case >= 79 and < 101:
                    direction = "East";
                    break;
                case >= 101 and < 124:
                    direction = "East-Southeast";
                    break;
                case >= 124 and < 146:
                    direction = "Southeast";
                    break;
                case >= 146 and < 169:
                    direction = "South-Southeast";
                    break;
                case >= 169 and < 191:
                    direction = "South";
                    break;
                case >= 191 and < 214:
                    direction = "South-Southwest";
                    break;
                case >= 214 and < 236:
                    direction = "Southwest";
                    break;
                case >= 236 and < 259:
                    direction = "West-Southwest";
                    break;
                case >= 259 and < 281:
                    direction = "West";
                    break;
                case >= 281 and < 304:
                    direction = "West-Northwest";
                    break;
                case >= 304 and < 326:
                    direction = "Northwest";
                    break;
                case >= 326 and < 349:
                    direction = "North-Northwest";
                    break;
            }
            return direction;
        }
    }
}




// This produces the order of importance. Daily Weather > TimeOfDayEvents of the Day > Weather for the times of those events
//  var eventWeather = new ObservableCollection<EventsViewModel>();



//if (_ss.EventsData != null)
//{
//    // Because this is in the middle of a daily for loop we need to work with the events of the date being interated over.
//    var dayEvents = _ss.EventsData.EventTimes.Where(x => x.WeatherHour.Time == new DateTime(periodDate, TimeOnly.MinValue)).ToObservableCollection();
//    foreach (var de in dayEvents)
//    {
//        EventsViewModel events = new EventsViewModel();
//        events.GetEvent = de;
//        events.CurrentHourly = allHourlyForecasts.Where(x => x.Time == de.WeatherHour).FirstOrDefault();
//        eventWeather.Add(events);
//    }
//}
//         weather.SelectedDailyHourlyForecasts = new ObservableCollection<HourlyForecast>(allHourlyForecasts.Where(x => x.Time.Time == DateTIme(periodDate, TimeOnly.MinValue)).OrderBy(x => x.ForecastTime));

//DayViewModel detailsViewModel = new DayViewModel
//{
//    DailyWeather = dailyForecast,
//    DailyEvents = eventWeather.OrderBy(x => x.GetEvent.OrderTime).ToArray(),
//    DailyForecasts = dailyForecast.WeatherSummary,
//    DailyIcon = dailyForecast.DailyIcon,
//    DailyName = dailyForecast.TimeOfDayName,
//    TimePeriod = GetTimePeriodTitle(dailyForecast),
//    DisplayName = _ss.SelectedLocation.DisplayName
//};

//DailyStartTime = dailyForecast.Time
//Time = new DateTime(dailyForecast.Time, TimeOnly.MinValue),
//IsToday = false,
//IsEmpty = false,
//IsEventDay = true,
//IsEventWindowDay = false,
//Day = new DateTime(dailyForecast.Time, TimeOnly.MinValue).Day,
//IsFlipped = false,
//EventsData = eventWeather.Where(x => x.Time == periodDate).ToObservableCollection(),
//WeatherDetails = new WeatherDetailsViewModel
//{
//    Elevation = (int)Math.Round(weather.Elevation, 0),
//    LocalTime = new DateTime(dailyForecast.Time, TimeOnly.MinValue),
//    Location = selectedLocation.DisplayName,
//    Summary = dailyForecast.WeatherSummary,
//    OutdoorHealthSafety = dailyForecast.HealthSafetySummary,
//    WmoImageUrl = dailyForecast.TimeOfDayWeatherImage,
//    HourlyWeather = hw.Select(wh => new HourlyWeatherViewModel
//    {
//        Hour = wh.ForecastTime,
//        TimeOfDay = wh.TimeOfDay,
//        WmoImageUrl = wh.TimeOfDayWeatherImage,
//        Summary = wh.WeatherSummary,
//        OutdoorHealthSafety = wh.HealthSafetySummary,
//        TimeOfDayEvents = eventWeather.Where(t => t.Time == periodDate).OrderBy(x => x.OrderTime).ToObservableCollection(),
//    }).OrderBy(o => o.Hour).ToObservableCollection()
//}
//   };

//  DayViewModels.Add(detailsViewModel);
//GfsDataService.Add(weather);

// } // End of Day For Loop
//             Debug.WriteLine($"Success {DateTime.UtcNow}: Weather data successfully contains {WeatherForecasts.Count} entries. {WeatherForecasts.Where(x => x.Forecast != null).Count()} daily entries. {WeatherForecasts.SelectMany(x => x.SelectedDailyHourlyForecasts).Count()} hourly entries.");
//  } // end of if item != null