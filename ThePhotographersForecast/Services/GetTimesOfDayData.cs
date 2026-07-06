using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.SunMoonCalc;
using ThePhotographersForecast.Services.SunMoonCalc.Dtos;

namespace ThePhotographersForecast.Services
{
    public partial class GetTimesOfDayData : ObservableObject
    {


        private readonly HttpClient _client;
        public GetTimesOfDayData(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("photographer") ?? new HttpClient();
        }
        private async Task<DailySunMoon> GetSunTimesAsync(DateTime date, double latitude, double longitude, TimeZoneInfo timeZone)
        {
            try
            {
                var sunTimes = SunCalc.GetTimes(date, latitude, longitude, timeZone);
                var moonTimes = MoonCalc.GetMoonTimes(date, latitude, longitude);
                var moonPhase = MoonCalc.GetMoonIllumination(date);

                var day = new DailySunMoon { Date = date, SunTimes = sunTimes, MoonTimes = moonTimes, Phase = moonPhase };
                return day;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching sun and moon times for {date.ToShortDateString()}: {ex.Message}");
            }
            return new DailySunMoon();
        }

        public async Task<ObservableCollection<EventModel>> GetDaysAsync(DateTime startDate, DateTime endDate, double latitude, double longitude, TimeZoneInfo timeZone)
        {
            ObservableCollection<EventModel> eventData = new ObservableCollection<EventModel>();
            int nbrOfDays = (int)Math.Round(endDate.Date.Subtract(startDate.Date).TotalDays + 1, 0);
            var sunnyMoonList = new List<DailySunMoon>(); ;
            var current = startDate.Date;
            for (int a = 0; a < nbrOfDays; a++)
            {
                var daily = await GetSunTimesAsync(current, latitude, longitude, timeZone);
                sunnyMoonList.Add(daily);
                current = current.AddDays(1).Date;
            }

            //List<DailyAstronomicalData> astroList = new List<DailyAstronomicalData>();

            try
            {
                foreach (var time in sunnyMoonList)
                {
                    List<EventTime> timesOfTheDayList = new List<EventTime>();
                    var sun = time.SunTimes;
                    var moon = time.MoonTimes;
                    var moonPhase = time.Phase;
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.NightEnd.Date, EventName = "Astronomical Twilight Ends", AstroTime = sun.NightEnd });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Night.Date, EventName = "Night Ends", AstroTime = sun.Night });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.NauticalDawn.Date, EventName = "Nautical Twilight Ends", AstroTime = sun.NauticalDawn });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Dawn.Date, EventName = "Civil Twilight Ends", AstroTime = sun.Dawn });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.BlueHour.Date, EventName = "Blue Hour", AstroTime = sun.BlueHour });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.GoldenHour.Date, EventName = "Golden Hour", AstroTime = sun.GoldenHour });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Sunrise.Date, EventName = "Sunrise", AstroTime = sun.Sunrise });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.SolarNoon.Date, EventName = "Solar Noon", AstroTime = sun.SolarNoon });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.GoldenHourEnd.Date, EventName = "Golden Hour Ends", AstroTime = sun.GoldenHourEnd });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.BlueHourEnd.Date, EventName = "Blue Hour Ends", AstroTime = sun.BlueHourEnd });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Sunset.Date, EventName = "Sunset", AstroTime = sun.Sunset });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.NauticalDusk.Date, EventName = "Nautical Twilight Begins", AstroTime = sun.NauticalDusk });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Night.Date, EventName = "Astronomical Twilight Begins", AstroTime = sun.Night });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Night.Date, EventName = "Night Begins", AstroTime = sun.Night });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Nadir.Date, EventName = "Midnight", AstroTime = sun.Nadir });
                    //timesOfTheDayList.Add(item: new EventTime { Date = sun.Dusk.Date, EventName = "Civil Twilight Begins", AstroTime = sun.Dusk });
                    //timesOfTheDayList.Add(item: new EventTime { Date = moon.Rise.Date, EventName = "Moonrise", AstroTime = moon.Rise });
                    //timesOfTheDayList.Add(item: new EventTime { Date = moon.Set.Date, EventName = "Moonset", AstroTime = moon.Set });
                    List<EventTime> events = new List<EventTime>();
                    foreach (var item in timesOfTheDayList) { if (item.AstroTime != DateTime.MinValue) { events.Add(item); } }

                    

                    var moonInfo = new MoonInfo();
                    moonInfo.Date = sun.SolarNoon.Date;
                    moonInfo.MoonPhase = moonPhase.Phase; // Replace with actual moon phase calculation
                    moonInfo.AlwaysUp = moon.AlwaysUp;
                    moonInfo.AlwaysDown = moon.AlwaysDown;

                    var sorted = new ObservableGroupedCollection<(DateOnly, TimeOfDay), EventTime>(events.OrderBy(x => x.OrderTime).GroupBy(y => (y.Date, y.TimeOfDay)).OrderBy(k => k.Key.Date));


                    var eventModel = new EventModel
                    {
                        Date = moonInfo.Date,
                        Moon = moonInfo,
                        EventTimes = sorted
                    };
                    eventData.Add(eventModel);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error processing daily sun and moon data: " + ex);
            }

            return eventData;
        }

        private static DateTime TruncateDt(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
        }
    }
}
//public record EventMapping<T>(
//string Name,
//Func<T, DateTime> AstroTime
////Func<TimeData.Astronomy, string?> OtherTime
//);
//}

