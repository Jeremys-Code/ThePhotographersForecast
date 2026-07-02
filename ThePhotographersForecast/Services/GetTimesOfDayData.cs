using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public partial class GetTimesOfDayData : ObservableObject
    {


        private readonly HttpClient _client;
        public GetTimesOfDayData(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("photographer") ?? new HttpClient();
        }
        public async Task<(double elevation, DailyAstronomicalData astronomicalData, EventTime[] eventTimes)> GetDaysAsync(DateTime date, double latitude, double longitude)
        {
            var json = new TimeData.Rootobject();
            List<EventTime> timesOfTheDayList = new List<EventTime>();
            List<DailyAstronomicalData> astroList = new List<DailyAstronomicalData>();
            var runDate = date.ToString("yyyy-MM-dd");

            // string apiEventString = $@"https://api.ipgeolocation.io/v3/astronomy/timeSeries?apiKey=b3c99c6a0b9e44169fa96b6918495732&dateStart={startDate}&dateEnd={endDate}&lat={latitude}&long={longitude}";

            try
            {

                //     for (int d = 0; d < dates.Length; d++)
                //      {
                string apiString = $@"https://api.ipgeolocation.io/v3/astronomy?apiKey=b3c99c6a0b9e44169fa96b6918495732&lat={latitude}&long={longitude}&date={runDate}";
                var request = new HttpRequestMessage(HttpMethod.Get, apiString);
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadFromJsonAsync<TimeData.Rootobject>() ?? new TimeData.Rootobject();
                }
                DailyAstronomicalData astronomicalData = new DailyAstronomicalData();
                double elevation = 0.0;
                if (json != null)
                {
                    EventMapping[] morningMappings =
                        {
                            new ("Astronomical Twilight Begins", a => a.morning.astronomical_twilight_begin, _ => null),
                            new ("Nautical Twilight Begins", a => a.morning.nautical_twilight_begin, _ => null),
                            new ("Civil Twilight Begins", a => a.morning.civil_twilight_begin, _ => null),
                            new ("Blue Hour", a => a.morning.blue_hour_begin, a => a.morning.blue_hour_end),
                            new ("Golden Hour", a => a.morning.golden_hour_begin, a => a.morning.golden_hour_end),
                        };
                    EventMapping[] eveningMappings =
                    {
                            new ("Golden Hour", a => a.evening.golden_hour_begin, a => a.evening.golden_hour_end),
                            new ("Blue Hour", a => a.evening.blue_hour_begin, a => a.evening.blue_hour_end),
                            new ("Civil Twilight Ends", a => a.evening.civil_twilight_end, _ => null),
                            new ("Nautical Twilight Ends", a => a.evening.nautical_twilight_end, _ => null),
                            new ("Astronomical Twilight Ends",a => a.evening.astronomical_twilight_end, _ => null),
                        };
                    EventMapping[] topLevel =
                    {
                            new ("Sunrise", a => a.sunrise, _ => null),
                            new ("Sunset", a => a.sunset,  _ => null),
                            new ("Moonrise", a => a.moonrise ?? DateTime.MinValue.ToString(), _ => null),
                            new ("Moonset", a => a.moonset ?? DateTime.MinValue.ToString(), _ => null),
                            new ("Night Ends", a => a.night_end, _ => null),
                            new ("Night Begins", a => a.night_begin, _ => null),
                            new ("Solar Noon", a => a.solar_noon, _ => null),
                            new ("Midnight", a => a.mid_night, _ => null),
                        };


                    elevation = (double)json.location.elevation;


                    DateTime current = new DateTime(json.astronomy.date.Year, json.astronomy.date.Month, json.astronomy.date.Day, 0, 0, 0);
                    astronomicalData = new DailyAstronomicalData
                    {
                        Date = current,
                        MoonAltitude = json.astronomy.moon_altitude,
                        MoonAngle = json.astronomy.moon_angle,
                        MoonAzimuth = json.astronomy.moon_azimuth,
                        MoonParallacticAngle = json.astronomy.moon_parallactic_angle,
                        SunAltitude = json.astronomy.sun_altitude,
                        SunAzimuth = json.astronomy.sun_azimuth,
                        SunDistance = json.astronomy.sun_distance,
                        DayLength = json.astronomy.day_length,
                        MoonDistance = json.astronomy.moon_distance,
                        MoonIlluminationPercentage = json.astronomy.moon_illumination_percentage,
                        MoonPhase = json.astronomy.moon_phase
                    };


                    var astro = json.astronomy;
                    //foreach (var astro in json.astronomy)
                    //{
                    foreach (var m in morningMappings)
                    {
                        if (m.Name.Contains("Hour"))
                        {
                            timesOfTheDayList.Add(AddHourEvent(current, m.Name, m.AstroTime, m.OtherTime, astro));
                        }
                        else
                        {
                            timesOfTheDayList.Add(AddEvent(current, m.Name, m.AstroTime, astro));
                        }
                    }


                    foreach (var m in eveningMappings)
                    {
                        if (m.Name.Contains("Hour"))
                        {
                            timesOfTheDayList.Add(AddHourEvent(current, m.Name, m.AstroTime, m.OtherTime, astro));
                        }
                        else
                        {
                            timesOfTheDayList.Add(AddEvent(current, m.Name, m.AstroTime, astro));
                        }
                    }

                    foreach (var m in topLevel)
                    {
                        timesOfTheDayList.Add(AddEvent(current, m.Name, m.AstroTime, astro));
                    }                    // End of the daily mapped events foreach loops } 

                    // End of the astro for loop

                }// End of if json is valid
                var eventTimes = new EventTime[timesOfTheDayList.Count];
                int t = 0;
                foreach (var item in timesOfTheDayList)
                {
                    if (item.AstroTime != DateTime.MinValue)
                    {
                        eventTimes[t] = item;
                        t++;
                    }
                }
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Night Ends",
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].night_end, out var t) ? new DateTime(dateOnly, t),
                //            AstroTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].night_end, out var t) ? new DateTime(dateOnly, t)  TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Astronomical Twilight Begins",
                //            //alpha = -18.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].morning.astronomical_twilight_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.astronomical_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].morning.astronomical_twilight_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.astronomical_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });

                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Nautical Twilight Begins",
                //            //alpha = -12.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].morning.nautical_twilight_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.nautical_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].morning.nautical_twilight_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.nautical_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Civil Twilight Begins",
                //            //alpha = -6.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].morning.civil_twilight_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.civil_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].morning.civil_twilight_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.civil_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // An approximation for the blue hour, which is the period of twilight when the sun is between 4 and 6 degrees below the horizon.   
                //            EventName = "Blue Hour",
                //            //alpha = -4.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].morning.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.blue_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].morning.blue_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.blue_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].morning.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.blue_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        TimeOnly timeOnly = new TimeOnly();
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // An approximation for sunrise, which is the moment when the upper limb of the sun just starts to appear above the horizon. The actual time of sunrise can vary depending on atmospheric conditions and the observer's location, but -0.833 degrees is a commonly used value for calculating sunrise times.
                //            EventName = "Sunrise",
                //            //alpha = -0.833,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].sunrise, out var t) ? new DateTime(dateOnly, t) : null,

                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].sunrise, out  ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunrise)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].sunrise, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunrise))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // An approximation for the golden hour, the time when the sun is between sunrise and 4 degrees above the horizon.
                //            EventName = "Golden Hour",
                //            //alpha = 4.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].morning.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.golden_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].morning.golden_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.golden_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].morning.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].morning.golden_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // Solar noon
                //            EventName = "Solar Noon",
                //            //alpha = 90.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].solar_noon, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].solar_noon)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].solar_noon, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].solar_noon))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // The evening golden hour.
                //            EventName = "Golden Hour",
                //            //alpha = 4.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].evening.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.golden_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].evening.golden_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.golden_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].evening.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.golden_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // The time the sunsets.
                //            EventName = "Sunset",
                //            //alpha = -0.833,

                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].sunset, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunset)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            AstroTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].sunset, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].sunset))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            // The evening blue hour.
                //            EventName = "Blue Hour",
                //            //alpha = -6.0,
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].evening.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.blue_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].evening.blue_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.blue_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].evening.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.blue_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Civil Twilight Ends",
                //            //alpha = -6.0,
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].evening.civil_twilight_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.civil_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            AstroTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].evening.civil_twilight_end, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.civil_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Nautical Twilight Ends",
                //            //alpha = -12.0,
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].evening.nautical_twilight_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.nautical_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            AstroTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].evening.nautical_twilight_end, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.nautical_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Astronomical Twilight Ends",
                //            //alpha = -18.0,
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].evening.astronomical_twilight_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.astronomical_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            AstroTime = default,
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].evening.astronomical_twilight_end, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].evening.astronomical_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Night Begins",
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].night_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].night_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].night_begin))) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Midnight",
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].mid_night, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].mid_night)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].mid_night, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].mid_night))) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default

                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Moonrise",
                //            AstroTime = TimeOnly.TryParse(json.astronomy[i].moonrise, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonrise)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].moonrise, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonrise))) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            OtherTime = default
                //        });
                //        timesOfTheDayList.Add(new EventTime
                //        {
                //            Date = dateOnly,
                //            EventName = "Moonset",
                //            OtherTime = TimeOnly.TryParse(json.astronomy[i].moonset, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonset)) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            WeatherHour = TimeOnly.TryParse(json.astronomy[i].moonset, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy[i].moonset))) : new DateTime(dateOnly, TimeOnly.MinValue),
                //            AstroTime = default
                //        });
                //    }
                //}
                return (elevation, astronomicalData, eventTimes);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error, show a message to the user, etc.)
                Debug.WriteLine($"Error fetching event times: {ex.Message}");
            }

            return (0.0, new DailyAstronomicalData(), Array.Empty<EventTime>());
        }

        private static EventTime AddEvent(DateTime date, string eventName, Func<TimeData.Astronomy, string> Selector, TimeData.Astronomy astroItem)
        {
            EventTime newEvent = new EventTime
            {
                EventName = eventName,
                Date = date
            };
            string sStr = Selector.Invoke(astroItem);
            try
            {
                var time = TimeOnly.Parse(sStr);
                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                newEvent.AstroTime = dateTime;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{eventName} doesn't have a valid time {sStr}");
                newEvent.AstroTime = DateTime.MinValue;
            }
            return newEvent;
        }
        private static EventTime AddHourEvent(DateTime date, string eventName, Func<TimeData.Astronomy, string> startSelector, Func<TimeData.Astronomy, string> endSelector, TimeData.Astronomy astroItem)
        {
            EventTime newEvent = new EventTime
            {
                EventName = eventName,
                Date = date

            };
            string sStr = startSelector.Invoke(astroItem);
            var time = TimeOnly.Parse(sStr);
            DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
            newEvent.AstroTime = dateTime;

            string eStr = endSelector.Invoke(astroItem);
            var endTime = TimeOnly.Parse(eStr);
            DateTime endDateTime = new DateTime(date.Year, date.Month, date.Day, endTime.Hour, endTime.Minute, endTime.Second);
            newEvent.AstroTime = dateTime;

            return newEvent;
        }

        private static DateTime TruncateDt(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
        }
    }
    public record EventMapping(
    string Name,
    Func<TimeData.Astronomy, string> AstroTime,
    Func<TimeData.Astronomy, string?> OtherTime
    );
}





