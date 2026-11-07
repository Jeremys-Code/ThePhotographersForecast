using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models
{

    public class EventModel
    {
        public EventModel()
        {

        }

        public EventModel(EventModel eventModel)
        {
            this.Date = eventModel.Date;
            this.AstronomicalData = new Astronomical(eventModel.AstronomicalData);
            this.EventTimes = new EventTime[eventModel.EventTimes.Length];
            this.EventTimes = [.. eventModel.EventTimes];
        }
        public DateTime Date { get; set; }
        public EventTime[] EventTimes { get; set; }
        public Astronomical AstronomicalData { get; set; } = new Astronomical();
    }
    public class EventTime
    {
        public string EventName { get; set; } = string.Empty;
        public string TimeOfDay => Services.TimeOfDayService.CalcTimeOfDay(this.WeatherHour.Hour);
        public string TimeLabel => (OtherTime == null ? $"Time: {this.AstroTime.ToString("h:mm tt")}" : $"Start Time: {this.AstroTime.ToString("h:mm tt")}");
        public DateTime AstroTime { get; set; }
        public string? OtherTimeLabel => (OtherTime != null ? $"End Time: {this.OtherTime?.ToString("h:mm tt")}" : null);
        public DateTime? OtherTime { get; set; }
        public DateTime WeatherHour { get; set; }
        public DateTime OrderTime => AstroTime;
        public NwsHourlyForecast? Forecast { get; set; }
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


    public class Astronomical
    {
        public Astronomical()
        {

        }

        public Astronomical(Astronomical astro)
        {
            this.DayLength = astro.DayLength;
            this.SunAltitude = astro.SunAltitude;
            this.SunAzimuth = astro.SunAzimuth;
            this.MoonPhase = astro.MoonPhase;
            this.MoonImage = astro.MoonImage;
            this.SkMoon = astro.SkMoon;
        }
        public TimeSpan DayLength { get; set; }
        public double SunAltitude { get; set; }
        //public double SunDistance { get; set; }
        public double SunAzimuth { get; set; }
        public string MoonPhase { get; set; }
        //public double MoonAltitude { get; set; }
        //public double MoonDistance { get; set; }
        //public double MoonAzimuth { get; set; }
        //public double MoonParallacticAngle { get; set; }
        //public double MoonIlluminationPercentage { get; set; }
        //public double MoonAngle { get; set; }
        public ImageSource MoonImage { get; set; }
        public SKFileLottieImageSource SkMoon { get; set; }
    }
}