//     for (int d = 0; d < dates.Length; d++)
//      {
//string apiString = $@"https://api.ipgeolocation.io/v3/astronomy?apiKey=b3c99c6a0b9e44169fa96b6918495732&lat={latitude}&long={longitude}&date={runDate}";
//var request = new HttpRequestMessage(HttpMethod.Get, apiString);
//var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
//if (response.IsSuccessStatusCode)
//{
//    json = await response.Content.ReadFromJsonAsync<TimeData.Rootobject>() ?? new TimeData.Rootobject();
//}
//DailyAstronomicalData astronomicalData = new DailyAstronomicalData();
//double elevation = 0.0;

//{
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Night Ends",
//            OtherTime = TimeOnly.TryParse(json.astronomy.night_end, out var t) ? new DateTime(dateOnly, t),
//            AstroTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.night_end, out var t) ? new DateTime(dateOnly, t)  TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.night_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Astronomical Twilight Begins",
//            //alpha = -18.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.morning.astronomical_twilight_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.astronomical_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.morning.astronomical_twilight_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.astronomical_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });

//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Nautical Twilight Begins",
//            //alpha = -12.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.morning.nautical_twilight_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.nautical_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.morning.nautical_twilight_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.nautical_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Civil Twilight Begins",
//            //alpha = -6.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.morning.civil_twilight_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.civil_twilight_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.morning.civil_twilight_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.civil_twilight_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // An approximation for the blue hour, which is the period of twilight when the sun is between 4 and 6 degrees below the horizon.   
//            EventName = "Blue Hour",
//            //alpha = -4.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.morning.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.blue_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = TimeOnly.TryParse(json.astronomy.morning.blue_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.blue_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.morning.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.blue_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        TimeOnly timeOnly = new TimeOnly();
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // An approximation for sunrise, which is the moment when the upper limb of the sun just starts to appear above the horizon. The actual time of sunrise can vary depending on atmospheric conditions and the observer's location, but -0.833 degrees is a commonly used value for calculating sunrise times.
//            EventName = "Sunrise",
//            //alpha = -0.833,
//            AstroTime = TimeOnly.TryParse(json.astronomy.sunrise, out var t) ? new DateTime(dateOnly, t) : null,

//            AstroTime = TimeOnly.TryParse(json.astronomy.sunrise, out  ? new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.sunrise)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.sunrise, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.sunrise))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // An approximation for the golden hour, the time when the sun is between sunrise and 4 degrees above the horizon.
//            EventName = "Golden Hour",
//            //alpha = 4.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.morning.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.golden_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = TimeOnly.TryParse(json.astronomy.morning.golden_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.golden_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.morning.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.morning.golden_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // Solar noon
//            EventName = "Solar Noon",
//            //alpha = 90.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.solar_noon, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.solar_noon)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.solar_noon, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.solar_noon))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // The evening golden hour.
//            EventName = "Golden Hour",
//            //alpha = 4.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.evening.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.golden_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = TimeOnly.TryParse(json.astronomy.evening.golden_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.golden_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.evening.golden_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.golden_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // The time the sunsets.
//            EventName = "Sunset",
//            //alpha = -0.833,

