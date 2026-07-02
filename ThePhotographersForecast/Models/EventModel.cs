using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Models
{
    public class EventModel
    {
        public DateTime Date { get; set; }
        public EventTime[] EventTimes { get; set; }
        public DailyAstronomicalData AstronomicalData { get; set; }
    }
    public class EventTime
    {
        public DateTime Date { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string TimeOfDay => Services.GetTimeOfDay.TimeOfDay(AstroTime.Hour);
        public DateTime AstroTime { get; set; }
        public DateTime? OtherTime { get; set; }
        public DateTime WeatherHour => GetWeatherHour();
        public DateTime OrderTime => AstroTime;
        private DateTime GetWeatherHour()
        {
            var dateTime = new DateTime();
            if (AstroTime.Minute > 45)
            {
                if (AstroTime.Hour != 23)
                {
                    dateTime = new DateTime(AstroTime.Year, AstroTime.Month, AstroTime.Day, AstroTime.AddHours(1).Hour, 0, 0);
                }
                else
                {
                    dateTime = new DateTime(AstroTime.Year, AstroTime.Month, AstroTime.AddDays(1).Day, AstroTime.AddHours(1).Hour, 0, 0);
                }
            }
            else
            {
                dateTime = new DateTime(AstroTime.Year, AstroTime.Month, AstroTime.Day, AstroTime.Hour, 0, 0);
            }
            return dateTime;
        }
    }

    public class DailyAstronomicalData
    {
        public DateTime Date { get; set; }
        public TimeSpan DayLength { get; set; }
        public double SunAltitude { get; set; }
        public double SunDistance { get; set; }
        public double SunAzimuth { get; set; }
        public string MoonPhase { get; set; }
        public double MoonAltitude { get; set; }
        public double MoonDistance { get; set; }
        public double MoonAzimuth { get; set; }
        public double MoonParallacticAngle { get; set; }
        public double MoonIlluminationPercentage { get; set; }
        public double MoonAngle { get; set; }
    }
}

public class TimeOfDayGroup
{
    public string TimeOfDay { get; set; }
    public ObservableCollection<EventTime> EventTimes { get; set; }
    public TimeOfDayGroup(string timeOfDay, ObservableCollection<EventTime> eventTimes)
    {
        TimeOfDay = timeOfDay;
        EventTimes = eventTimes;
    }

}