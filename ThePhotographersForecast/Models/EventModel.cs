using System.Collections.ObjectModel;
using System.Globalization;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Models
{
    public class EventModel
    {
        public DateOnly Date { get; set; }
        public ObservableCollection<EventTime> EventTimes { get; set; } = new ObservableCollection<EventTime>();
        public DailyAstronomicalData AstronomicalData { get; set; } = new DailyAstronomicalData();
    }
    public class EventTime
    {
        public DateOnly Date { get; set; }
        public string EventName { get; set; } = string.Empty;
        public TimeOfDay TimeOfDay => Services.GetTimeOfDay.CalcTimeOfDay(AstroTime.Hour);
        public DateTime AstroTime { get; set; }
        public DateTime? OtherTime { get; set; }
        public DateTime WeatherHour => GetWeatherHour();
        public DateTime OrderTime => AstroTime;
        private DateTime GetWeatherHour()
        {
            if (AstroTime == DateTime.MinValue)
            {
                return DateTime.MinValue;
            }
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
        public DateOnly Date { get; set; }
        public TimeSpan DayLength { get; set; }
        public double SunAltitude { get; set; }
        public double SunDistance { get; set; }
        public double SunAzimuth { get; set; }
        public string MoonPhase
        {
            get => field;
            set
            {
                TextInfo phase = CultureInfo.CurrentCulture.TextInfo;
                string reformat = phase.ToTitleCase(value.Replace("_", " ").ToLowerInvariant());
                field = reformat;
            }
        } = string.Empty;

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
    public DateTime Date { get; set; }
    public TimeOfDay TimeOfDay { get; set; }
    public ObservableCollection<EventTime> EventTimes { get; set; } = new ObservableCollection<EventTime>();
}

public enum TimeOfDay
{
    //From 12AM - 2:59:59AM
    Overnight,
    //From 3AM - 5:59:59AM
    EarlyMorning,
    //From 6AM - 11:59:59AM
    Morning,
    //From 12PM - 5:59:59PM
    Afternoon,
    //From 6PM - 9:59:59PM
    Evening,
    //From 10PM - 11:59:59PM
    Night
}
