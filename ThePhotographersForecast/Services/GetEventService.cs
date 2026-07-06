using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services;

public partial class GetEventService : ObservableObject, IGetEventService
{
    private HttpClient _client;
    private Session _session;

    public GetEventService(Session session, IHttpClientFactory client)
    {
        _session = session;
        _client = client.CreateClient("photographer");
    }

    public async Task GetPhotoDayEventsAsync()
    {
        _session.EventsData = new ObservableCollection<EventModel>();

        for (int i = 0; i < _session.Dates.Length; i++)
        {

            string apiEventString = $@"https://api.ipgeolocation.io/v3/astronomy?apiKey=b3c99c6a0b9e44169fa96b6918495732&lat={_session.SelectedLocation.Latitude}&long={_session.SelectedLocation.Longitude}&{_session.Dates[i].Date.ToString("yyyy-MM-dd")}";
            var json = new TimeData.Rootobject();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiEventString);
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadFromJsonAsync<TimeData.Rootobject>() ?? new TimeData.Rootobject();
                }
                //Create new Event Model for the Day
                var newEvents = new EventModel();
                var eventsList = new List<EventTime>();
                newEvents.Date = DateOnly.FromDateTime(_session.Dates[i].Date);
                if (_session.SelectedLocation.City == string.Empty)
                    _session.SelectedLocation.City = json.location.city;
                if (_session.SelectedLocation.State == string.Empty)
                    _session.SelectedLocation.State = json.location.state_prov;
                if (_session.SelectedLocation.Country != json.location.country_name)
                    _session.SelectedLocation.Country = json.location.country_name;
                //Extract the Astronomical data from the json
                newEvents.AstronomicalData = new DailyAstronomicalData
                {
                    Date = newEvents.Date,
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
                var astro = json.astronomy;

                foreach (var m in morningMappings)
                {
                    if (m.Name.Contains("Hour"))
                    {
                        eventsList.Add(AddHourEvent(newEvents.Date, m.Name, m.AstroTime, m.OtherTime, astro));
                    }
                    else
                    {
                        eventsList.Add(AddEvent(newEvents.Date, m.Name, m.AstroTime, astro));
                    }
                }


                foreach (var m in eveningMappings)
                {
                    if (m.Name.Contains("Hour"))
                    {
                        eventsList.Add(AddHourEvent(newEvents.Date, m.Name, m.AstroTime, m.OtherTime, astro));
                    }
                    else
                    {
                        eventsList.Add(AddEvent(newEvents.Date, m.Name, m.AstroTime, astro));
                    }
                }

                foreach (var m in topLevel)
                {
                    eventsList.Add(AddEvent(newEvents.Date, m.Name, m.AstroTime, astro));
                }                    // End of the daily mapped events foreach loops } 
                var goodEvents = new List<EventTime>();
                foreach (EventTime e in eventsList)
                {
                    if (e.AstroTime != DateTime.MinValue)
                    {
                        goodEvents.Add(e);
                    }
                }

                Debug.WriteLine($"INFO: Events before removing MinValue: {eventsList.Count}");
                eventsList.RemoveAll(m => m.AstroTime == DateTime.MinValue);
                Debug.WriteLine($"INFO: Events after removing MinValue: {eventsList.Count}");
                _session.EventTimes = new ObservableCollection<EventTime>(eventsList.OrderBy(o => o.OrderTime));
                newEvents.EventTimes = new ObservableCollection<EventTime>(eventsList.OrderBy(o => o.OrderTime));

                _session.EventsData.Add(newEvents);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }//for day loop finished

        return;
    }


    private static EventTime AddEvent(DateOnly date, string eventName, Func<TimeData.Astronomy, string> Selector, TimeData.Astronomy astroItem)
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
            _ = ex;
            newEvent.AstroTime = DateTime.MinValue;
        }
        return newEvent;
    }

    private static EventTime AddHourEvent(DateOnly date, string eventName, Func<TimeData.Astronomy, string> startSelector, Func<TimeData.Astronomy, string> endSelector, TimeData.Astronomy astroItem)
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

    public EventModel GetSelectedDayEventModel(DateOnly date)
    {
        var selectedEventModel = _session.EventsData.Where(d => d.Date == date).FirstOrDefault<EventModel>();
        if (selectedEventModel != null)
        {
            return selectedEventModel;
        }
        return new EventModel();
    }


}

public record EventMapping(
string Name,
Func<TimeData.Astronomy, string> AstroTime,
Func<TimeData.Astronomy, string?> OtherTime
);
