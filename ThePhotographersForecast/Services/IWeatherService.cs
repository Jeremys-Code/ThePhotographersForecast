using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IWeatherService
    {
        Task InitializeAsync(IHttpClientFactory factory, ImageCacheService cacheService);
        Task<GfsWeatherModel> GetWeatherAsync(double latitude, double longitude);
    }
}
