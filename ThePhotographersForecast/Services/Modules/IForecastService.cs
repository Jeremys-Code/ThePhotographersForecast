using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Modules
{
    public interface IForecastService
    {
        int TCount { get; set; }
        CalendarDayModel[] Dates { get; set; }
        HourlyForecast[] gfsHourlies { get; set; }
        ObservableCollection<ForecastInputs> Inputs { get; set; }
        NwsHourlyForecast[] NwsHourlies { get; set; }
        DateTime SelectedDate { get; set; }
        DateTime SelectedTime { get; set; }
        ObservableCollection<NwsTimeOfDayForecast> TimeOfDayForecasts { get; set; }

        Task<ObservableCollection<string>> BuildDailyForecastAsync(NwsTimeOfDayForecast tod);
        Task BuildForecastAsync(NwsTimeOfDayForecast timeOfDayForecast);
        ObservableCollection<ForecastInputs> BuildForecastInputs(NwsHourlyForecast[] hours);
        Task<NwsTimeOfDayForecast[]> BuildTimeOfDayForecastsAsync(NwsHourlyForecast[] hours);
        double Max(Bin bin);
        double Mean(Bin bin);
        double Min(Bin bin);
        string ProcessTrend(double currentValue, double futureValue);
        string TempCategory(double temp, double dewPoint, double relativeHumidity);
        string TemperatureStatement(double t, double d, double q);
        string Trend(double previousMean, double currentMean);
    }
}