//            OtherTime = TimeOnly.TryParse(json.astronomy.sunset, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.sunset)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            AstroTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.sunset, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.sunset))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            // The evening blue hour.
//            EventName = "Blue Hour",
//            //alpha = -6.0,
//            AstroTime = TimeOnly.TryParse(json.astronomy.evening.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.blue_hour_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = TimeOnly.TryParse(json.astronomy.evening.blue_hour_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.blue_hour_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.evening.blue_hour_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.blue_hour_begin))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Civil Twilight Ends",
//            //alpha = -6.0,
//            OtherTime = TimeOnly.TryParse(json.astronomy.evening.civil_twilight_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.civil_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            AstroTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.evening.civil_twilight_end, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.civil_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Nautical Twilight Ends",
//            //alpha = -12.0,
//            OtherTime = TimeOnly.TryParse(json.astronomy.evening.nautical_twilight_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.nautical_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            AstroTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.evening.nautical_twilight_end, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.nautical_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Astronomical Twilight Ends",
//            //alpha = -18.0,
//            OtherTime = TimeOnly.TryParse(json.astronomy.evening.astronomical_twilight_end, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.astronomical_twilight_end)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            AstroTime = default,
//            WeatherHour = TimeOnly.TryParse(json.astronomy.evening.astronomical_twilight_end, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.evening.astronomical_twilight_end))) : new DateTime(dateOnly, TimeOnly.MinValue)
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Night Begins",
//            AstroTime = TimeOnly.TryParse(json.astronomy.night_begin, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.night_begin)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.night_begin, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.night_begin))) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Midnight",
//            AstroTime = TimeOnly.TryParse(json.astronomy.mid_night, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.mid_night)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.mid_night, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.mid_night))) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default

//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Moonrise",
//            AstroTime = TimeOnly.TryParse(json.astronomy.moonrise, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.moonrise)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.moonrise, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.moonrise))) : new DateTime(dateOnly, TimeOnly.MinValue),
//            OtherTime = default
//        });
//        timesOfTheDayList.Add(new EventTime
//        {
//            Date = dateOnly,
//            EventName = "Moonset",
//            OtherTime = TimeOnly.TryParse(json.astronomy.moonset, out var t) ? new DateTime(dateOnly, t)new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.moonset)) : new DateTime(dateOnly, TimeOnly.MinValue),
//            WeatherHour = TimeOnly.TryParse(json.astronomy.moonset, out var t) ? new DateTime(dateOnly, t)TruncateDt(new DateTime(dateOnly, TimeOnly.Parse(json.astronomy.moonset))) : new DateTime(dateOnly, TimeOnly.MinValue),
//            AstroTime = default
//        });
//    }
//

//EventMapping[] eveningMappings =
//{
//        new ("Golden Hour", a => a.evening.golden_hour_begin, a => a.evening.golden_hour_end),
//        new ("Blue Hour", a => a.evening.blue_hour_begin, a => a.evening.blue_hour_end),
//        new ("Civil Twilight Ends", a => a.evening.civil_twilight_end, _ => null),
//        new ("Nautical Twilight Ends", a => a.evening.nautical_twilight_end, _ => null),
//        new ("Astronomical Twilight Ends",a => a.evening.astronomical_twilight_end, _ => null),
//    };
//EventMapping[] topLevel =
//{
//        new ("Sunrise", a => a.sunrise, _ => null),
//        new ("Sunset", a => a.sunset,  _ => null),
//        new ("Moonrise", a => a.moonrise ?? DateTime.MinValue.ToString(), _ => null),
//        new ("Moonset", a => a.moonset ?? DateTime.MinValue.ToString(), _ => null),
//        new ("Night Ends", a => a.night_end, _ => null),
//        new ("Night Begins", a => a.night_begin, _ => null),
//        new ("Solar Noon", a => a.solar_noon, _ => null),
//        new ("Midnight", a => a.mid_night, _ => null),
//    };


//elevation = (double)json.location.elevation;


