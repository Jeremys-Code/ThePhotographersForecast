using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ThePhotographersForecast.Data;

public class GridHourlyData
{

    public class Rootobject
    {
        public object[]? context { get; set; }
        public string type { get; set; }
        [JsonProperty("geometry")]
        public Geometry geometry { get; set; }
        [JsonProperty("properties")]
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
        public float value { get; set; }
    }

    public class Period
    {
        [JsonProperty("number")]
        public int number { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("startTime")]
        public DateTime startTime { get; set; }
        [JsonProperty("endTime")]
        public DateTime endTime { get; set; }
        [JsonProperty("isDaytime")]
        public bool isDaytime { get; set; }
        [JsonProperty("temperature")]
        public double temperature { get; set; }
        [JsonProperty("temperatureUnit")]
        public string temperatureUnit { get; set; }

        [JsonProperty("temperatureTrend")]
        public string? temperatureTrend { get; set; }
        [JsonProperty("probabilityOfPrecipitation")]
        public Probabilityofprecipitation probabilityOfPrecipitation { get; set; }
        [JsonProperty("dewpoint")]
        public Dewpoint dewpoint { get; set; }
        [JsonProperty("relativeHumidity")]
        public Relativehumidity relativeHumidity { get; set; }
        [JsonProperty("windSpeed")]
        public string windSpeed { get; set; }
        [JsonProperty("windDirection")]
        public string windDirection { get; set; }

        [JsonProperty("hourlyIcon")]
        public string icon { get; set; }
        [JsonProperty("shortForecast")]
        public string shortForecast { get; set; }

    }

    public class Probabilityofprecipitation
    {
        public string unitCode { get; set; }
        public double value { get; set; }
    }

    public class Dewpoint
    {
        public string unitCode { get; set; }
        public float value { get; set; }
    }

    public class Relativehumidity
    {
        public string unitCode { get; set; }
        public double value { get; set; }
    }

}
