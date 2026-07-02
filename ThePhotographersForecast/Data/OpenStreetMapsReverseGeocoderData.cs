namespace ThePhotographersForecast.Data
{
    public class OpenStreetMapsReverseGeocoderData
    {
        public class Rootobject
        {
            public int place_id { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string _class { get; set; }
            public string type { get; set; }
            public int place_rank { get; set; }
            public float importance { get; set; }
            public string addresstype { get; set; }
            public string name { get; set; }
            public string display_name { get; set; }
            public Address address { get; set; }
            public string[] boundingbox { get; set; }
        }

        public class Address
        {
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
        }


    }
}
