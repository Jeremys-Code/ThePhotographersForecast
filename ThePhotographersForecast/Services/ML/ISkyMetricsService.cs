using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.ML
{
    public interface ISkyMetricsService
    {
        Task<SkyHourData[]> GetSkyModelDataAsync();
    }
}