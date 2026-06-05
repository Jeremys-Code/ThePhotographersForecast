using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models;


public class DaySchedule
{
    public DateTime PhotoDate { get; set; }
    public PhotoSchedule[] PhotoSchedules { get; set; }
}

public class Hourly
{
    //Naming format: Sunday Morning @ 11AM
    public string Name => String.Concat(this.StartTime.ToString("dddd"), " ", Services.GetTimeOfDay.TimeOfDay(this.StartTime.Hour), " @ ", this.StartTime.ToString("h tt"));
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string HourlyTemperature { get; set; }
    public string HourlyIcon { get; set; }
    public string HourlyForecast { get; set; }
    public ImageSource HourlyImageSource { get; set; } //=> new BitmapImage(new Uri(this.HourlyIcon, UriKind.Absolute));
}
public class Daily
{
    public string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public String DailyTemperature { get; set; }
    public string DailyHigh { get; set; }
    public string DailyLow { get; set; }
    public string DailyIcon { get; set; }
    public string DailyForecast { get; set; }
    public ImageSource DailyImageSource { get; set; }
}
public class PhotoSchedule
{
    public DateTime PhotoDateTime { get; set; }
    public string TimeOfDay => Services.GetTimeOfDay.TimeOfDay(this.PhotoDateTime.Hour);
    public string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsHigh { get; set; } = false;
    public bool IsLow { get; set; } = false;
    public string ForecastIcon { get; set; }
    public string ForecastDetail { get; set; }
    public ObservableCollection<EventTime> Events { get; set; } = new ObservableCollection<EventTime>();
}
