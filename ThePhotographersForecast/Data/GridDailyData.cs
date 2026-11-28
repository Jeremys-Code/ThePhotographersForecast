namespace ThePhotographersForecast.Models;

public class GridDailyData
{


    public class Rootobject
    {
        public object[] context { get; set; }
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public double[][][] coordinates { get; set; }
    }

    public class Properties
    {
        public string units { get; set; }
        public string forecastGenerator { get; set; }
        public DateTime generatedAt { get; set; }
        public DateTime updateTime { get; set; }
        public string validTimes { get; set; }
        public Elevation elevation { get; set; }
        public Period[] periods { get; set; }
    }

    public class Elevation
    {
        public string unitCode { get; set; }
        public double value { get; set; }
    }

    public class Period
    {
        public int number { get; set; }
        public string name { get; set; }
        public DateTimeOffset startTime { get; set; }
        public DateTimeOffset endTime { get; set; }
        public bool isDaytime { get; set; }
        public Temperature temperature { get; set; }
        public object temperatureTrend { get; set; }
        public Probabilityofprecipitation probabilityOfPrecipitation { get; set; }
        public Windspeed windSpeed { get; set; }
        public Windgust windGust { get; set; }
        public string windDirection { get; set; }
        public string icon { get; set; }
        public string shortForecast { get; set; }
        public string detailedForecast { get; set; }
    }

    public class Temperature
    {
        public string unitCode { get; set; }
        public double value { get; set; }
    }

    public class Probabilityofprecipitation
    {
        public string unitCode { get; set; }
        public double value { get; set; }
    }

    public class Windspeed
    {
        public string unitCode { get; set; }
        public double value { get; set; }
        public double maxValue { get; set; }
        public double minValue { get; set; }
    }

    public class Windgust
    {
        public string unitCode { get; set; }
        public double value { get; set; }
    }

}
