using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models;

public class MappedDaily
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsDay { get; set; }
    public double DailyTemperature { get; set; }
    public string DailyTemperatureUnits { get; set; } = string.Empty;
    public string DailyHigh { get; set; } = string.Empty;
    public string DailyLow { get; set; } = string.Empty;
    public ImageSource DailyImageSource { get; set; }
    public string DailyForecast { get; set; } = string.Empty;
    public MappedHourly[] HourlyCollection { get; set; }
    //public DailyAstronomicalData AstroData { get; set; }
    public EventTime[] DailyEvents { get; set; }
}
