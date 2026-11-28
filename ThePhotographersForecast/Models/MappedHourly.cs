using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models;

public class MappedHourly
{
    //Naming format: Sunday Morning @ 11AM
    public string Name => String.Concat(this.StartTime.ToString("dddd"), " ", Services.TimeOfDayService.CalcTimeOfDay(this.StartTime.Hour).ToString(), " @ ", this.StartTime.ToString("h tt"));
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public bool IsDay { get; set; }
    public string TimeOfDay => Services.TimeOfDayService.CalcTimeOfDay(this.StartTime.Hour).ToString();
    public TemperatureValue Temperature { get; set; }
    public bool IsDailyHigh { get; set; }
    public bool IsDailyLow { get; set; }
    public SKFileLottieImageSource SkHourlyAnimation { get; set; }
    public string HourlyForecast { get; set; } = string.Empty;
    public EventTime[] Events { get; set; }
    public string TemperatureStatement { get; set; }
    public string SkyCoverStatement { get; set; }
    public string WindSpeedStatement { get; set; }
    public string BriefForecast { get; set; }
    public string PrecipitationStatement { get; set; }
    public string StormStatement { get; set; }
    public string HealthSafetyStatement { get; set; }


}

// Temp
//
//