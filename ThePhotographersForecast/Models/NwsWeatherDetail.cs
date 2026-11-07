using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models
{
    public class NwsWeatherDetail
    {
        public string TimeOfDay => Services.TimeOfDayService.CalcTimeOfDay(this.StartTime.Hour);

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public WeatherLayer MaxWeatherTime { get; set; }
        public WeatherLayer MinWeatherTime { get; set; }
        public TimeSpan DurationOfWeather { get; set; }
        public Trend PriorTimeToTimeTrend { get; set; }
        public string WeatherSummary { get; set; }
        public ObservableCollection<WeatherLayer> WeatherLayerCollection { get; set; }


        public class WeatherLayer
        {
            public double Attribute { get; set; }
            public string UnitCode { get; set; }
            public DateTime Time { get; set; }


        }

        public enum Trend
        {
            steady,
            increase,
            decrease
        }

        public enum Intensity
        {
            Mostly,
            Partly,
            Light,
            Heavy,

        }
        public enum Attribute
        {
            // Sky Conditions
            Sunny,
            MostlySunny,
            PartlySunny,
            Clear,
            MostlyClear,
            PartlyCloudy,
            MostlyCloudy,
            Cloudy,
            IncreasingClouds,
            DecreasingClouds,
            BecomingSunny,
            BecomingCloudy,
            GradualClearing,
            ClearingLate,

            // Rain / Showers / Drizzle
            Rain,
            LightRain,
            HeavyRain,
            Showers,
            RainShowers,
            Drizzle,
            Sprinkles,
            IsolatedRain,
            IsolatedShowers,
            IsolatedDrizzle,
            SlightChanceRain,
            SlightChanceShowers,
            SlightChanceDrizzle,
            ScatteredRain,
            ScatteredShowers,
            ScatteredDrizzle,
            AreasRain,
            AreasShowers,
            AreasDrizzle,
            ChanceRain,
            ChanceShowers,
            ChanceDrizzle,
            RainLikely,
            ShowersLikely,
            DrizzleLikely,

            // Thunderstorms
            Thunderstorms,
            SevereThunderstorms,
            IsolatedThunderstorms,
            SlightChanceThunderstorms,
            ScatteredThunderstorms,
            AreasThunderstorms,
            ChanceThunderstorms,
            ThunderstormsLikely,

            // Snow / Wintry Mix
            Snow,
            LightSnow,
            HeavySnow,
            SnowShowers,
            FreezingRain,
            FreezingDrizzle,
            Sleet,
            WintryMix,
            IsolatedSnow,
            SlightChanceSnow,
            ScatteredSnow,
            AreasSnow,
            ChanceSnow,
            SnowLikely,

            // Fog / Visibility
            Fog,
            DenseFog,
            FreezingFog,
            DenseFreezingFog,
            PatchyFog,
            PatchyFreezingFog,
            //When Low Visibility Occurance Risk Index (Visibility Index) is at 8,9, or 10 and fog mixes with smoke it creates Super Fog. 
            //This is a fire condition.
            SuperFog,
            AreasFog,
            AreasFreezingFog,
            IsolatedFog,
            IsolatedFreezingFog,

            // Wind / Temperature
            Breezy,
            Windy,
            VeryWindy,
            Blustery,
            Hot,
            Cold,

            // Obstructions
            Haze,
            Smoke,
            BlowingDust,
            BlowingSnow,
            PatchyHaze,
            AreasHaze,
            IsolatedHaze,
            PatchySmoke,
            AreasSmoke,
            IsolatedSmoke,

            // Tropical / Hurricane
            TropicalStormConditions,
            TropicalStormConditionsPossible,
            TropicalStormConditionsExpected,
            TropicalStormOrHurricaneConditionsPossible,
            HurricaneConditionsPossible,
            HurricaneConditionsExpected,

            // Frost
            Frost,
            PatchyFrost,
            AreasFrost,
            MorningFrost
        }

        public enum Coverage
        {
            areas,
            brief,
            chance,
            definite,
            few,
            frequent,
            intermittent,
            isolated,
            likely,
            numerous,
            occasional,
            patchy,
            periods,
            scattered,
            slight_chance,
            widespread
        }
       

    }
}