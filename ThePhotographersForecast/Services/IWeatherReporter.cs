using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IWeatherReporter
    {
        MeteorologisticAgent BuildDailyBins(WeatherForecast forecast);
        string GetDailyHealthInfo(DailyForecast day);
        string GetDailyPrecipitation(DailyForecast day);
        (string weather, string health, string precipitation) GetDailyWeatherSummary(WeatherForecast forecast);
        string GetHourlyHealthInfo(HourlyForecast hour);
        string GetHourlyPrecipitation(HourlyForecast hour);
        (string weather, string health, string precipitation) GetHourlyWeatherSummary(HourlyForecast hourlyForecast);
        void Init(GetWeatherService weatherService);
    }
}