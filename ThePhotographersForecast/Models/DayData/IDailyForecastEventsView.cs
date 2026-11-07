namespace ThePhotographersForecast.Models.DayData
{
    public interface IDailyForecastEventsView
    {
        Task<MappedHourly[]> BuildMappedHourlyAsync();
    }
}