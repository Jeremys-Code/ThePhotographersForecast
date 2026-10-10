namespace ThePhotographersForecast.Data
{
    // Max of 4 Days of Info can be pulled.
    public class SkyCompositionData
    {
        public class Rootobject
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double generationtime_ms { get; set; }
            public int utc_offset_seconds { get; set; }
            public string timezone { get; set; }
            public string timezone_abbreviation { get; set; }
            public double elevation { get; set; }
            public Hourly_Units hourly_units { get; set; }
            public Hourly hourly { get; set; }
        }

        public class Hourly_Units
        {
            public string time { get; set; }
            public string pm10 { get; set; }
            public string pm2_5 { get; set; }
            public string nitrogen_dioxide { get; set; }
            public string sulphur_dioxide { get; set; }
            public string ozone { get; set; }
            public string dust { get; set; }
            public string uv_index { get; set; }
            public string uv_index_clear_sky { get; set; }
            public string aerosol_optical_depth { get; set; }
            public string carbon_monoxide { get; set; }
        }

        public class Hourly
        {
            public DateTime[] time { get; set; }
            public double[] pm10 { get; set; }
            public double[] pm2_5 { get; set; }
            public double[] nitrogen_dioxide { get; set; }
            public double[] sulphur_dioxide { get; set; }
            public double[] ozone { get; set; }
            public double[] dust { get; set; }
            public double[] uv_index { get; set; }
            public double[] uv_index_clear_sky { get; set; }
            public double[] aerosol_optical_depth { get; set; }
            public double[] carbon_monoxide { get; set; }
        }

    }
}