//DateTime current = new DateTime(json.astronomy.date.Year, json.astronomy.date.Month, json.astronomy.date.Day, 0, 0, 0);
//astronomicalData = new DailyAstronomicalData
//{
//    Date = current,
//    MoonAltitude = json.astronomy.moon_altitude,
//    MoonAngle = json.astronomy.moon_angle,
//    MoonAzimuth = json.astronomy.moon_azimuth,
//    MoonParallacticAngle = json.astronomy.moon_parallactic_angle,
//    SunAltitude = json.astronomy.sun_altitude,
//    SunAzimuth = json.astronomy.sun_azimuth,
//    SunDistance = json.astronomy.sun_distance,
//    DayLength = json.astronomy.day_length,
//    MoonDistance = json.astronomy.moon_distance,
//    MoonIlluminationPercentage = json.astronomy.moon_illumination_percentage,
//    MoonPhase = json.astronomy.moon_phase
//};


//var astro = json.astronomy;
//foreach (var astro in json.astronomy)
//{/}
//                foreach (var m in sunMapping)
//                    {
//                        if (m.Name.Contains("Hour"))
//                        {
//                            timesOfTheDayList.Add(AddHourEvent(current, m.Name, m.AstroTime, m.OtherTime, astro));
//                        }
//                        else
//                        {
//                            timesOfTheDayList.Add(AddEvent(current, m.Name, m.AstroTime, astro));
//                        }
//                    }


//                    foreach (var m in eveningMappings)
//{
//    if (m.Name.Contains("Hour"))
//    {
//        timesOfTheDayList.Add(AddHourEvent(current, m.Name, m.AstroTime, m.OtherTime, astro));
//    }
//    else
//    {
//        timesOfTheDayList.Add(AddEvent(current, m.Name, m.AstroTime, astro));
//    }
//}

//foreach (var m in topLevel)
//{
//    timesOfTheDayList.Add(AddEvent(current, m.Name, m.AstroTime, astro));
//}                    // End of the daily mapped events foreach loops } 

//                    // End of the astro for loop

//                }// End of if json is valid
//            var eventTimes = new EventTime[timesOfTheDayList.Count];
//int t = 0;
//foreach (var item in timesOfTheDayList)
//{
//    if (item.AstroTime != DateTime.MinValue)
//    {
//        eventTimes[t] = item;
//        t++;
//    }
//}

//return (elevation, astronomicalData, eventTimes);
//            }
//        }
//            catch (Exception ex)
//            {
//    // Handle exceptions (e.g., log the error, show a message to the user, etc.)
//    Debug.WriteLine($"Error fetching event times: {ex.Message}");
//}

//return (0.0, new DailyAstronomicalData(), Array.Empty<EventTime>());
//        }
//        }

//        private static EventTime AddEvent(DateTime date, string eventName, Func<TimeData.Astronomy, string> Selector, TimeData.Astronomy astroItem)
//{
//    EventTime newEvent = new EventTime
//    {
//        EventName = eventName,
//        Date = date
//    };
//    string sStr = Selector.Invoke(astroItem);
//    try
//    {
//        var time = TimeOnly.Parse(sStr);
//        DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
//        newEvent.AstroTime = dateTime;
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"{eventName} doesn't have a valid time {sStr}");
//        newEvent.AstroTime = DateTime.MinValue;
//    }
//    return newEvent;
//}
//private static EventTime AddHourEvent(DateTime date, string eventName, Func<TimeData.Astronomy, string> startSelector, Func<TimeData.Astronomy, string> endSelector, TimeData.Astronomy astroItem)
//{
//    EventTime newEvent = new EventTime
//    {
//        EventName = eventName,
//        Date = date

//    };
//    string sStr = startSelector.Invoke(astroItem);
//    var time = TimeOnly.Parse(sStr);
//    DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
//    newEvent.AstroTime = dateTime;

//    string eStr = endSelector.Invoke(astroItem);
//    var endTime = TimeOnly.Parse(eStr);
//    DateTime endDateTime = new DateTime(date.Year, date.Month, date.Day, endTime.Hour, endTime.Minute, endTime.Second);
//    newEvent.AstroTime = dateTime;

//    return newEvent;