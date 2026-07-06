using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models;

public class MappedHourly
{
    //Naming format: Sunday Morning @ 11AM
    public string Name => String.Concat(this.StartTime.ToString("dddd"), " ", Services.GetTimeOfDay.CalcTimeOfDay(this.StartTime.Hour).ToString(), " @ ", this.StartTime.ToString("h tt"));
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsDay { get; set; }
    public string TimeOfDay => Services.GetTimeOfDay.CalcTimeOfDay(this.StartTime.Hour).ToString();
    public double HourlyTemperature { get; set; }
    public string HourlyTemperatureUnits { get; set; } = string.Empty;
    public string Temp => $"{Math.Round(this.HourlyTemperature, 0)}{this.HourlyTemperatureUnits}";
    public bool IsDailyHigh { get; set; }
    public bool IsDailyLow { get; set; }
    public ImageSource HourlyImageSource { get; set; } = ImageSource.FromFile("unknown.png");
    public string HourlyForecast { get; set; } = string.Empty;
    public EventTime[] Events { get; set; }
}
