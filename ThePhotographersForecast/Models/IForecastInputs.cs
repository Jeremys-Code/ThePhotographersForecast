using System.Collections.ObjectModel;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Models
{
    public interface IForecastInputs
    {
        ForecastApparentTemperature ApparentTemperature { get; set; }
        ForecastCeilingHeight CeilingHeight { get; set; }
        string CeilingHeightTrend { get; set; }
        ForecastDewPoint Dewpoint { get; set; }
        DateTime EndTime { get; set; }
        ForecastHeatIndex HeatIndex { get; set; }
        ForecastHeatRisk HeatRisk { get; set; }
        ForecastIceAccumulation IceAccumulation { get; set; }
        ForecastLowOccuranceVisibilityIndex Lovri { get; set; }
        ForecastMaxTemperature MaxTemp { get; set; }
        ForecastMinTemperature MinTemp { get; set; }
        ForecastMixingHeight MixingHeight { get; set; }
        ForecastPrecipitation Precipitation { get; set; }
        string QuantitativePrecipitationTrend { get; set; }
        ForecastRelativeHumidity RelativeHumidity { get; set; }
        ForecastSkyCover SkyCover { get; set; }
        string SkyCoverStatement { get; set; }
        string SkyCoverTrend { get; set; }
        ForecastSnowfallAmount SnowFallAmount { get; set; }
        DateTime StartTime { get; set; }
        string StormWeatherStatement { get; set; }
        ForecastTemperature Temp { get; set; }
        string TempStatement { get; set; }
        string TempTrend { get; set; }
        ForecastThunder Thunder { get; set; }
        string ThunderTrend { get; set; }
        string TimeOfDay { get; }
        ForecastTransportWind TransportWindSpeed { get; set; }
        string TransportWindTrend { get; set; }
        ForecastVisibility Visibility { get; set; }
        string VisibilityTrend { get; set; }
        ObservableCollection<WeatherStatementsValue> Weather { get; set; }
        ForecastWbgt WetBulbGlobeTemp { get; set; }
        ForecastWind Wind { get; set; }
        ForecastWindChill WindChill { get; set; }
        ForecastGusts WindGusts { get; set; }
        string WindGustStatement { get; set; }
        string WindTrend { get; set; }
        string WmoForecast { get; set; }

        ObservableCollection<ForecastInputs> BuildForecastInputs(NwsDailyForecast daily);
    }
}