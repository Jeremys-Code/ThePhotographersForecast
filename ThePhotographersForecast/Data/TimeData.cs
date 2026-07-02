namespace ThePhotographersForecast.Data
{
    public class TimeData
    {
        public class Rootobject
        {
            public Location location { get; set; } = null!;
            public Astronomy astronomy { get; set; } = null!;
        }

        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
            public required string country_name { get; set; }
            public required string state_prov { get; set; }
            public required string city { get; set; }
            public string? locality { get; set; }
            public int elevation { get; set; }
        }

        public class Astronomy
        {
            public DateOnly date { get; set; }
            public string current_time { get; set; } = string.Empty;
            public string mid_night { get; set; } = string.Empty;
            public string night_end { get; set; } = string.Empty;
            public Morning morning { get; set; } = null!;
            public string sunrise { get; set; } = string.Empty;
            public string sunset { get; set; } = string.Empty;
            public Evening evening { get; set; } = null!;
            public string night_begin { get; set; } = string.Empty;
            public string solar_noon { get; set; } = string.Empty;
            public TimeSpan day_length { get; set; }
            public double sun_altitude { get; set; }
            public double sun_distance { get; set; }
            public double sun_azimuth { get; set; }
            public string moon_phase { get; set; }
            public string? moonrise { get; set; }
            public string? moonset { get; set; }
            public double moon_altitude { get; set; }
            public double moon_distance { get; set; }
            public double moon_azimuth { get; set; }
            public double moon_parallactic_angle { get; set; }
            public double moon_illumination_percentage { get; set; }
            public double moon_angle { get; set; }
        }

        public class Morning
        {
            public string astronomical_twilight_begin { get; set; } = string.Empty;
            public string astronomical_twilight_end { get; set; } = string.Empty;
            public string nautical_twilight_begin { get; set; } = string.Empty;
            public string nautical_twilight_end { get; set; } = string.Empty;
            public string civil_twilight_begin { get; set; } = string.Empty;
            public string civil_twilight_end { get; set; } = string.Empty;
            public string blue_hour_begin { get; set; } = string.Empty;
            public string blue_hour_end { get; set; } = string.Empty;
            public string golden_hour_begin { get; set; } = string.Empty;
            public string golden_hour_end { get; set; } = string.Empty;
        }

        public class Evening
        {
            public string golden_hour_begin { get; set; } = string.Empty;
            public string golden_hour_end { get; set; } = string.Empty;
            public string blue_hour_begin { get; set; } = string.Empty;
            public string blue_hour_end { get; set; } = string.Empty;
            public string civil_twilight_begin { get; set; } = string.Empty;
            public string civil_twilight_end { get; set; } = string.Empty;
            public string nautical_twilight_begin { get; set; } = string.Empty;
            public string nautical_twilight_end { get; set; } = string.Empty;
            public string astronomical_twilight_begin { get; set; } = string.Empty;
            public string astronomical_twilight_end { get; set; } = string.Empty;
        }
    }
}
