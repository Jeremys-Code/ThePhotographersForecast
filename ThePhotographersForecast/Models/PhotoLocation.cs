using CommunityToolkit.Mvvm.ComponentModel;

namespace ThePhotographersForecast.Models
{
    public partial class PhotoLocation : ObservableObject
    {
        [ObservableProperty]
        public partial string GeocodeDisplayName { get; set; } = string.Empty;
        public string DisplayName => CreateDisplayName();
        [ObservableProperty]
        public partial double Latitude { get; set; }
        [ObservableProperty]
        public partial double Longitude { get; set; }
        [ObservableProperty]
        public partial string City { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string County { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string State { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string Country { get; set; } = string.Empty;
        [ObservableProperty]
        public partial ElevationValue Elevation { get; set; }
        [ObservableProperty]
        public partial string TimeZoneName { get; set; }
        [ObservableProperty]
        public partial string TimeZoneOffset { get; set; }

        private string CreateDisplayName()
        {
            string displayName = string.Empty;
            displayName = this.City + ", " + this.State;
            return displayName;
        }

        public PhotoLocation()
        { }

        public PhotoLocation(PhotoLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location), "Source object cannot be null.");

            Latitude = location.Latitude;
            Longitude = location.Longitude;
            City = location.City;
            State = location.State;
            County = location.County;
            Country = location.Country;
            GeocodeDisplayName = location.GeocodeDisplayName;
            Elevation = location.Elevation;
            TimeZoneName = location.TimeZoneName;
            TimeZoneOffset = location.TimeZoneOffset;
        }
    }

    public class ElevationValue
    {
        public double Elevation { get; set; }
        public string UnitCode { get; set; }
        public string ElevationStatement => GetElevationString();
        private string GetElevationString()
        {
            return $"Elevation: {Math.Round(this.Elevation, 0)}{this.UnitCode} above sea level (MSL).";
        }
    }
}
