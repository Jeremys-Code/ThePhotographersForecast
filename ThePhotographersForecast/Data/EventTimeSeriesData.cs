namespace ThePhotographersForecast.Data
{
    public class EventTimeSeriesData
    {

        public class Rootobject
        {
            public Location location { get; set; }
            public Astronomy[] astronomy { get; set; }
        }

        public class Location
        {
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string country_name { get; set; }
            public string state_prov { get; set; }
            public string city { get; set; }
            public string locality { get; set; }
            public string elevation { get; set; }
        }

        public class Astronomy
        {
            public DateOnly date { get; set; }
            public string mid_night { get; set; }
            public string night_end { get; set; }
            public Morning morning { get; set; }
            public string sunrise { get; set; }
            public string sunset { get; set; }
            public Evening evening { get; set; }
            public string night_begin { get; set; }
            public string solar_noon { get; set; }
            public TimeSpan day_length { get; set; }
            public string moon_phase { get; set; }
            public string? moonrise { get; set; }
            public string? moonset { get; set; }
        }

        public class Morning
        {
            public string astronomical_twilight_begin { get; set; }
            public string astronomical_twilight_end { get; set; }
            public string nautical_twilight_begin { get; set; }
            public string nautical_twilight_end { get; set; }
            public string civil_twilight_begin { get; set; }
            public string civil_twilight_end { get; set; }
            public string blue_hour_begin { get; set; }
            public string blue_hour_end { get; set; }
            public string golden_hour_begin { get; set; }
            public string golden_hour_end { get; set; }
        }

        public class Evening
        {
            public string golden_hour_begin { get; set; }
            public string golden_hour_end { get; set; }
            public string blue_hour_begin { get; set; }
            public string blue_hour_end { get; set; }
            public string civil_twilight_begin { get; set; }
            public string civil_twilight_end { get; set; }
            public string nautical_twilight_begin { get; set; }
            public string nautical_twilight_end { get; set; }
            public string astronomical_twilight_begin { get; set; }
            public string astronomical_twilight_end { get; set; }
        }

    }
}
