using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Data
{
    public class GeocodeResult
    {
        public class Rootobject
        {
            public Class1[] Property1 { get; set; }
        }

        public class Class1
        {
            public int place_id { get; set; }
            public string licence { get; set; }
            public string osm_type { get; set; }
            public int osm_id { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public string _class { get; set; }
            public string type { get; set; }
            public int place_rank { get; set; }
            public float importance { get; set; }
            public string addresstype { get; set; }
            public string name { get; set; }
            public string display_name { get; set; }
            public Address address { get; set; }
            public double[] boundingbox { get; set; }
        }

        public class Address
        {
            public string county { get; set; }
            public string state { get; set; }
            public string ISO31662lvl4 { get; set; }
            public string country { get; set; }
            public string country_code { get; set; }
            public string suburb { get; set; }
            public string city { get; set; }
            public string village { get; set; }
            public string town { get; set; }
            public string postcode { get; set; }
            public string neighbourhood { get; set; }
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
