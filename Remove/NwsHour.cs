using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models
{
    public class NwsHour
    {
        public int Number { get; set; }
        public DateTime StartTime { get; set; }

        public bool IsDay { get; set; }
        public SKFileLottieImageSource SkIconSource { get; set; }
        public string Description { get; set; }

        public NwsHourlySupplemental HourlySupplemental { get; set; }

        public NwsHourlySupplemental[] DailySupplementals { get; set; }

    }
}
