using System.Diagnostics;

namespace ThePhotographersForecast.Models
{
    public class PhotoLocation
    {
        public double Latitude { get; set; } = 42.31250;
        public double Longitude { get; set; } = -70.99787;
        public string? Village { get; set; }
        public string? Town { get; set; }
        public string? Suburb { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string State { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public string Country { get; set; } = "United States";
        public double Elevation { get; set; } = 0.0;
        public LocationCoordinates SpatialCoordinates { get; set; } = new LocationCoordinates();
        public int TimeZoneOffset => GetTimeZoneInfo().GetUtcOffset(DateTime.UtcNow).Hours;
        public string? DisplayName { get; set; }
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
        private string CreateDisplayName()
        {
            string displayName = string.Empty;
            string[] nameParts = new string[5] { this.Neighborhood ?? "", this.Village ?? "", this.Town ?? "", this.Suburb ?? "", this.City ?? "" };
            for (int p = 0; p < nameParts.Length; p++)
            {
                if (!string.IsNullOrWhiteSpace(nameParts[p]))
                {
                    displayName += $"{nameParts[p]}, ";
                }
            }
            displayName += this.State + ", " + this.Country;
            return displayName;
        }


        public class LocationCoordinates
        {
            public (Double Latitude, Double Longitude) SouthWest { get; set; } = (-71.39122, 42.17460);
            public (Double Latitude, Double Longitude) NorthEast { get; set; } = (-70.60432, 42.45031);
        }
    }
}
