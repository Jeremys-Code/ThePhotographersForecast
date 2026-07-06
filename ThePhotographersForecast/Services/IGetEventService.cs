using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetEventService
    {
        Task GetPhotoDayEventsAsync();
        EventModel GetSelectedDayEventModel(DateOnly date);
    }
}