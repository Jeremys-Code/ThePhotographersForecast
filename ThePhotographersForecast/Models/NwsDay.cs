using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models
{
    public class NwsDay
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsDay { get; set; }
        public SKFileLottieImageSource SkDailyIconSource { get; set; }
        public string DailyStatement { get; set; }
        public NwsHour[] NwsHours { get; set; }
        public NwsHourlySupplemental[] DailySupplementals { get; set; }

    }
    public class NwsHour
    {
        public int Number { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsDay { get; set; }
        public SKFileLottieImageSource SkHourlyIconSource { get; set; }
        public string HourlyStatement { get; set; }
        public NwsHourlySupplemental HourlySupplemental { get; set; }
    }
}
