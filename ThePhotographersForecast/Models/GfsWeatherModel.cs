using System.Collections.ObjectModel;
using static ThePhotographersForecast.Data.GridDetailData;

namespace ThePhotographersForecast.Models
{
    // Translates the weather api model into what is stored in the database
    public class GfsWeatherModel
    {

        public GfsDailyForecast[] DailyForecasts { get; set; }

        public GfsHourlyForecast[] HourlyForecasts { get; set; }
    }
    public class GfsDailyForecast
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Number { get; set; }

        public bool IsEmpty { get; set; }

        public string Name { get; set; }

        public bool IsDaytime { get; set; }
        public double Temperature { get; set; }

        public string TemperatureUnit { get; set; }

        public int ChanceOfPrecipitation { get; set; }

        public string PrecipitationUnitCode { get; set; }

        public string DailyIcon { get; set; }

        public string DetailForecast { get; set; }

        public string WindSpeedDirection { get; set; }

        public ImageSource DailyImageSource { get; set; } = ImageSource.FromFile("unknown.png");

        public GfsHourlyForecast[] HourlyWeathers { get; set; }

    }

    public class GfsHourlyTimeTemp
    {

        public DailyHigh High { get; set; }

        public DailyLow Low { get; set; }
    }

    public class DailyHigh
    {

        public DateTime Time { get; set; }

        public Double Temp { get; set; }
    }
    public class DailyLow
    {

        public DateTime Time { get; set; }

        public Double Temp { get; set; }
    }

    public class GfsHourlyForecast
    {
        public int Number { get; set; }

        public string Name => String.Concat(this.StartTime.ToString("dddd"), " ", Services.GetTimeOfDay.CalcTimeOfDay(this.StartTime.Hour).ToString(), " @ ", this.StartTime.ToString("h tt"));
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TimeOfDay => Services.GetTimeOfDay.CalcTimeOfDay(this.StartTime.Hour).ToString();
        public bool IsDaytime { get; set; }

        public double Temperature { get; set; }

        public string TemperatureUnit { get; set; }

        public int ChanceOfPrecipitation { get; set; }

        public string PrecipitationUnitCode { get; set; }

        public int Dewpoint { get; set; }

        public string DewpointUnit { get; set; }

        public string Humidity { get; set; }

        public string HumidityUnit { get; set; }

        public string WindDirection { get; set; }

        public string WindSpeed { get; set; }

        public string HourlyIcon { get; set; }

        public string ShortForecast { get; set; }

        public string DetailForecast { get; set; }

        public bool IsHigh { get; set; } = false;

        public bool IsLow { get; set; } = false;

        public ImageSource HourlyImageSource { get; set; } = ImageSource.FromFile("unknown.png");

        public string IntroTimeOfDay => FromTimesOfDay(this.StartTime.Hour);

        private string FromTimesOfDay(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 3):
                    return "Good evening";
                case int h when (3 <= h && h < 12):
                    return "Good morning";
                case int h when (12 <= h && h < 18):
                    return "Good afternoon";
                case int h when (18 <= h && h < 22):
                    return "Good evening";
                case int h when (22 <= h && h < 24):
                    return "Good evening";
                default:
                    return string.Empty;
            }
        }

    }

    //Values Needed
    //ApparentTemperature
    //WetBulbGlobeTemperature
    //HeatIndex
    //WindChill
    //SkyCover
    //WindGust
    //Weather
    //HeatRisk
    public class GfsHourlySupplement
    {
        public DateTime Time { get; set; }
        public bool IsDay { get; set; }
        public Measurement MaxTemperature { get; set; }
        public Measurement MinTemperature { get; set; }
        public Measurement RelativeHumidity { get; set; }
        public Measurement Temperature { get; set; }
        public Measurement ApparentTemperature { get; set; }
        public Measurement WetBulbGlobeTemperature { get; set; }
        public Measurement HeatIndex { get; set; }
        public Measurement Skycover { get; set; }
        public Measurement WindChill { get; set; }
        public Measurement WindGust { get; set; }
        public Measurement WindSpeed { get; set; }
        public Measurement2 WindDirection { get; set; }
        public Measurement ProbabilityOfPrecipitation { get; set; }
        public Measurement QuantitativePrecipitation { get; set; }
        public Measurement ProbabilityOfTropicalStormWinds { get; set; }
        public Measurement ProbabilityOfHurricaneWinds { get; set; }
        public Measurement PotentialOf15MphWinds { get; set; }
        public Measurement PotentialOf25MphWinds { get; set; }
        public Measurement PotentialOf35MphWinds { get; set; }
        public Measurement PotentialOf45MphWinds { get; set; }
        public Measurement PotentialOf20MphWindGusts { get; set; }
        public Measurement PotentialOf30MphWindGusts { get; set; }
        public Measurement PotentialOf40MphWindGusts { get; set; }
        public Measurement PotentialOf50MphWindGusts { get; set; }
        public Measurement PotentialOf60MphWindGusts { get; set; }
        public Measurement2 ProbabilityOfThunder { get; set; }
        public string[] Weather { get; set; }
    }

    //public class Measurement
    //{
    //    public double Value { get; set; }
    //    public string Uom { get; set; }
    //}
    //public class Measurement2
    //{
    //    public int Value { get; set; }
    //    public string Uom { get; set; }
    //}
}