using SunCalcSharp;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    public partial class GetEphemerisService : IGetEphemerisService
    {
        public Session SharedSession { get; set; }
        public GetEphemerisService(Session sharedSession)
        {
            SharedSession = sharedSession;
        }

        public async Task<EventModel[]> GetDailyAstronomicalEventsAsync(double latitude, double longitude, NwsWeatherModel nws, int selectedTimeZoneOffset)
        {
            var eventModels = new List<EventModel>();
            if (SharedSession.Dates == null || SharedSession.Dates.Length == 0)
            {
                throw new ApplicationException("There are no photo shoot dates selected. At least one date needs to be selected.");
                return null;
            }
            //Creates the EventModel for each Photo Day
            for (int e = 0; e < SharedSession.Dates.Length; e++)
            {
                var date = SharedSession.Dates[e].Date.AddSeconds(SharedSession.SelectedTimeZoneOffset * -1);
                var eventList = new List<EventTime>();
                var dailyEvents = SunCalc.GetTimes(date, latitude, longitude);
                var moonEvents = MoonCalc.GetMoonTimes(date, latitude, longitude);
                var moonIllumination = MoonCalc.GetMoonIllumination(date);
                if (dailyEvents == null || moonEvents == null || moonIllumination == null) return null;
                var astronomicalTwilightBegins = dailyEvents.AstronomicalTwilightBegins.AddSeconds(selectedTimeZoneOffset);
                var nauticalTwilightBegins = dailyEvents.NauticalTwilightBegins.AddSeconds(selectedTimeZoneOffset);
                var blueHourBegins = dailyEvents.BlueHourBegins.AddSeconds(selectedTimeZoneOffset);
                var civilTwilightBegins = dailyEvents.CivilTwilightBegins.AddSeconds(selectedTimeZoneOffset);
                var goldenHourBegins = dailyEvents.GoldenHourBegins.AddSeconds(selectedTimeZoneOffset);
                var sunrise = dailyEvents.Sunrise.AddSeconds(selectedTimeZoneOffset);
                var sunriseEnds = dailyEvents.SunriseEnd.AddSeconds(selectedTimeZoneOffset);
                var solarNoon = dailyEvents.SolarNoon.AddSeconds(selectedTimeZoneOffset);
                var sunsetBegins = dailyEvents.SunsetBegins.AddSeconds(selectedTimeZoneOffset);
                var sunset = dailyEvents.Sunset.AddSeconds(selectedTimeZoneOffset);
                var goldenHourEnds = dailyEvents.GoldenHourEnds.AddSeconds(selectedTimeZoneOffset);
                var civilTwilightEnds = dailyEvents.CivilTwilightEnds.AddSeconds(selectedTimeZoneOffset);
                var blueHourEnds = dailyEvents.BlueHourEnds.AddSeconds(selectedTimeZoneOffset);
                var nauticalTwilightEnds = dailyEvents.NauticalTwilightEnds.AddSeconds(selectedTimeZoneOffset);
                var astronomicalTwilightEnds = dailyEvents.AstronomicalTwilightEnds.AddSeconds(selectedTimeZoneOffset);
                var midnight = dailyEvents.Midnight.AddSeconds(selectedTimeZoneOffset);
                var moonrise = moonEvents.Rise.GetValueOrDefault();
                var moonset = moonEvents.Set.GetValueOrDefault();
                var moonPhase = moonIllumination.Phase switch
                {
                    MoonPhase.NewMoon => "New Moon",
                    MoonPhase.WaxingCrescent => "Waxing Crescent",
                    MoonPhase.FirstQuarter => "First Quarter",
                    MoonPhase.WaxingGibbous => "Waxing Gibbous",
                    MoonPhase.FullMoon => "Full Moon",
                    MoonPhase.WaningGibbous => "Waning Gibbous",
                    MoonPhase.LastQuarter => "Last Quarter",
                    MoonPhase.WaningCrescent => "Waning Crescent",
                    _ => ""
                };


                //Turn the sun and moon ephemeris into the events list
                var astroBegins = new EventTime
                {
                    EventName = "Astronomical Twilight Begins",
                    AstroTime = astronomicalTwilightBegins,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(astronomicalTwilightBegins)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(astronomicalTwilightBegins)
                };
                eventList.Add(astroBegins);
                var nauticalBegins = new EventTime
                {
                    EventName = "Nautical Twilight Begins",
                    AstroTime = nauticalTwilightBegins,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(nauticalTwilightBegins)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(nauticalTwilightBegins)
                };
                eventList.Add(nauticalBegins);
                var blueHourStart = new EventTime
                {
                    EventName = "Blue Hour Begins",
                    AstroTime = blueHourBegins,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(blueHourBegins)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(blueHourBegins)
                };
                eventList.Add(blueHourStart);
                var civilBegins = new EventTime
                {
                    EventName = "Civil Twilight Begins",
                    AstroTime = civilTwilightBegins,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(civilTwilightBegins)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(civilTwilightBegins)
                };
                eventList.Add(civilBegins);
                var goldenHourStart = new EventTime
                {
                    EventName = "Golden Hour Begins",
                    AstroTime = goldenHourBegins,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(goldenHourBegins)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(goldenHourBegins)
                };
                eventList.Add(goldenHourStart);
                var sunriseEvent = new EventTime
                {
                    EventName = "Sunrise",
                    AstroTime = sunrise,
                    OtherTime = sunriseEnds,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(sunrise)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(sunrise)
                };
                eventList.Add(sunriseEvent);
                var solarNoonEvent = new EventTime
                {
                    EventName = "Solar Noon",
                    AstroTime = solarNoon,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(solarNoon)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(solarNoon)
                };
                eventList.Add(solarNoonEvent);
                var sunsetEvent = new EventTime
                {
                    EventName = "Sunset",
                    AstroTime = sunsetBegins,
                    OtherTime = sunset,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(sunset)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(sunset)
                };
                eventList.Add(sunsetEvent);
                var goldenHourEndsEvent = new EventTime
                {
                    EventName = "Golden Hour Ends",
                    AstroTime = goldenHourEnds,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(goldenHourEnds)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(goldenHourEnds)
                };
                eventList.Add(goldenHourEndsEvent);
                var civilTwilightEndsEvent = new EventTime
                {
                    EventName = "Civil Twilight Ends",
                    AstroTime = civilTwilightEnds,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(civilTwilightEnds)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(civilTwilightEnds)
                };
                eventList.Add(civilTwilightEndsEvent);
                var blueHourEndsEvent = new EventTime
                {
                    EventName = "Blue Hour Ends",
                    AstroTime = blueHourEnds,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(blueHourEnds)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(blueHourEnds)
                };
                eventList.Add(blueHourEndsEvent);
                var nauticalTwilightEndsEvent = new EventTime
                {
                    EventName = "Nautical Twilight Ends",
                    AstroTime = nauticalTwilightEnds,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(nauticalTwilightEnds)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(nauticalTwilightEnds)
                };
                eventList.Add(nauticalTwilightEndsEvent);
                var astronomicalTwilightEndsEvent = new EventTime
                {
                    EventName = "Astronomical Twilight Ends",
                    AstroTime = astronomicalTwilightEnds,
                    //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(astronomicalTwilightEnds)).FirstOrDefault() ?? new NwsHourlyForecast(),
                    WeatherHour = GetWeatherHour(astronomicalTwilightEnds)
                };
                eventList.Add(astronomicalTwilightEndsEvent);
                if (!moonEvents.AlwaysDown)
                {
                    var moonriseEvent = new EventTime
                    {
                        EventName = "Moonrise",
                        AstroTime = moonrise,
                        //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(moonrise)).FirstOrDefault() ?? new NwsHourlyForecast(),
                        WeatherHour = GetWeatherHour(moonrise)
                    };
                    eventList.Add(moonriseEvent);
                }
                if (!moonEvents.AlwaysUp)
                {
                    var moonsetEvent = new EventTime
                    {
                        EventName = "Moonset",
                        AstroTime = moonset,
                        //Forecast = nws.HourlyForecasts.Where(h => h.StartTime == GetWeatherHour(moonset)).FirstOrDefault() ?? new NwsHourlyForecast(),
                        WeatherHour = GetWeatherHour(moonset)
                    };
                    eventList.Add(moonsetEvent);
                }


                var eventData = new EventModel
                {
                    Date = DateOnly.FromDateTime(date),
                    AstronomicalData = new Astronomical
                    {
                        Date = DateOnly.FromDateTime(date),
                        DayLength = astronomicalTwilightEnds.Subtract(astronomicalTwilightBegins),
                        MoonPhase = moonPhase,
                        SkMoon = MeteoconsResolver.ResolveMoons(moonPhase)
                    },
                    EventTimes = eventList.OrderBy(e => e.AstroTime).ToArray()
                };

                eventModels.Add(eventData);

            }//Finished processing each photo day

            //Return the eventModels
            return eventModels.OrderBy(o => o.Date).ToArray();
        }

        private DateTime GetWeatherHour(DateTime astroTime)
        {

            var dateTime = new DateTime();
            if (astroTime.Minute > 45)
            {
                if (astroTime.Hour != 23)
                {

                    dateTime = new DateTime(astroTime.Year, astroTime.Month, astroTime.Day, astroTime.AddHours(1).Hour, 0, 0);
                }
                else
                {
                    dateTime = new DateTime(astroTime.Year, astroTime.Month, astroTime.AddDays(1).Day, astroTime.AddHours(1).Hour, 0, 0);
                }
            }
            else
            {
                dateTime = new DateTime(astroTime.Year, astroTime.Month, astroTime.Day, astroTime.Hour, 0, 0);
            }
            return dateTime;
        }

        private EventTime BuildEventTime(DateTime eventTime, string eventName)
        {
            var eventTimeModel = new EventTime
            {
                EventName = eventName,
                AstroTime = eventTime,
                WeatherHour = GetWeatherHour(eventTime)
            };

            return eventTimeModel;
        }
    }
}
