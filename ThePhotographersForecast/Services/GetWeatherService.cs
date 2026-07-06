using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Shared;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Services
{
    public partial class GetWeatherService : ObservableObject
    {
        private readonly HttpClient _http;

        private GetEventService _eventService;
        private WeatherReporter _weatherReporter;
        private Session _session;


        private WeatherForecast[] weatherForecasts;

        public GetWeatherService(IHttpClientFactory http, GetEventService eventService, WeatherReporter weatherReporter, Session session)
        {
            _http = http.CreateClient("photographer");
            _eventService = eventService;
            _weatherReporter = weatherReporter;
            _session = session;
        }

        public async Task BuildWeatherForecastsAsync(PhotoLocation selectedLocation, DateTime startDate, DateTime endDate)
        {
            var item = new GfsDataService.Rootobject();
            string temperatureUnit = Preferences.Get("TemperatureUnit", "fahrenheit");
            string precipitationUnit = Preferences.Get("PrecipitationUnit", "inch");
            string windUnit = Preferences.Get("WindSpeedUnit", "mph");
            string apiString = $@"https://api.open-meteo.com/v1/forecast?latitude={selectedLocation.Latitude}&longitude={selectedLocation.Longitude}&daily=sunrise,sunset,daylight_duration,sunshine_duration,weather_code,temperature_2m_max,temperature_2m_min,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant,precipitation_probability_max,precipitation_probability_min,rain_sum,showers_sum,snowfall_sum,precipitation_sum,dew_point_2m_mean,uv_index_max,apparent_temperature_max,apparent_temperature_min,wet_bulb_temperature_2m_min,wet_bulb_temperature_2m_max&hourly=temperature_2m,dew_point_2m,precipitation_probability,precipitation,rain,showers,snowfall,snow_depth,weather_code,cloud_cover,visibility,wind_speed_10m,wind_direction_10m,wind_gusts_10m,is_day,sunshine_duration,relative_humidity_2m,apparent_temperature,uv_index,wet_bulb_temperature_2m&models=gfs_seamless&timezone=auto&wind_speed_unit={windUnit}&temperature_unit={temperatureUnit}&precipitation_unit={precipitationUnit}";
            string dateParam = $@"&start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}";


            apiString += dateParam;
            try
            {

                CancellationToken ct = new CancellationToken();

                var request = new HttpRequestMessage(HttpMethod.Get, apiString);
                _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //       _http.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("jeremy@bostonfromabove.com", "BostonFromAbove"));
                var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!response.IsSuccessStatusCode)
                    return;

                item = await response.Content.ReadFromJsonAsync<GfsDataService.Rootobject>(cancellationToken: ct);


                if (item != null)
                {


                    if (_session.EventsData.Count != _session.Dates.Length || _session.EventsData == null)
                    {
                        await Shell.Current.DisplayAlertAsync("Event Data Error", "There was a problem retreiving the event data", "OK");
                        return;
                    }


                    // Early call to pull all Hourly Forecasts at once
                    var allHourlyForecasts = GetHourly(item);

                    var timeZone = item.timezone;

                    if (_session.SelectedLocation.TimeZone == string.Empty)
                        _session.SelectedLocation.TimeZone = timeZone;
                    weatherForecasts = new WeatherForecast[item.daily.time.Length];
                    for (int i = 0; i < item.daily.time.Length; i++)
                    {
                        var periodDate = DateOnly.TryParse(item.daily.time[i], out var dt) ? dt : default;


                        weatherForecasts[i] = new WeatherForecast
                        {
                            Date = periodDate,
                            Latitude = item.latitude,
                            Longitude = item.longitude,
                            GenerationTimeMs = item.generationtime_ms,
                            TimeZone = timeZone,
                            Elevation = item.elevation,
                            UtcOffsetSeconds = item.utc_offset_seconds,
                            DayForecast = new DailyForecast
                            {
                                Date = periodDate,
                                Sunrise = DateTime.TryParse(item.daily.sunrise[i], out var sr) ? sr : default,
                                Sunset = DateTime.TryParse(item.daily.sunset[i], out var ss) ? ss : default,
                                DayLightDuration = new Measurement { Value = item.daily.daylight_duration[i], Unit = item.daily_units.daylight_duration },
                                SunshineDuration = new Measurement { Value = item.daily.sunshine_duration[i], Unit = item.daily_units.sunshine_duration },
                                WeatherCode = item.daily.weather_code[i],
                                WeatherCodeDescription = WmoLookup.GetWeather(item.daily.weather_code[i]).Description,
                                DailyIcon = WmoLookup.GetWeather(item.daily.weather_code[i]).IconImage,
                                Temperature2mMax = new Measurement { Value = item.daily.temperature_2m_max[i], Unit = item.daily_units.temperature_2m_max },
                                Temperature2mMin = new Measurement { Value = item.daily.temperature_2m_min[i], Unit = item.daily_units.temperature_2m_min },
                                WindSpeed10mMax = new Measurement { Value = item.daily.wind_speed_10m_max[i], Unit = item.daily_units.wind_speed_10m_max },
                                WindGusts10mMax = new Measurement { Value = item.daily.wind_gusts_10m_max[i], Unit = item.daily_units.wind_gusts_10m_max },
                                WindDirection10mDominant = new Measurement2 { Value = item.daily.wind_direction_10m_dominant[i], Description = WindDirection(item.daily.wind_direction_10m_dominant[i]), Unit = item.daily_units.wind_direction_10m_dominant },
                                PrecipitationProbabilityMax = new Measurement { Value = item.daily.precipitation_probability_max[i], Unit = item.daily_units.precipitation_probability_max },
                                PrecipitationProbabilityMin = new Measurement { Value = item.daily.precipitation_probability_min[i], Unit = item.daily_units.precipitation_probability_min },
                                RainSum = new Measurement { Value = item.daily.rain_sum[i], Unit = item.daily_units.rain_sum },
                                ShowersSum = new Measurement { Value = item.daily.showers_sum[i], Unit = item.daily_units.showers_sum },
                                SnowfallSum = new Measurement { Value = item.daily.snowfall_sum[i], Unit = item.daily_units.snowfall_sum },
                                PrecipitationSum = new Measurement { Value = item.daily.precipitation_sum[i], Unit = item.daily_units.precipitation_sum },
                                DewPoint2mMean = new Measurement { Value = item.daily.dew_point_2m_mean[i], Unit = item.daily_units.dew_point_2m_mean },
                                ApparentTemperatureMax = new Measurement { Value = item.daily.apparent_temperature_max[i], Unit = item.daily_units.apparent_temperature_max },
                                ApparentTemperatureMin = new Measurement { Value = item.daily.apparent_temperature_min[i], Unit = item.daily_units.apparent_temperature_min },
                                UvIndexMax = new Measurement { Value = item.daily.uv_index_max[i], Unit = item.daily_units.uv_index_max },
                                WetBulbTemperature2mMin = new Measurement { Value = item.daily.wet_bulb_temperature_2m_min[i], Unit = item.daily_units.wet_bulb_temperature_2m_min },
                                WetBulbTemperature2mMax = new Measurement { Value = item.daily.wet_bulb_temperature_2m_max[i], Unit = item.daily_units.wet_bulb_temperature_2m_max }
                            }
                        };
                        var hourlyForecasts = allHourlyForecasts.Where(h => h.Date == periodDate).OrderBy(o => o.Time).ToArray();
                        var summaries = _weatherReporter.GetDailyWeatherSummary(weatherForecasts[i].DayForecast, hourlyForecasts);
                        weatherForecasts[i].DayForecast.WeatherSummary = summaries.weather;
                        weatherForecasts[i].DayForecast.PrecipitationSummary = summaries.precipitation;
                        weatherForecasts[i].DayForecast.HealthSafetySummary = summaries.health;
                        weatherForecasts[i].DayForecast.DailyEvents = _session.EventsData.Where(x => x.Date == periodDate).First();
                        weatherForecasts[i].HourlyForecasts = hourlyForecasts.ToArray();
                    }



                    _session.WeatherForecasts = new ObservableCollection<WeatherForecast>(weatherForecasts);

                    return;
                }
            }// for Daily Done
            catch (Exception ex)
            {
                Console.WriteLine($"Error {DateTime.UtcNow}: The Weather Service encountered and error. {ex}");
            }
            return;
        }//End of SelectedLocation IF

        private string GetTimePeriodTitle(DailyForecast daily)
        {
            string timePeriod = string.Empty;
            timePeriod = $"{daily.Date.ToString("dddd, MMMM dd, yyyy")}";
            return timePeriod;
        }
        // Retreive all Weather.HourlyForecasts for the time periods requested
        private ObservableCollection<HourlyForecast> GetHourly(GfsDataService.Rootobject item)
        {

            int cnt = item.hourly.time.Length;
            var hourly = item.hourly;
            var hourlyUnits = item.hourly_units;
            ObservableCollection<HourlyForecast> hours = new ObservableCollection<HourlyForecast>();
            for (int h = 0; h < cnt; h++)
            {
                var forecastTime = DateTime.TryParse(hourly.time[h], out var ft) ? ft : DateTime.MinValue;
                var date = DateOnly.FromDateTime(forecastTime);
                var hourlyForecast = new HourlyForecast
                {
                    Time = forecastTime,
                    Temperature2m = new Measurement { Value = item.hourly.temperature_2m[h], Unit = hourlyUnits.temperature_2m },
                    DewPoint2m = new Measurement { Value = item.hourly.dew_point_2m[h], Unit = hourlyUnits.dew_point_2m },
                    PrecipitationProbability = new Measurement { Value = item.hourly.precipitation_probability[h], Unit = hourlyUnits.precipitation_probability },
                    Precipitation = new Measurement { Value = item.hourly.precipitation[h], Unit = hourlyUnits.precipitation },
                    Rain = new Measurement { Value = item.hourly.rain[h], Unit = hourlyUnits.rain },
                    Showers = new Measurement { Value = item.hourly.showers[h], Unit = hourlyUnits.showers },
                    Snowfall = new Measurement { Value = item.hourly.snowfall[h], Unit = hourlyUnits.snowfall },
                    SnowDepth = new Measurement { Value = item.hourly.snow_depth[h], Unit = hourlyUnits.snow_depth },
                    WeatherCode = item.hourly.weather_code[h],
                    WeatherCodeDescription = WmoLookup.GetWeather(item.hourly.weather_code[h], item.hourly.is_day[h]).Description,
                    HourlyIcon = WmoLookup.GetWeather(item.hourly.weather_code[h], item.hourly.is_day[h]).IconImage,
                    CloudCover = new Measurement { Value = item.hourly.cloud_cover[h], Unit = hourlyUnits.cloud_cover },
                    Visibility = new Measurement { Value = (hourlyUnits.visibility.ToLowerInvariant() == "ft" ? Math.Round(item.hourly.visibility[h] / 5280, 0) : Math.Round(item.hourly.visibility[h] / 1000)), Unit = (hourlyUnits.visibility.ToLowerInvariant() == "ft" ? "mi" : "km") },
                    WindSpeed10m = new Measurement { Value = item.hourly.wind_speed_10m[h], Unit = hourlyUnits.wind_speed_10m },
                    WindDirection10m = new Measurement2 { Value = item.hourly.wind_direction_10m[h], Description = WindDirection(item.hourly.wind_direction_10m[h]), Unit = hourlyUnits.wind_direction_10m },
                    WindGusts10m = new Measurement { Value = item.hourly.wind_gusts_10m[h], Unit = hourlyUnits.wind_gusts_10m },
                    IsDay = item.hourly.is_day[h],
                    SunshineDuration = new Measurement { Value = ((item.hourly.sunshine_duration[h] / 3600) * 100), Unit = "%" },
                    RelativeHumidity2m = new Measurement { Value = item.hourly.relative_humidity_2m[h], Unit = hourlyUnits.relative_humidity_2m },
                    ApparentTemperature = new Measurement { Value = item.hourly.apparent_temperature[h], Unit = hourlyUnits.apparent_temperature },
                    UvIndex = new Measurement { Value = item.hourly.uv_index[h], Unit = hourlyUnits.uv_index },
                    WetBulbTemperature2m = new Measurement { Value = item.hourly.wet_bulb_temperature_2m[h], Unit = hourlyUnits.wet_bulb_temperature_2m }
                };
                var summaries = _weatherReporter.GetHourlyWeatherSummary(hourlyForecast);
                hourlyForecast.WeatherSummary = summaries.weather;
                hourlyForecast.HealthSafetySummary = summaries.health;
                hourlyForecast.PrecipitationSummary = summaries.precipitation;
                hours.Add(hourlyForecast);
            }// End of HourlyForecast For Loop
            return hours;
        }

        public string FromTimesOfDay(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 3):
                    return "night";
                case int h when (3 <= h && h < 12):
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




// This produces the order of importance. Daily Weather > Events of the Day > Weather for the times of those events
//  var eventWeather = new ObservableCollection<EventsViewModel>();



//if (_ss.EventsData != null)
//{
//    // Because this is in the middle of a daily for loop we need to work with the events of the date being interated over.
//    var dayEvents = _ss.EventsData.EventTimes.Where(x => x.WeatherHour.Date == new DateTime(periodDate, TimeOnly.MinValue)).ToObservableCollection();
//    foreach (var de in dayEvents)
//    {
//        EventsViewModel events = new EventsViewModel();
//        events.GetEvent = de;
//        events.CurrentHourly = allHourlyForecasts.Where(x => x.Time == de.WeatherHour).FirstOrDefault();
//        eventWeather.Add(events);
//    }
//}
//         weather.HourlyForecasts = new ObservableCollection<HourlyForecast>(allHourlyForecasts.Where(x => x.Time.Date == DateTIme(periodDate, TimeOnly.MinValue)).OrderBy(x => x.ForecastTime));

//DayViewModel detailsViewModel = new DayViewModel
//{
//    DailyWeather = dailyForecast,
//    DailyEvents = eventWeather.OrderBy(x => x.GetEvent.OrderTime).ToArray(),
//    DailyForecast = dailyForecast.WeatherSummary,
//    DailyIcon = dailyForecast.DailyIcon,
//    DailyName = dailyForecast.Name,
//    TimePeriod = GetTimePeriodTitle(dailyForecast),
//    DisplayName = _ss.SelectedLocation.DisplayName
//};

//DailyStartTime = dailyForecast.Date
//Date = new DateTime(dailyForecast.Date, TimeOnly.MinValue),
//IsToday = false,
//IsEmpty = false,
//IsEventDay = true,
//IsEventWindowDay = false,
//Day = new DateTime(dailyForecast.Date, TimeOnly.MinValue).Day,
//IsFlipped = false,
//EventModels = eventWeather.Where(x => x.Date == periodDate).ToObservableCollection(),
//WeatherDetails = new WeatherDetailsViewModel
//{
//    Elevation = (int)Math.Round(weather.Elevation, 0),
//    LocalTime = new DateTime(dailyForecast.Date, TimeOnly.MinValue),
//    Location = selectedLocation.DisplayName,
//    Summary = dailyForecast.WeatherSummary,
//    OutdoorHealthSafety = dailyForecast.HealthSafetySummary,
//    WmoImageUrl = dailyForecast.WeatherImage,
//    HourlyWeather = hw.Select(wh => new HourlyWeatherViewModel
//    {
//        Hour = wh.ForecastTime,
//        TimeOfDay = wh.TimeOfDay,
//        WmoImageUrl = wh.WeatherImage,
//        Summary = wh.WeatherSummary,
//        OutdoorHealthSafety = wh.HealthSafetySummary,
//        Events = eventWeather.Where(t => t.Date == periodDate).OrderBy(x => x.OrderTime).ToObservableCollection(),
//    }).OrderBy(o => o.Hour).ToObservableCollection()
//}
//   };

//  DayViewModels.Add(detailsViewModel);
//GfsDataService.Add(weather);

// } // End of Day For Loop
//             Debug.WriteLine($"Success {DateTime.UtcNow}: Weather data successfully contains {WeatherData.Count} entries. {WeatherData.Where(x => x.Forecast != null).Count()} daily entries. {WeatherData.SelectMany(x => x.HourlyForecasts).Count()} hourly entries.");
//  } // end of if item != null