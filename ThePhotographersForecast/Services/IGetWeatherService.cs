using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetWeatherService
    {
        Task<ObservableCollection<WeatherForecast>> BuildWeatherForecastsAsync(PhotoLocation selectedLocation, DateTime startDate, DateTime endDate);
        string FromTimesOfDay(int hour);
        string WindDirection(int windDirection);
    }
}