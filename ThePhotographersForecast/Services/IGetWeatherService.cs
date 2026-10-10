using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetWeatherService
    {
        Task<WeatherForecast> BuildWeatherForecastsAsync();
        string WindDirection(int windDirection);
    }
}