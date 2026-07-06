namespace ThePhotographersForecast.Models
{
    public class DailySunMoon
    {
        public DateTime Date { get; set; }
        public SunTimes SunTimes { get; set; }
        public MoonTimes MoonTimes { get; set; }
        public MoonIllumination Phase { get; set; }
    }
}
