namespace ThePhotographersForecast.Data
{
    public class SkyData
    {

        public class Rootobject
        {
            public float latitude { get; set; }
            public float longitude { get; set; }
            public float generationtime_ms { get; set; }
            public int utc_offset_seconds { get; set; }
            public string timezone { get; set; }
            public string timezone_abbreviation { get; set; }
            public float elevation { get; set; }
            public Hourly_Units hourly_units { get; set; }
            public Hourly hourly { get; set; }
        }

        public class Hourly_Units
        {
            public string time { get; set; }
            public string temperature_2m { get; set; }
            public string cloud_cover { get; set; }
            public string cloud_cover_low { get; set; }
            public string cloud_cover_mid { get; set; }
            public string cloud_cover_high { get; set; }
            public string visibility { get; set; }
            public string relative_humidity_2m { get; set; }
            public string wind_direction_120m { get; set; }
            public string wind_speed_120m { get; set; }
            public string wind_gusts_10m { get; set; }
            public string precipitation_probability { get; set; }
            public string precipitation { get; set; }
            public string apparent_temperature { get; set; }
        }

        public class Hourly
        {
            public string[] time { get; set; }
            public float[] temperature_2m { get; set; }
            public int[] cloud_cover { get; set; }
            public int[] cloud_cover_low { get; set; }
            public int[] cloud_cover_mid { get; set; }
            public int[] cloud_cover_high { get; set; }
            public float[] visibility { get; set; }
            public int[] relative_humidity_2m { get; set; }
            public int[] wind_direction_120m { get; set; }
            public float[] wind_speed_120m { get; set; }
            public float[] wind_gusts_10m { get; set; }
            public int[] precipitation_probability { get; set; }
            public float[] precipitation { get; set; }
            public float[] apparent_temperature { get; set; }
        }

    }
}
