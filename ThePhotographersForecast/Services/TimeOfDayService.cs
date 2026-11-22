using System.Collections.ObjectModel;
using System.Text;

namespace ThePhotographersForecast.Services
{
    public static class TimeOfDayService
    {
        public static string CalcTimeOfDay(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 6):
                    return "Early Morning";
                case int h when (6 <= h && h < 12):
                    return "Morning";
                case int h when (12 <= h && h < 18):
                    return "Afternoon";
                case int h when (18 <= h && h < 22):
                    return "Evening";
                case int h when (22 <= h && h < 24):
                    return "Night";
                default:
                    return "Unknown";
            }
        }

        public static TimeOfDay GetTimeOfDay(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 6):
                    return TimeOfDay.EarlyMorning;
                case int h when (6 <= h && h < 12):
                    return TimeOfDay.Morning;
                case int h when (12 <= h && h < 18):
                    return TimeOfDay.Afternoon;
                case int h when (18 <= h && h < 22):
                    return TimeOfDay.Evening;
                case int h when (22 <= h && h < 24):
                    return TimeOfDay.Night;
                default:
                    return TimeOfDay.Morning;
            }
        }
        public static string TimesOfDayGreeting(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 12):
                    return "Good morning";
                case int h when (12 <= h && h < 18):
                    return "Good afternoon";
                case int h when (18 <= h && h < 24):
                    return "Good evening";
                default:
                    return "Hello";
            }
        }

        public static LinearGradientBrush GetTimeOfDayBackground(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 6):
                    return (LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"];
                case int h when (6 <= h && h < 12):
                    return (LinearGradientBrush)Application.Current.Resources["PeriodMorning"];
                case int h when (12 <= h && h < 18):
                    return (LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"];
                case int h when (18 <= h && h < 22):
                    return (LinearGradientBrush)Application.Current.Resources["PeriodEvening"];
                case int h when (22 <= h && h < 24):
                    return (LinearGradientBrush)Application.Current.Resources["PeriodNight"];
                default:
                    return (LinearGradientBrush)Application.Current.Resources["PeriodMorning"];
            }
        }

        public static Color GetTimeOfDayTextColor(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 6):
                    return Colors.White;
                case int h when (6 <= h && h < 12):
                    return Colors.Black;
                case int h when (12 <= h && h < 18):
                    return Colors.Black;
                case int h when (18 <= h && h < 22):
                    return Colors.White;
                case int h when (22 <= h && h < 24):
                    return Colors.White;
                default:
                    return Colors.Black;
            }
        }



        public enum TimeOfDay
        {
            EarlyMorning,
            Morning,
            Afternoon,
            Evening,
            Night
        }

    }
    //National Weather Service Alerts Types for later intergration
    //with api.weather.gov/alert/active/zone/{MAZ015} the zone id comes from the 
    //forecastZone located in the CoordinatesToGridData.properties.forecastZone
    //split on "/" and take the last item in the array.
    public enum NwsAlert
    {
        NineOneOneTelephoneOutage,
        AdministrativeMessage,
        AirQualityAlert,
        AirStagnationAdvisory,
        AshfallAdvisory,
        AshfallWarning,
        AvalancheAdvisory,
        AvalancheWarning,
        AvalancheWatch,
        BeachHazardsStatement,
        BlizzardWarning,
        BlowingDustAdvisory,
        BlowingDustWarning,
        BlueAlert,
        BriskWindAdvisory,
        ChildAbductionEmergency,
        CivilDangerWarning,
        CivilEmergencyMessage,
        CoastalFloodAdvisory,
        CoastalFloodStatement,
        CoastalFloodWarning,
        CoastalFloodWatch,
        ColdWeatherAdvisory,
        DenseFogAdvisory,
        DenseSmokeAdvisory,
        DustAdvisory,
        DustStormWarning,
        EarthquakeWarning,
        EvacuationImmediate,
        ExtremeHeatWarning,
        ExtremeHeatWatch,
        ExtremeColdWarning,
        ExtremeColdWatch,
        ExtremeFireDanger,
        ExtremeWindWarning,
        FireWarning,
        FireWeatherWatch,
        FlashFloodStatement,
        FlashFloodWarning,
        FlashFloodWatch,
        FloodAdvisory,
        FloodStatement,
        FloodWarning,
        FloodWatch,
        FreezeWarning,
        FreezeWatch,
        FreezingFogAdvisory,
        FreezingSprayAdvisory,
        FrostAdvisory,
        GaleWarning,
        GaleWatch,
        HazardousMaterialsWarning,
        HazardousSeasWarning,
        HazardousSeasWatch,
        HazardousWeatherOutlook,
        HeatAdvisory,
        HeavyFreezingSprayWarning,
        HeavyFreezingSprayWatch,
        HighSurfAdvisory,
        HighSurfWarning,
        HighWindWarning,
        HighWindWatch,
        HurricaneForceWindWarning,
        HurricaneForceWindWatch,
        HurricaneWarning,
        HurricaneWatch,
        HydrologicOutlook,
        IceStormWarning,
        LakeEffectSnowWarning,
        LakeWindAdvisory,
        LakeshoreFloodAdvisory,
        LakeshoreFloodStatement,
        LakeshoreFloodWarning,
        LakeshoreFloodWatch,
        LawEnforcementWarning,
        LocalAreaEmergency,
        LowWaterAdvisory,
        MarineWeatherStatement,
        NuclearPowerPlantWarning,
        RadiologicalHazardWarning,
        RedFlagWarning,
        RipCurrentStatement,
        SevereThunderstormWarning,
        SevereThunderstormWatch,
        SevereWeatherStatement,
        ShelterInPlaceWarning,
        ShortTermForecast,
        SmallCraftAdvisory,
        SnowSquallWarning,
        SpecialMarineWarning,
        SpecialWeatherStatement,
        StormSurgeWarning,
        StormSurgeWatch,
        StormWarning,
        StormWatch,
        Test,
        TornadoWarning,
        TornadoWatch,
        TropicalCycloneLocalStatement,
        TropicalStormWarning,
        TropicalStormWatch,
        TsunamiAdvisory,
        TsunamiWarning,
        TsunamiWatch,
        TyphoonWarning,
        TyphoonWatch,
        VolcanoWarning,
        WindAdvisory,
        WinterStormWarning,
        WinterStormWatch,
        WinterWeatherAdvisory
    }



    public class ForecastEvolution
    {
        //Buckets for holding the forecast hours
        public ObservableCollection<ValueEvolution> EarlyMorning { get; set; }
        public ObservableCollection<ValueEvolution> Morning { get; set; }
        public ObservableCollection<ValueEvolution> Afternoon { get; set; }
        public ObservableCollection<ValueEvolution> Evening { get; set; }
        public ObservableCollection<ValueEvolution> Night { get; set; }
    }
    public class ValueEvolution
    {
        // The Object that holds the Forecast Hour Values
        public DateTime Time { get; set; }
        public TimeOfDayService.TimeOfDay TimeOfDay => TimeOfDayService.GetTimeOfDay(this.Time.Hour);
        public int Order => Time.Hour;
        public bool IsDay { get; set; }
        public List<ForecastTemperature> Temperature { get; set; } = new List<ForecastTemperature>();
        public List<ForecastApparentTemperature> ApparentTemperature { get; set; } = new List<ForecastApparentTemperature>();
        public string PrecipitationType { get; set; }
        public double PrecipitationAmt { get; set; }
        public double WindSpeed { get; set; }
        public List<ForecastPrecipitation> Precipitation { get; set; } = new List<ForecastPrecipitation>();
        public List<ForecastWind> Wind { get; set; } = new List<ForecastWind>();
        public List<ForecastGusts> Gusts { get; set; } = new List<ForecastGusts>();
        public List<ForecastSkyCover> SkyCover { get; set; } = new List<ForecastSkyCover>();
        public List<ForecastMixingHeight> MixingHeight { get; set; } = new List<ForecastMixingHeight>();
        public List<ForecastWbgt> Wbgt { get; set; } = new List<ForecastWbgt>();
        public List<ForecastDewPoint> DewPoints { get; set; } = new List<ForecastDewPoint>();
        public List<ForecastRelativeHumidity> RelativeHumidity { get; set; } = new List<ForecastRelativeHumidity>();
        public List<ForecastHeatRisk> HeatRisk { get; set; } = new List<ForecastHeatRisk>();
        public List<ForecastWindChill> WindChill { get; set; } = new List<ForecastWindChill>();
        public List<ForecastShortForecast> ShortForecast { get; set; } = new List<ForecastShortForecast>();
        public List<ForecastWmoDescription> WeatherDescription { get; set; } = new List<ForecastWmoDescription>();
        public List<ForecastThunder> Thunder { get; set; } = new List<ForecastThunder>();
        public List<ForecastWeatherStatement> WeatherStatement { get; set; } = new List<ForecastWeatherStatement>();
        public List<ForecastIceAccumulation> IceAccumulation { get; set; } = new List<ForecastIceAccumulation>();
        public List<ForecastSnowfallAmount> SnowfallAmount { get; set; } = new List<ForecastSnowfallAmount>();

        public DateTime EndTime { get; set; }

    }

    public class ForecastCeilingHeight
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }

    public class ForecastTemperature
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastDewPoint
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastRelativeHumidity
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastLowOccuranceVisibilityIndex
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastApparentTemperature
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastVisibility
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastMaxTemperature
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastMinTemperature
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }

    public class ForecastWind
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string Direction { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastTransportWind
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string Direction { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastGusts
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastSkyCover
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastMixingHeight
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastWbgt
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastHeatRisk
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastHeatIndex
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastWindChill
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastPrecipitation
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Type { get; set; }
        public double Value { get; set; }
        public double Amount { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastIceAccumulation
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastThunder
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastSnowfallAmount
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastWeatherStatement
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastShortForecast
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class ForecastWmoDescription
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Value { get; set; }
        public string UnitCode { get; set; }
    }
    //If a Forecast Value stays the same over consective hours, use start and end time.
    //If a forecast Value occurs mulitple times but not in sucesssion. use the hour and the Value for each occurance.

    public class ForecastGroup
    {
        public DateTime Time { get; set; }
        public TimeOfDayService.TimeOfDay TimeOfDay { get; set; }
    }

    public static class NumberGroups
    {
        public static string WindsToString(double w)
        {

            return w switch
            {
                >= 0 and <= 1 => "faint breeze",
                >= 1 and <= 3 => "calm breeze",
                >= 4 and <= 7 => "light breeze",
                >= 8 and <= 12 => "gentle breeze",
                >= 13 and <= 18 => "moderate breeze",
                >= 19 and <= 24 => "fresh breeze",
                >= 25 and <= 31 => "strong breeze",
                >= 32 and <= 38 => "near gale force wind",
                >= 39 and <= 46 => "gale force wind",
                >= 47 and <= 54 => "severe gale force wind",
                >= 55 and <= 63 => "storm wind",
                >= 64 and <= 72 => "violent storm wind",
                >= 73 => "hurricane force wind",
                _ => ""
            };

        }

        public static string TempCategory(double temp, double dewPoint, double relativeHumidity, double wbgt)
        {
            var sb = new StringBuilder();
            sb.Append($"The temperature will be {NumberGroups.TempsToString(temp)} with a dew point {NumberGroups.DewPointToString(dewPoint)}. Expect ");
            // --- Extreme heat danger (WBGT-based) ---
            if (wbgt >= 90)
            {
                sb.Append("extreme dangerous heat this hour.");
                return sb.ToString();
            }
            if (wbgt >= 82)
            {
                sb.Append("dangerous heat this hour.");
                return sb.ToString();
            }
            if (wbgt >= 78)
            {
                sb.Append("very hot and humid this hour.");
                return sb.ToString();
            }
            // --- RelativeHumidity / dew point discomfort ---
            bool oppressiveHumidity = dewPoint >= 75 || relativeHumidity >= 85;
            bool veryHumid = dewPoint >= 70 || relativeHumidity >= 75;
            bool humid = dewPoint >= 65 || relativeHumidity >= 60;

            // --- Cold stress ---
            if (temp <= 20)
            {
                sb.Append("frigid conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 32)
            {
                sb.Append("freezing conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 40)
            {
                sb.Append(humid ? "raw and cold conditions this hour." : "cold conditions this hour.");
                return sb.ToString();
            }
            // --- Cool to mild ---
            if (temp <= 50)
            {
                sb.Append(humid ? "chilly and damp conditions this hour." : "cool conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 60)
            {
                sb.Append(humid ? "cool and humid conditions this hour." : "mildly cool conditions this hour.");
                return sb.ToString();
            }
            // --- Mild to warm ---
            if (temp <= 70)
            {
                sb.Append(humid ? "mild but humid conditions this hour." : "mild conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 80)
            {
                if (oppressiveHumidity) sb.Append("warm and oppressive conditions this hour.");
                if (veryHumid) sb.Append("warm and very humid conditions this hour.");
                if (humid) sb.Append("warm and humid conditions this hour.");
                sb.Append("warm conditions this hour.");
                return sb.ToString();
            }

            // --- Hot categories ---
            if (temp <= 90)
            {
                if (oppressiveHumidity) sb.Append("hot and oppressive conditions this hour.");
                if (veryHumid) sb.Append("hot and very humid conditions this hour.");
                if (humid) sb.Append("hot and humid conditions this hour.");
                sb.Append("hot conditions this hour.");
                return sb.ToString();
            }

            // --- Extreme heat (temp-based) ---
            if (temp > 90)
            {
                if (oppressiveHumidity) sb.Append("extremely hot and oppressive conditions this hour.");
                if (veryHumid) sb.Append("extremely hot and very humid conditions this hour.");
                sb.Append("extremely hot conditions this hour.");
                return sb.ToString();
            }

            sb.Append("unknown");
            return sb.ToString();
        }

        public static string DewPointToString(double d)
        {
            string dp = string.Empty;

            if (d < 30) dp = "is less than 30,";
            if (30 <= d && d < 34) dp = "in the low 30's,";
            if (34 <= d && d <= 36) dp = "in the mid 30's,";
            if (37 <= d && d < 40) dp = "in the high 30's";
            if (40 <= d && d < 44) dp = "in the low 40's";
            if (44 <= d && d <= 46) dp = "in the mid 40's,";
            if (47 <= d && d < 50) dp = "in the high 40's,";
            if (50 <= d && d < 54) dp = "in the low 50's";
            if (54 <= d && d <= 56) dp = "in the mid 50's,";
            if (57 <= d && d < 60) dp = "in the high 50's";
            if (60 <= d && d < 64) dp = "in the low 60's,";
            if (64 <= d && d <= 66) dp = "in the mid 60's,";
            if (67 <= d && d < 70) dp = "in the high 60's,";
            if (70 <= d && d < 74) dp = "in the low 70's,";
            if (74 <= d && d < 76) dp = "in the mid 70's,";
            if (76 <= d && d < 80) dp = "in the high 70's,";
            if (d >= 80) dp = "at " + d.ToString() + ",";
            return dp;
        }
        public static string TempsToString(double t)
        {
            string temp = string.Empty;
            if (t < 0) temp = "in the negatives,";
            if (0 <= t && t < 30) temp = "below freezing,";
            if (30 <= t && t < 34) temp = "in the low 30's,";
            if (34 <= t && t <= 36) temp = "in the mid 30's,";
            if (37 <= t && t < 40) temp = "in the high 30's";
            if (40 <= t && t < 44) temp = "in the low 40's";
            if (44 <= t && t <= 46) temp = "in the mid 40's,";
            if (47 <= t && t < 50) temp = "in the high 40's,";
            if (50 <= t && t < 54) temp = "in the low 50's";
            if (54 <= t && t <= 56) temp = "in the mid 50's,";
            if (57 <= t && t < 60) temp = "in the high 50's";
            if (60 <= t && t < 64) temp = "in the low 60's,";
            if (64 <= t && t <= 66) temp = "in the mid 60's,";
            if (67 <= t && t < 70) temp = "in the high 60's";
            if (70 <= t && t < 74) temp = "in the low 70's";
            if (74 <= t && t < 76) temp = "in the mid 70's";
            if (76 <= t && t < 80) temp = "in the high 70's";
            if (80 <= t && t < 84) temp = "in the low 80's";
            if (84 <= t && t < 86) temp = "in the mid 80's";
            if (86 <= t && t < 90) temp = "in the high 80's";
            if (90 <= t && t < 94) temp = "in the low 90's";
            if (94 <= t && t < 96) temp = "in the mid 90's";
            if (98 <= t && t < 100) temp = "in the high 90's";
            if (100 <= t) temp = "in the 100's";

            return temp;
        }

        public static string ThunderToString(double pct)
        {
            return pct switch
            {
                > 75 => " there is an extremely high risk wide spread and intense thunderstorms with strong winds.",
                > 50 and <= 75 => " there is a high risk of thunderstorms and strong winds.",
                > 30 and <= 50 => " there is a moderate risk of thunderstorms and strong winds.",
                > 10 and <= 30 => " there is a slight risk of scattered thunderstorms possible.",
                > 1 and <= 10 => " there is a low chance of thunderstorms.",
                _ => " No thunderstorms expected."
            };
        }
    }
}



//"Birthday of Martin Luther King, Jr."

//Third Monday in January

//Inauguration Day

//January 20, every 4 years following a presidential election

//"Washington's Birthday" 

//Also known as "Presidents Day"

//Third Monday in February

//"Memorial Day"

//Last Monday in May

//"Juneteenth National Independence Day"

//June 19

//"Independence Day"

//July 4

//"Labor Day"

//First Monday in September

//"Columbus Day"

//Second Monday in October

//"Veterans Day"

//November 11

//"Thanksgiving Day"

//Fourth Thursday in November

//"Christmas Day"

//December 25