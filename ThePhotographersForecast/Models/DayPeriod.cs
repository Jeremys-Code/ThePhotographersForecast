using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models;

public partial class DayViewModel : ObservableObject
{
    [ObservableProperty]
    public partial LocationViewModel SelectedLocation { get; set; }
    [ObservableProperty]
    public partial DateTime Date { get; set; }
    [ObservableProperty]
    public partial string Name { get; set; }
    [ObservableProperty]
    public partial int Day { get; set; }

    [ObservableProperty]
    public partial bool IsEventDay { get; set; }

    [ObservableProperty]
    public partial bool IsEventWindowDay { get; set; }

    [ObservableProperty]
    public partial bool IsToday { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    [ObservableProperty]
    public partial bool IsFlipped { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<DayPeriod> Periods { get; set; }
    [ObservableProperty]
    public partial NwsHourlyForecast[] SelectedDailyHourlyForecasts { get; set; }
    [ObservableProperty]
    public partial NwsHourlyForecast CurrentForecast { get; set; }
    [ObservableProperty]
    public partial NwsDailyForecast[] DailyForecasts { get; set; }
    [ObservableProperty]
    public partial NwsDailyForecast SelectedDailyForecast { get; set; }
    [ObservableProperty]
    public partial EventModel Events { get; set; }
    [ObservableProperty]
    public partial string DailyDetailedForecast { get; set; }

    public string DailyLowStatement => GetDailyLowStatement();
    public string DailyHighStatement => GetDailyHighStatement();
    public static SKFileLottieImageSource SkLowAnimation = new SKFileLottieImageSource { File = "metecons/thermometer_colder.json" };
    public static SKFileLottieImageSource SkHighAnimation = new SKFileLottieImageSource { File = "meteocons/thermometer_warmer.json" };

    private string GetDailyLowStatement()
    {
        var lowTemp = this.SelectedDailyHourlyForecasts.Where(x => x.StartTime.Date == this.Date.Date && x.IsDaytime == false).MinBy(s => s.Temperature.Value);
        var selectedTimeLowTemp = lowTemp != null ? $"{Math.Round(lowTemp.Temperature.Value, 0)}{lowTemp.Temperature.UnitCode} at {lowTemp.StartTime.ToString("h tt")}" : string.Empty;
        return selectedTimeLowTemp;
    }
    private string GetDailyHighStatement()
    {
        var highTemp = this.SelectedDailyHourlyForecasts.Where(x => x.StartTime.Date == this.Date.Date && x.IsDaytime == true).MaxBy(s => s.Temperature.Value);
        var selectedTimeHighTemp = highTemp != null ? $"{Math.Round(highTemp.Temperature.Value, 0)}{highTemp.Temperature.UnitCode} at {highTemp.StartTime.ToString("htt")}." : string.Empty;
        return selectedTimeHighTemp;
    }
}
public partial class DayPeriod : ObservableObject
{
    [ObservableProperty]
    public partial DateTime Time { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayName { get; set; }
    [ObservableProperty]
    public partial SKFileLottieImageSource TimeOfDayImage { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<EventTime> TimeOfDayEvents { get; set; } = new ObservableCollection<EventTime>();
    [ObservableProperty]
    public partial LinearGradientBrush TimeOfDayBackgroundBrush { get; set; }
    [ObservableProperty]
    public partial SKFileLottieImageSource TimeOfDayWeatherImages { get; set; }
    [ObservableProperty]
    public partial NwsHourlyForecast[] HourlyWeather { get; set; }
    [ObservableProperty]
    public partial string HourlyName { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayTemperature { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayDewpoint { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayApparentTemperature { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayShortForecast { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayWind { get; set; }
    [ObservableProperty]
    public partial string? TimeOfDayGust { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayStormSummary { get; set; }
    [ObservableProperty]
    public partial string TimeOfDaySummary { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayDetailForecast { get; set; }
    [ObservableProperty]
    public partial Dictionary<DateOnly, SKColor[]> SkyColor { get; set; }

    /// <summary>
    /// Gets the lottie parades for the day period.
    /// </summary>
    //public LottieParade[] LottieParades => GetLottieParades(SkyColor);

    //private LottieParade[] GetLottieParades(Dictionary<DateOnly, SKColor[]> skyColor)
    //{
    //    var parade = new LottieParade[this.HourlyWeather.Length];
    //    for (int i = 0; i < this.HourlyWeather.Length; i++)
    //    {

    //        var bkgnd = skyColor.TryGetValue(DateOnly.FromDateTime(this.HourlyWeather[i].StartTime), out var sC) ? sC : null;
    //        if (bkgnd != null)
    //        {
    //            parade[i] = new LottieParade
    //            {
    //                ParadeFloat = this.HourlyWeather[i].SkHourlyAnimation,
    //                Time = this.HourlyWeather[i].StartTime,
    //                SkyTransition = bkgnd
    //            };
    //        }

    //    }
    //    return parade;
    //}

}

public partial class LottieParade : ObservableObject
{
    [ObservableProperty]
    public partial SKFileLottieImageSource ParadeFloat { get; set; }
    [ObservableProperty]
    public partial DateTime Time { get; set; }
    [ObservableProperty]
    public partial SKColor[] SkyTransition { get; set; }
}



//public partial class HourlyForecastHighLights : ObservableObject
//{
//    [ObservableProperty]
//    public partial string TimeOfDay { get; set; }

//    [ObservableProperty]
//    public partial DateTime Hour { get; set; }

//    [ObservableProperty]
//    public partial string WeatherSummary { get; set; }
//    [ObservableProperty]
//    public partial string Temmperature { get; set; }

//    [ObservableProperty]
//    public partial SKFileLottieImageSource SkWmoImage { get; set; }

//    [ObservableProperty]
//    public partial string OutdoorHealthSafety { get; set; }

//    [ObservableProperty]
//    public partial EventTime[] Events { get; set; } = Array.Empty<EventTime>();
//}
public partial class LocationViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Location { get; set; }

    [ObservableProperty]
    public partial TimeZoneInfo TimeZone { get; set; }

    [ObservableProperty]
    public partial ElevationValue Elevation { get; set; }
    [ObservableProperty]
    public partial int TimeZoneOffset { get; set; }
}
// Sun and Moon from the observer's point of view recorded as an event record
