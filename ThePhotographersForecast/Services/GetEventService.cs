using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services;

public partial class GetEventService : IGetEventService
{
    private ISession Session;
    private HttpClient _client;
    public GetEventService(ISession session, IHttpClientFactory client)
    {
        Session = session;
        _client = client.CreateClient("astroClient");
    }
    private async Task<T?> GetDataAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        // Make retries for transient socket/IO/HTTP issues. Only safe to retry when request has no content (GET/HEAD).
        const int maxAttempts = 3;
        var attempt = 0;
        var delay = TimeSpan.FromSeconds(1);
        var canRetry = request.Content == null && (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head);

        HttpRequestMessage CloneHeaders(HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);
            foreach (var header in original.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (original.Version != null)
                clone.Version = original.Version;
            return clone;
        }

        while (true)
        {
            attempt++;
            // Clone headers into a fresh HttpRequestMessage because instances are generally not safe to reuse.
            using var reqToSend = CloneHeaders(request);
            try
            {
                var response = await _client.SendAsync(reqToSend, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"ERROR: {_client.BaseAddress} FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    return default;
                }

                Debug.WriteLine($"SUCCESSFULLY RECEIVED JSON FROM {_client.BaseAddress}: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken).ConfigureAwait(false);
                return result;

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Caller requested cancellation - propagate as completed without result
                Debug.WriteLine("GetDataAsync cancelled by caller.");
                return default;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is System.IO.IOException || ex is ObjectDisposedException || ex is System.Net.Sockets.SocketException)
            {
                Debug.WriteLine($"Transient error on attempt {attempt}: {ex.GetType().Name} - {ex.Message}");
                if (!canRetry || attempt >= maxAttempts || cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("Giving up after transient errors.");
                    return default;
                }

                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return default;
                }

                // exponential backoff
                delay = TimeSpan.FromSeconds(Math.Min(30, delay.TotalSeconds * 2));
                // retry loop
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Non-transient error fetching NWS data: {ex}");
                return default;
            }
        }
    }
    public async Task<EventModel[]> GetDayEventsAsync()
    {

        var json = new EventTimeSeriesData.Rootobject();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.ipgeolocation.io/v3/astronomy/timeSeries?apiKey=b3c99c6a0b9e44169fa96b6918495732&dateStart={Session.StartDate.ToString("yyyy-MM-dd")}&dateEnd={Session.EndDate.ToString("yyyy-MM-dd")}&lat={Session.SelectedLocation.Latitude}&long={Session.SelectedLocation.Longitude}");
            json = await GetDataAsync<EventTimeSeriesData.Rootobject>(request);


            if (json == null) return default;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get Events Service Error {ex}");
        }

        var eventData = new List<EventModel>();
        //Create new Event Model for the Day


        try
        {
            // Populating more SelectedLocation properties
            await GetSelectedLocationInformationAsync(json.location);
            //Task Check    
            //
            Debug.WriteLine($"Did the city update: {Session.SelectedLocation.City}");

            for (int i = 0; i < json.astronomy.Length; i++)
            {

                var eventsList = new List<EventTime>();
                var astro = json.astronomy[i];
                var eventModel = new EventModel();
                var date = json.astronomy[i].date;
                eventModel.Date = date;
                TextInfo phase = CultureInfo.CurrentCulture.TextInfo;
                //Extract the Astronomical data from the json
                eventModel.AstronomicalData = new Astronomical
                {
                    Date = date,
                    DayLength = json.astronomy[i].day_length,
                    MoonPhase = phase.ToTitleCase(json.astronomy[i].moon_phase.Replace("_", " ").ToLowerInvariant()),
                    MoonImage = ImageSource.FromFile(json.astronomy[i].moon_phase.Replace("_", "").ToLowerInvariant() + ".png"),
                    SkMoon = MeteoconsResolver.ResolveMoons(json.astronomy[i].moon_phase)
                };



                EventMapping[] morningMappings =
                    {
                            new ("Astronomical Twilight Begins", a => a.morning.astronomical_twilight_begin),
                            new ("Nautical Twilight Begins", a => a.morning.nautical_twilight_begin),
                            new ("Civil Twilight Begins", a => a.morning.civil_twilight_begin)
                        };
                HourEventMapping[] hourMorningMappings =
                {
                    new ("Blue Hour", a => a.morning.blue_hour_begin, a => a.morning.blue_hour_end),
                            new ("Golden Hour", a => a.morning.golden_hour_begin, a => a.morning.golden_hour_end)
                };
                EventMapping[] eveningMappings =
                    {
                            new ("Civil Twilight Ends", a => a.evening.civil_twilight_end),
                            new ("Nautical Twilight Ends", a => a.evening.nautical_twilight_end),
                            new ("Astronomical Twilight Ends",a => a.evening.astronomical_twilight_end)
                    };
                HourEventMapping[] hourEveningMappings =
                {
                            new ("Golden Hour", a => a.evening.golden_hour_begin, a => a.evening.golden_hour_end),
                            new ("Blue Hour", a => a.evening.blue_hour_begin, a => a.evening.blue_hour_end)
                };
                EventMapping[] topLevel =
        {
                            new ("Sunrise", a => a.sunrise),
                            new ("Sunset", a => a.sunset),
                            new ("Moonrise", a => a.moonrise ?? DateTime.MinValue.ToString()),
                            new ("Moonset", a => a.moonset ?? DateTime.MinValue.ToString()),
                            new ("Night Ends", a => a.night_end),
                            new ("Night Begins", a => a.night_begin),
                            new ("Solar Noon", a => a.solar_noon),
                            new ("Midnight", a => a.mid_night),
                        };


                foreach (var m in morningMappings)
                {
                    eventsList.Add(AddEvent(date, m.Name, m.AstroTime, astro));
                }
                foreach (var m in hourMorningMappings)
                {
                    eventsList.Add(AddHourEvent(date, m.Name, m.AstroTime, m.OtherTime, astro));
                }
                foreach (var m in eveningMappings)
                {
                    eventsList.Add(AddEvent(date, m.Name, m.AstroTime, astro));
                }
                foreach (var m in hourEveningMappings)
                {
                    eventsList.Add(AddHourEvent(date, m.Name, m.AstroTime, m.OtherTime, astro));
                }

                foreach (var m in topLevel)
                {
                    eventsList.Add(AddEvent(date, m.Name, m.AstroTime, astro));
                }                    // End of the daily mapped events foreach loops } 


                eventModel.EventTimes = eventsList.Where(x => x.AstroTime != DateTime.MinValue).OrderBy(o => o.OrderTime).ToArray();


                Debug.WriteLine($"From EventService eventTimes add {eventModel.EventTimes.Length}");



                eventData.Add(eventModel);
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        var eventsArray = eventData.OrderBy(o => o.Date).ToArray();
        return eventsArray;

    }

    private async Task GetSelectedLocationInformationAsync(EventTimeSeriesData.Location json)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Session.SelectedLocation.Country = json.country_name;
            if (json.locality != null)
            {
                Session.SelectedLocation.City = json.locality;
            }
            if (Session.SelectedLocation.State == null) Session.SelectedLocation.State = json.state_prov;
        });
        return;
    }
    private EventTime AddEvent(DateOnly date, string eventName, Func<EventTimeSeriesData.Astronomy, string> Selector, EventTimeSeriesData.Astronomy astroItem)
    {
        EventTime newEvent = new EventTime
        {
            EventName = eventName,
            Date = date
        };
        string sStr = Selector.Invoke(astroItem);
        try
        {
            TimeOnly t;
            var time = TimeOnly.TryParse(sStr, out t);
            if (time)
            {
                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, t.Hour, t.Minute, t.Second);
                newEvent.AstroTime = dateTime;
            }
            else
            {
                newEvent.AstroTime = DateTime.MinValue;
            }
        }
        catch (Exception ex)
        {
            _ = ex;
            newEvent.AstroTime = DateTime.MinValue;
        }
        return newEvent;
    }

    private EventTime AddHourEvent(DateOnly date, string eventName, Func<EventTimeSeriesData.Astronomy, string> startSelector, Func<EventTimeSeriesData.Astronomy, string> endSelector, EventTimeSeriesData.Astronomy astroItem)
    {
        EventTime newEvent = new EventTime
        {
            EventName = eventName,
            Date = date

        };
        string sStr = startSelector.Invoke(astroItem);
        TimeOnly t;
        var time = TimeOnly.TryParse(sStr, out t);
        if (time)
        {
            DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, t.Hour, t.Minute, t.Second);
            newEvent.AstroTime = dateTime;
        }
        else
        {
            newEvent.AstroTime = DateTime.MinValue;
        }

        string eStr = endSelector.Invoke(astroItem);
        TimeOnly e;
        var endTime = TimeOnly.TryParse(eStr, out e);
        if (endTime)
        {
            DateTime endDateTime = new DateTime(date.Year, date.Month, date.Day, e.Hour, e.Minute, e.Second);
            newEvent.OtherTime = endDateTime;
        }
        else
        {
            newEvent.OtherTime = DateTime.MinValue;
        }
        return newEvent;
    }

    public async Task<EventModel> GetSelectedDayEventModelAsync(DateTime date)
    {
        var selectedEventModel = Session.EventsData.Where(d => d.Date == DateOnly.FromDateTime(date)).FirstOrDefault<EventModel>();
        if (selectedEventModel != null)
        {
            return selectedEventModel;
        }
        else
        {
            var result = await GetDayEventsAsync();
        }
        var model = Session.EventsData.Where(d => d.Date == DateOnly.FromDateTime(date)).FirstOrDefault<EventModel>();
        return model;
    }
}

public record EventMapping(
string Name,
Func<EventTimeSeriesData.Astronomy, string> AstroTime
);

public record HourEventMapping(
string Name,
Func<EventTimeSeriesData.Astronomy, string> AstroTime,
Func<EventTimeSeriesData.Astronomy, string> OtherTime
);
