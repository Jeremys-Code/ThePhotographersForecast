namespace ThePhotographersForecast.Data
{
    public class GfsDataService
    {

        public class Rootobject
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double generationtime_ms { get; set; }
            public int utc_offset_seconds { get; set; }
            public string timezone { get; set; } = string.Empty;
            public string timezone_abbreviation { get; set; } = string.Empty;
            public Hourly_Units hourly_units { get; set; } = new Hourly_Units();
            public Hourly hourly { get; set; } = new Hourly();
            public Daily_Units daily_units { get; set; } = new Daily_Units();
            public Daily daily { get; set; } = new Daily();
        }

        public class Hourly_Units
        {
            public string time { get; set; } = string.Empty;
            public string temperature_2m { get; set; } = string.Empty;
            public string dew_point_2m { get; set; } = string.Empty;
            public string precipitation_probability { get; set; } = string.Empty;
            public string precipitation { get; set; } = string.Empty;
            public string rain { get; set; } = string.Empty;
            public string showers { get; set; } = string.Empty;
            public string snowfall { get; set; } = string.Empty;
            public string snow_depth { get; set; } = string.Empty;
            public string weather_code { get; set; } = string.Empty;
            public string cloud_cover { get; set; } = string.Empty;
            public string visibility { get; set; } = string.Empty;
            public string wind_speed_10m { get; set; } = string.Empty;
            public string wind_direction_10m { get; set; } = string.Empty;
            public string wind_gusts_10m { get; set; } = string.Empty;
            public string is_day { get; set; } = string.Empty;
            public string sunshine_duration { get; set; } = string.Empty;
            public string relative_humidity_2m { get; set; } = string.Empty;
            public string apparent_temperature { get; set; } = string.Empty;
            public string uv_index { get; set; } = string.Empty;
            public string wet_bulb_temperature_2m { get; set; } = string.Empty;
        }

        public class Hourly
        {
            public string[] time { get; set; }
            public double[] temperature_2m { get; set; }
            public double[] dew_point_2m { get; set; }
            public double[] precipitation_probability { get; set; }
            public double[] precipitation { get; set; }
            public double[] rain { get; set; }
            public double[] showers { get; set; }
            public double[] snowfall { get; set; }
            public double[] snow_depth { get; set; }
            public int[] weather_code { get; set; }
            public double[] cloud_cover { get; set; }
            public double[] visibility { get; set; }
            public double[] wind_speed_10m { get; set; }
            public int[] wind_direction_10m { get; set; }
            public double[] wind_gusts_10m { get; set; }
            public int[] is_day { get; set; }
            public double[] sunshine_duration { get; set; }
            public double[] relative_humidity_2m { get; set; }
            public double[] apparent_temperature { get; set; }
            public double[] uv_index { get; set; }
            public double[] wet_bulb_temperature_2m { get; set; }
        }

        public class Daily_Units
        {
            public string time { get; set; } = string.Empty;
            public string sunrise { get; set; } = string.Empty;
            public string sunset { get; set; } = string.Empty;
            public string daylight_duration { get; set; } = string.Empty;
            public string sunshine_duration { get; set; } = string.Empty;
            public string weather_code { get; set; } = string.Empty;
            public string temperature_2m_max { get; set; } = string.Empty;
            public string temperature_2m_min { get; set; } = string.Empty;
            public string relative_humidity_2m_mean { get; set; } = string.Empty;
            public string wind_speed_10m_max { get; set; } = string.Empty;
            public string wind_gusts_10m_max { get; set; } = string.Empty;
            public string wind_direction_10m_dominant { get; set; } = string.Empty;
            public string precipitation_probability_max { get; set; } = string.Empty;
            public string precipitation_probability_min { get; set; } = string.Empty;
            public string rain_sum { get; set; } = string.Empty;
            public string showers_sum { get; set; } = string.Empty;
            public string snowfall_sum { get; set; } = string.Empty;
            public string precipitation_sum { get; set; } = string.Empty;
            public string dew_point_2m_mean { get; set; } = string.Empty;
            public string uv_index_max { get; set; } = string.Empty;
            public string apparent_temperature_max { get; set; } = string.Empty;
            public string apparent_temperature_min { get; set; } = string.Empty;
            public string wet_bulb_temperature_2m_min { get; set; } = string.Empty;
            public string wet_bulb_temperature_2m_max { get; set; } = string.Empty;
        }

        public class Daily
        {
            public string[] time { get; set; }
            public string[] sunrise { get; set; }
            public string[] sunset { get; set; }
            public double[] daylight_duration { get; set; }
            public double[] sunshine_duration { get; set; }
            public int[] weather_code { get; set; }
            public double[] temperature_2m_max { get; set; }
            public double[] temperature_2m_min { get; set; }
            public double[] relative_humidity_2m_mean { get; set; }
            public double[] wind_speed_10m_max { get; set; }
            public double[] wind_gusts_10m_max { get; set; }
            public int[] wind_direction_10m_dominant { get; set; }
            public double[] precipitation_probability_max { get; set; }
            public double[] precipitation_probability_min { get; set; }
            public double[] rain_sum { get; set; }
            public double[] showers_sum { get; set; }
            public double[] snowfall_sum { get; set; }
            public double[] precipitation_sum { get; set; }
            public double[] dew_point_2m_mean { get; set; }
            public double[] uv_index_max { get; set; }
            public double[] apparent_temperature_max { get; set; }
            public double[] apparent_temperature_min { get; set; }
            public double[] wet_bulb_temperature_2m_min { get; set; }
            public double[] wet_bulb_temperature_2m_max { get; set; }
        }

    }
}
