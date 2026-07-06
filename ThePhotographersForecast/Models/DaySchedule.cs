using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models;

public class DaySchedule
{
    public DateTime PhotoDate { get; set; }
    public string? Holiday { get; set; }
    public ObservableCollection<PhotoSchedule> PhotoSchedules { get; set; } = new ObservableCollection<PhotoSchedule>();
}
    public class PhotoSchedule
{
    public DateTime PhotoDate { get; set; }
    public string TimeOfDay => Services.GetTimeOfDay.CalcTimeOfDay(this.StartTime.Hour).ToString();
    public string Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double HourlyTemperature { get; set; }
    public string HourlyTemperatureUnits { get; set; }
    public bool IsHigh { get; set; } = false;
    public bool IsLow { get; set; } = false;
    public ImageSource ForecastImageSource { get; set; } = ImageSource.FromFile("unknown.png");
    public string ForecastDetail { get; set; } = string.Empty;
    public ObservableCollection<EventTime> Events { get; set; } = new ObservableCollection<EventTime>();
}