using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetEphemerisService
    {
        Task<EventModel[]> GetDailyAstronomicalEventsAsync(double latitude, double longitude, NwsWeatherModel nws);
    }
}