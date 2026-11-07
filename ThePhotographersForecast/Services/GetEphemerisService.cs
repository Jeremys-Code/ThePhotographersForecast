using SunCalcSharp;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    public partial class GetEphemerisService : IGetEphemerisService
    {
        private ISession _session;
        public GetEphemerisService(ISession session)
        {
            _session = session;
        }

        public async Task<EventModel[]> GetDailyAstronomicalEventsAsync(double latitude, double longitude, NwsWeatherModel nws)
        {
            var masterEvents = new List<EventTime>();
            var eventModels = new List<EventModel>();
            int selectedTimeZoneOffset = nws.TimezoneOffsetSeconds;
            if (_session.Dates == null || _session.Dates.Length == 0)
            {
                throw new ApplicationException("There are no photo shoot dates selected. At least one date needs to be selected.");
                return null;
            }
            //Creates the EventModel for each Photo Day
            for (int e = 0; e < _session.Dates.Length; e++)
            {

                var date = _session.Dates[e].Date.AddSeconds(_session.SelectedTimeZoneOffset * -1);
                var eventList = new List<EventTime>();
                var dailyEvents = SunCalc.GetTimes(date, latitude, longitude);
                var moonEvents = MoonCalc.GetMoonTimes(date, latitude, longitude);
                var moonIllumination = MoonCalc.GetMoonIllumination(date);
                var sunPosition = SunCalc.GetPosition(date, latitude, longitude);
                if (dailyEvents == null || moonEvents == null || moonIllumination == null) return null;
                var astronomicalTwilightBegins = dailyEvents.AstronomicalTwilightBegins.AddSeconds(selectedTimeZoneOffset);
                var nauticalTwilightBegins = dailyEvents.NauticalTwilightBegins.AddSeconds(selectedTimeZoneOffset);
                var morningBlueHourBegins = dailyEvents.MorningBlueHourBegins.AddSeconds(selectedTimeZoneOffset);
                var morningBlueHourEnds = dailyEvents.MorningBlueHourEnds.AddSeconds(selectedTimeZoneOffset);
                var civilTwilightBegins = dailyEvents.CivilTwilightBegins.AddSeconds(selectedTimeZoneOffset);
                var morningGoldenHourBegins = dailyEvents.MorningGoldenHourBegins.AddSeconds(selectedTimeZoneOffset);
                var morningGoldenHourEnds = dailyEvents.MorningGoldenHourEnds.AddSeconds(selectedTimeZoneOffset);
                var sunrise = dailyEvents.Sunrise.AddSeconds(selectedTimeZoneOffset);
                var sunriseEnds = dailyEvents.SunriseEnd.AddSeconds(selectedTimeZoneOffset);
                var solarNoon = dailyEvents.SolarNoon.AddSeconds(selectedTimeZoneOffset);
                var sunsetBegins = dailyEvents.SunsetBegins.AddSeconds(selectedTimeZoneOffset);
                var sunset = dailyEvents.Sunset.AddSeconds(selectedTimeZoneOffset);
                var eveningGoldenHourBegins = dailyEvents.EveningGoldenHourBegins.AddSeconds(selectedTimeZoneOffset);
                var eveningGoldenHourEnds = dailyEvents.EveningGoldenHourEnds.AddSeconds(selectedTimeZoneOffset);
                var eveningBlueHourBegins = dailyEvents.EveningBlueHourBegins.AddSeconds(selectedTimeZoneOffset);
                var civilTwilightEnds = dailyEvents.CivilTwilightEnds.AddSeconds(selectedTimeZoneOffset);
                var eveningBlueHourEnds = dailyEvents.EveningBlueHourEnds.AddSeconds(selectedTimeZoneOffset);
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
                    WeatherHour = GetWeatherHour(astronomicalTwilightBegins)
                };
                eventList.Add(astroBegins);
                var nauticalBegins = new EventTime
                {
                    EventName = "Nautical Twilight Begins",
                    AstroTime = nauticalTwilightBegins,
                    WeatherHour = GetWeatherHour(nauticalTwilightBegins)
                };
                eventList.Add(nauticalBegins);
                var morningBlueHour = new EventTime
                {
                    EventName = "Morning Blue Hour",
                    AstroTime = morningBlueHourBegins,
                    OtherTime = morningBlueHourEnds,
                    WeatherHour = GetWeatherHour(morningBlueHourBegins)
                };
                eventList.Add(morningBlueHour);
                var civilBegins = new EventTime
                {
                    EventName = "Civil Twilight Begins",
                    AstroTime = civilTwilightBegins,
                    WeatherHour = GetWeatherHour(civilTwilightBegins)
                };
                eventList.Add(civilBegins);
                var morningGoldenHour = new EventTime
                {
                    EventName = "Morning Golden Hour",
                    AstroTime = morningGoldenHourBegins,
                    OtherTime = morningGoldenHourEnds,
                    WeatherHour = GetWeatherHour(morningGoldenHourBegins)
                };
                eventList.Add(morningGoldenHour);
                var sunriseEvent = new EventTime
                {
                    EventName = "Sunrise",
                    AstroTime = sunrise,
                    OtherTime = sunriseEnds,
                    WeatherHour = GetWeatherHour(sunrise)
                };
                eventList.Add(sunriseEvent);
                var solarNoonEvent = new EventTime
                {
                    EventName = "Solar Noon",
                    AstroTime = solarNoon,
                    WeatherHour = GetWeatherHour(solarNoon)
                };
                eventList.Add(solarNoonEvent);
                var sunsetEvent = new EventTime
                {
                    EventName = "Sunset",
                    AstroTime = sunsetBegins,
                    OtherTime = sunset,
                    WeatherHour = GetWeatherHour(sunset)
                };
                eventList.Add(sunsetEvent);
                var eveningGoldenHour = new EventTime
                {
                    EventName = "Eveing Golden Hour",
                    AstroTime = eveningGoldenHourBegins,
                    OtherTime = eveningGoldenHourEnds,
                    WeatherHour = GetWeatherHour(eveningGoldenHourBegins)
                };
                eventList.Add(eveningGoldenHour);
                var civilTwilightEndsEvent = new EventTime
                {
                    EventName = "Civil Twilight Ends",
                    AstroTime = civilTwilightEnds,
                    WeatherHour = GetWeatherHour(civilTwilightEnds)
                };
                eventList.Add(civilTwilightEndsEvent);
                var eveningBlueHour = new EventTime
                {
                    EventName = "Evening Blue Hour",
                    AstroTime = eveningBlueHourBegins,
                    OtherTime = eveningBlueHourEnds,
                    WeatherHour = GetWeatherHour(eveningBlueHourBegins)
                };
                eventList.Add(eveningBlueHour);
                var nauticalTwilightEndsEvent = new EventTime
                {
                    EventName = "Nautical Twilight Ends",
                    AstroTime = nauticalTwilightEnds,
                    WeatherHour = GetWeatherHour(nauticalTwilightEnds)
                };
                eventList.Add(nauticalTwilightEndsEvent);
                var astronomicalTwilightEndsEvent = new EventTime
                {
                    EventName = "Astronomical Twilight Ends",
                    AstroTime = astronomicalTwilightEnds,
                    WeatherHour = GetWeatherHour(astronomicalTwilightEnds)
                };
                eventList.Add(astronomicalTwilightEndsEvent);
                if (!moonEvents.AlwaysDown)
                {
                    var moonriseEvent = new EventTime
                    {
                        EventName = "Moonrise",
                        AstroTime = moonrise,
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
                        WeatherHour = GetWeatherHour(moonset)
                    };
                    eventList.Add(moonsetEvent);
                }
                masterEvents.AddRange(eventList);


                var eventData = new EventModel
                {
                    Date = date,
                    AstronomicalData = new Astronomical
                    {
                        DayLength = astronomicalTwilightEnds.Subtract(astronomicalTwilightBegins),
                        MoonPhase = moonPhase,
                        SkMoon = MeteoconsResolver.ResolveMoons(moonPhase),
                        SunAltitude = sunPosition.Altitude,
                        SunAzimuth = sunPosition.Azimuth
                    },
                    EventTimes = eventList.OrderBy(e => e.AstroTime).ToArray()
                };


                eventModels.Add(eventData);

            }//Finished processing each photo day
            var c = new List<EventTime>();
            foreach (var a in masterEvents) { if (a.AstroTime != DateTime.MinValue) { c.Add(a); } }

            _session.EventTimes = c.OrderBy(v => v.AstroTime).ToArray();

            var finishedEventModels = new ObservableCollection<EventModel>();
            foreach (var model in eventModels)
            {
                var clone = new EventModel();
                clone.Date = model.Date;
                clone.AstronomicalData = new Astronomical(model.AstronomicalData);
                clone.EventTimes = new EventTime[model.EventTimes.Length];
                for (int time = 0; time < model.EventTimes.Length; time++)
                {
                    clone.EventTimes[time] = new EventTime
                    {
                        AstroTime = model.EventTimes[time].AstroTime,
                        OtherTime = model.EventTimes[time].OtherTime,
                        EventName = model.EventTimes[time].EventName,
                        WeatherHour = model.EventTimes[time].WeatherHour,
                        Forecast = nws.HourlyForecasts.Where(h => h.StartTime == model.EventTimes[time].WeatherHour).FirstOrDefault()
                    };
                }
                ;
                finishedEventModels.Add(clone);

            }
            ;

            //Return the eventModels
            return finishedEventModels.OrderBy(o => o.Date).ToArray();
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
