namespace ThePhotographersForecast.Data;

public class GridDailyData
{

    public class RootObject
    {
        public object[]? context { get; set; }
        public string? type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public required string type { get; set; }
        public required double[][][] coordinates { get; set; }
    }

    public class Properties
    {
        public string units { get; set; }
        public string forecastGenerator { get; set; }
        public DateTime generatedAt { get; set; }
        public DateTime updateTime { get; set; }
        public required string validTimes { get; set; }
        public required Elevation elevation { get; set; }
        public required Period[] periods { get; set; }
    }

    public class Elevation
    {
        public required string unitCode { get; set; }
        public double value { get; set; }
    }

    public class Period
    {
        public int number { get; set; }
        public required string name { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public bool isDaytime { get; set; }
        public double temperature { get; set; }
        public required string temperatureUnit { get; set; }
        public required Probabilityofprecipitation probabilityOfPrecipitation { get; set; }
        public required string windSpeed { get; set; }
        public required string windDirection { get; set; }
        public required string icon { get; set; }
        public string? shortForecast { get; set; }
        public required string detailedForecast { get; set; }
    }

    public class Probabilityofprecipitation
    {
        public required string unitCode { get; set; }
        public int value { get; set; }
    }

}
