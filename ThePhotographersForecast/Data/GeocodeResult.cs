using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Data
{
    public class GeocodeResult
    {

        public class Rootobject
        {
            public Feature[] features { get; set; }
        }

        public class Feature
        {
            public Properties properties { get; set; }
            public Geometry geometry { get; set; }
        }

        public class Properties
        {
            public Geocoding1 geocoding { get; set; }
        }

        public class Geocoding1
        {
            public int? place_id { get; set; }
            public string osm_type { get; set; }
            public int? osm_id { get; set; }
            public string osm_key { get; set; }
            public string osm_Value { get; set; }
            public string type { get; set; }
            public string label { get; set; }
            public string name { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string country_code { get; set; }
            public Admin admin { get; set; }
            public string city { get; set; }
            public string county { get; set; }
            public string district { get; set; }
            public string postcode { get; set; }
        }

        public class Admin
        {
            public string level6 { get; set; }
            public string level4 { get; set; }
            public string level8 { get; set; }
            public string level10 { get; set; }
        }

        public class Geometry
        {
            public string type { get; set; }
            public float[] coordinates { get; set; }
        }


    }
}
//public class Rootobject
//{
//    public Result[] results { get; set; }
//}

//public class Result
//{
//    public int id { get; set; }
//    public string name { get; set; } = string.Empty;
//    public double latitude { get; set; }
//    public double longitude { get; set; }
//    public double? elevation { get; set; }
//    public string feature_code { get; set; } = string.Empty;
//    public string country_code { get; set; } = string.Empty;
//    public int? admin1_id { get; set; }
//    public int? admin3_id { get; set; }
//    public string timezone { get; set; } = string.Empty;
//    public int? population { get; set; }
//    public string[] postcodes { get; set; } = Array.Empty<string>();
//    public int country_id { get; set; }
//    public string country { get; set; } = string.Empty;
//    public string admin1 { get; set; } = string.Empty;
//    public string? admin3 { get; set; } = string.Empty;
//    public int admin2_id { get; set; }
//    public string admin2 { get; set; } = string.Empty;
//}
