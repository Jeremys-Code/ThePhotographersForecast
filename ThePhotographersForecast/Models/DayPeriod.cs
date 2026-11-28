using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Models;

public partial class DayViewModel : ObservableObject
{
    [ObservableProperty]
    public partial PhotoLocation SelectedLocation { get; set; }
    [ObservableProperty]
    public partial DateOnly Date { get; set; }
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
    public partial NwsTimeOfDayForecast[] TimeOfDayForecasts { get; set; }
    [ObservableProperty]
    public partial EventModel Events { get; set; }
    [ObservableProperty]
    public partial string DailyDetailedForecast { get; set; }
    [ObservableProperty]
    public partial string DailyScore { get; set; }
    //[ObservableProperty]
    //public partial FutureMoonPhase[] MoonPhases { get; set; }
    [ObservableProperty]
    public partial LinearGradientBrush DailySkyBrush { get; set; }
    public string DailyLowStatement { get; set; }
    public string DailyHighStatement { get; set; }
    public SKFileLottieImageSource SkLowAnimation => new SKFileLottieImageSource { File = "thermometer_colder.json" };
    public SKFileLottieImageSource SkHighAnimation => new SKFileLottieImageSource { File = "thermometer_warmer.json" };
    public Color TimeOfDayTextColor => TimeOfDayService.GetTimeOfDayTextColor(Name.ToLower().Contains("night") ? 22 : 12);

}
public partial class DayPeriod : ObservableObject
{
    [ObservableProperty]
    public partial DateTime StartTime { get; set; }
    [ObservableProperty]
    public partial DateTime EndTime { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayName { get; set; }
    [ObservableProperty]
    public partial SKFileLottieImageSource TimeOfDaySkAnimation { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<EventTime> TimeOfDayEvents { get; set; } = new ObservableCollection<EventTime>();
    [ObservableProperty]
    public partial LinearGradientBrush TimeOfDayBackgroundBrush { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<LottieAnimation> TimeOfDaySkWeatherAnimations { get; set; }
    [ObservableProperty]
    public partial LottieAnimation SelectedTimeOfDaySkWeatherAnimation { get; set; }
    [ObservableProperty]
    public partial NwsHourlyForecast[] HourlyWeather { get; set; }
    [ObservableProperty]
    public partial string ForecastName { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayTemperature { get; set; }
    [ObservableProperty]
    public partial string TimeOfDayForecast { get; set; }
    [ObservableProperty]
    public partial string PhotoScore { get; set; }
    [ObservableProperty]
    public partial string DroneScore { get; set; }
    [ObservableProperty]
    public partial string ScoreSummary { get; set; }
    public Color TimeOfDayTextColor => TimeOfDayService.GetTimeOfDayTextColor(this.StartTime.Hour);

}


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
