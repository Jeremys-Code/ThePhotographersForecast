using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IWeatherReporter
    {
        (string healthSummary, Color uvIndexColor) GetDailyHealthInfo(DailyForecast day);
        string GetDailyPrecipitation(DailyForecast day);
        ForecastSummaries GetDailyWeatherSummary(DailyForecast dailyForecast, HourlyForecast[] hourlyForecasts);
        (string healthSummary, Color uvIndexColor) GetHourlyHealthInfo(HourlyForecast hour);
        string GetHourlyPrecipitation(HourlyForecast hour);
        ForecastSummaries GetHourlyWeatherSummary(HourlyForecast hourlyForecast);
    }
}