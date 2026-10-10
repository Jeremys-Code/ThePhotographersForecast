using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Modules
{
    public interface IForecastService
    {
        CalendarDayModel[] Dates { get; set; }
        HourlyForecast[] gfsHourlies { get; set; }
        NwsHourlyForecast[] NwsHourlies { get; set; }
        DateTime SelectedDate { get; set; }
        DateTime SelectedTime { get; set; }

        Task<ObservableCollection<string>> BuildDailyForecastAsync(NwsDailyForecast nwsDaily);
        string BuildTemperatureStatement(NwsHourlyForecast[] evolutions);
        double Max(Bin bin);
        double Mean(Bin bin);
        double Min(Bin bin);
        string TempCategory(double temp, double dewPoint, double relativeHumidity, double wbgt);
        string Trend(double previousMean, double currentMean);
    }
}