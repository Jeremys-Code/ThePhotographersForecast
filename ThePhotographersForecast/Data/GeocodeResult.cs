using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Data
{
    public class GeocodeResult
    {
        public class Rootobject
        {
            public Result[] results { get; set; }
        }

        public class Result
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double? elevation { get; set; }
            public string feature_code { get; set; } = string.Empty;
            public string country_code { get; set; } = string.Empty;
            public int? admin1_id { get; set; }
            public int? admin3_id { get; set; }
            public string timezone { get; set; } = string.Empty;
            public int? population { get; set; }
            public string[] postcodes { get; set; } = Array.Empty<string>();
            public int country_id { get; set; }
            public string country { get; set; } = string.Empty;
            public string admin1 { get; set; } = string.Empty;
            public string? admin3 { get; set; } = string.Empty;
            public int admin2_id { get; set; }
            public string admin2 { get; set; } = string.Empty;
        }
    }
}
