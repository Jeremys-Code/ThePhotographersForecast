namespace ThePhotographersForecast.Data
{
    public class SkyColorAttributesData
    {


        public class Rootobject
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double generationtime_ms { get; set; }
            public int utc_offset_seconds { get; set; }
            public string timezone { get; set; }
            public string timezone_abbreviation { get; set; }
            public int elevation { get; set; }
            public Hourly_Units hourly_units { get; set; }
            public Hourly hourly { get; set; }
        }

        public class Hourly_Units
        {
            public string time { get; set; }
            public string surface_pressure { get; set; }
            public string cloud_cover { get; set; }
            public string direct_normal_irradiance { get; set; }
            public string diffuse_radiation { get; set; }
            public string shortwave_radiation { get; set; }
        }

        public class Hourly
        {
            public DateTime[] time { get; set; }
            public double[] surface_pressure { get; set; }
            public int[] cloud_cover { get; set; }
            public double[] direct_normal_irradiance { get; set; }
            public double[] diffuse_radiation { get; set; }
            public double[] shortwave_radiation { get; set; }
        }


    }
}
