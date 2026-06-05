using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Services
{
    public class GetTimesOfDayData
    {
        private readonly IHttpClientFactory _factory;
        private TimeData.Rootobject json;
        public GetTimesOfDayData(IHttpClientFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            json = new TimeData.Rootobject();
        }
        public async Task<EventModel> GetDaysAsync(double latitude, double longitude)
        {
            var _client = _factory.CreateClient("eventsApi");
            DateTime TruncateDt(DateTime dt)
            {
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            }
            EventModel eventModel = new();
            List<EventTime> timesOfTheDayList = new List<EventTime>();
            var startDate = DateTime.Now.ToString("yyyy-MM-dd");
            var endDate = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");
            string apiEventString = $@"https://api.ipgeolocation.io/v3/astronomy/timeSeries?apiKey=b3c99c6a0b9e44169fa96b6918495732&dateStart={startDate}&dateEnd={endDate}&lat={latitude}&long={longitude}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiEventString);
                request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("PfApp", "3.0"));
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadFromJsonAsync<TimeData.Rootobject>() ?? new TimeData.Rootobject();
                }

                if (json != null)
                {
                    ServiceHelper.GetService<PhotoLocation>().Elevation = json.location.elevation;

                    // Replace the incorrect usage of 'json.astronomy[i].Select(...)' with direct assignment, since 'json' is a single object, not a collection.
                    // The seveame applies to both Morning and Evening assignments.

                    int eventCount = json.astronomy.Length;
                    for (int i = 0; i < eventCount; i++)
                    {

                        DateOnly dateOnly = json.astronomy[i].date;

                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Night Ends",
                            EndTime = json.astronomy[i].night_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            StartTime = default,
                            WeatherHour = json.astronomy[i].night_end != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Astronomical Twilight Begins",
                            //alpha = -18.0,
                            StartTime = json.astronomy[i].morning.astronomical_twilight_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.astronomical_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default,
                            WeatherHour = json.astronomy[i].morning.astronomical_twilight_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.astronomical_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });

                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Nautical Twilight Begins",
                            //alpha = -12.0,
                            StartTime = json.astronomy[i].morning.nautical_twilight_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.nautical_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default,
                            WeatherHour = json.astronomy[i].morning.nautical_twilight_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.nautical_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Civil Twilight Begins",
                            //alpha = -6.0,
                            StartTime = json.astronomy[i].morning.civil_twilight_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.civil_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default,
                            WeatherHour = json.astronomy[i].morning.civil_twilight_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.civil_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // An approximation for the blue hour, which is the period of twilight when the sun is between 4 and 6 degrees below the horizon.   
                            EventName = "Blue Hour",
                            //alpha = -4.0,
                            StartTime = json.astronomy[i].morning.blue_hour_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.blue_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = json.astronomy[i].morning.blue_hour_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.blue_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].morning.blue_hour_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.blue_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // An approximation for sunrise, which is the moment when the upper limb of the sun just starts to appear above the horizon. The actual time of sunrise can vary depending on atmospheric conditions and the observer's location, but -0.833 degrees is a commonly used value for calculating sunrise times.
                            EventName = "Sunrise",
                            //alpha = -0.833,
                            StartTime = json.astronomy[i].sunrise != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunrise)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default,
                            WeatherHour = json.astronomy[i].sunrise != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunrise))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // An approximation for the golden hour, the time when the sun is between sunrise and 4 degrees above the horizon.
                            EventName = "Golden Hour",
                            //alpha = 4.0,
                            StartTime = json.astronomy[i].morning.golden_hour_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.golden_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = json.astronomy[i].morning.golden_hour_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.golden_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].morning.golden_hour_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.golden_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // Solar noon
                            EventName = "Solar Noon",
                            //alpha = 90.0,
                            StartTime = json.astronomy[i].solar_noon != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].solar_noon)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default,
                            WeatherHour = json.astronomy[i].solar_noon != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].solar_noon))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // The evening golden hour.
                            EventName = "Golden Hour",
                            //alpha = 4.0,
                            StartTime = json.astronomy[i].evening.golden_hour_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.golden_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = json.astronomy[i].evening.golden_hour_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.golden_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].evening.golden_hour_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.golden_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // The time the sunsets.
                            EventName = "Sunset",
                            //alpha = -0.833,

                            EndTime = json.astronomy[i].sunset != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunset)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            StartTime = default,
                            WeatherHour = json.astronomy[i].sunset != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunset))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            // The evening blue hour.
                            EventName = "Blue Hour",
                            //alpha = -6.0,
                            StartTime = json.astronomy[i].evening.blue_hour_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.blue_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = json.astronomy[i].evening.blue_hour_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.blue_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].evening.blue_hour_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.blue_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Civil Twilight Ends",
                            //alpha = -6.0,
                            EndTime = json.astronomy[i].evening.civil_twilight_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.civil_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            StartTime = default,
                            WeatherHour = json.astronomy[i].evening.civil_twilight_end != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.civil_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Nautical Twilight Ends",
                            //alpha = -12.0,
                            EndTime = json.astronomy[i].evening.nautical_twilight_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.nautical_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            StartTime = default,
                            WeatherHour = json.astronomy[i].evening.nautical_twilight_end != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.nautical_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Astronomical Twilight Ends",
                            //alpha = -18.0,
                            EndTime = json.astronomy[i].evening.astronomical_twilight_end != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.astronomical_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            StartTime = default,
                            WeatherHour = json.astronomy[i].evening.astronomical_twilight_end != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.astronomical_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Night Begins",
                            StartTime = json.astronomy[i].night_begin != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].night_begin != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_begin))) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Midnight",
                            StartTime = json.astronomy[i].mid_night != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].mid_night)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].mid_night != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].mid_night))) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default

                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Moonrise",
                            StartTime = json.astronomy[i].moonrise != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonrise)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].moonrise != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonrise))) : new DateTime(dateOnly, TimeOnly.MinValue),
                            EndTime = default
                        });
                        timesOfTheDayList.Add(new EventTime
                        {
                            Date = dateOnly,
                            EventName = "Moonset",
                            EndTime = json.astronomy[i].moonset != "-:-" ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonset)) : new DateTime(dateOnly, TimeOnly.MinValue),
                            WeatherHour = json.astronomy[i].moonset != "-:-" ? TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonset))) : new DateTime(dateOnly, TimeOnly.MinValue),
                            StartTime = default
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error, show a message to the user, etc.)
                Debug.WriteLine($"Error fetching event times: {ex.Message}");
            }
            var timesOfDaySorted = timesOfTheDayList.OrderBy(t => t.OrderTime);
            eventModel.EventTimes = new ObservableCollection<EventTime>(timesOfDaySorted);
            ServiceHelper.GetService<SelectLocationViewModel>().EventCount = eventModel.EventTimes.Count;
            ServiceHelper.GetService<SelectLocationViewModel>().EventsModel = eventModel;
            return eventModel;
        }
    }

}

