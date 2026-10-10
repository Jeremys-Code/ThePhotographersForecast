using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetEventService
    {
        public Task<EventModel[]> GetDayEventsAsync();
        public Task<EventModel> GetSelectedDayEventModelAsync(DateTime date);
    }
}
