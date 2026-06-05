using System.Diagnostics;

namespace ThePhotographersForecast.Models
{
    public class PhotoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime StartTime { get; set; } = default;
        public DateTime EndTime { get; set; } = default;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public string Country { get; set; } = "United States of America";
        public double Elevation { get; set; } = 0.0;
        public int Population { get; set; } = 0;
        public int TimeZoneOffset => GetTimeZoneInfo().GetUtcOffset(DateTime.UtcNow).Hours;
        public string DisplayName => string.Concat(this.City, ", ", this.State, ", ", this.Country);
        public TimeZoneInfo GetTimeZoneInfo()
        {
            if (!string.IsNullOrEmpty(TimeZone))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    Debug.WriteLine($"The Time Zone '{TimeZone}' was not found on the system.");
                }
                catch (InvalidTimeZoneException)
                {
                    Debug.WriteLine($"The Time Zone '{TimeZone}' is invalid.");
                }
            }
            return TimeZoneInfo.Local; // Fallback to local timezone if not set or invalid
        }
    }
}
