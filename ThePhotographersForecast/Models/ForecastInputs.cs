using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Models
{
    [Table("HourlyForecastInputs")]
    public partial class ForecastInputs : ObservableObject
    {
        [PrimaryKey, AutoIncrement, Column("ForecastId")]
        public int ForecastId { get; set; }
        public string TimeOfDay => TimeOfDayService.CalcTimeOfDay(StartTime.Hour);
        [ObservableProperty]
        public partial DateTime StartTime { get; set; }
        [ObservableProperty]
        public partial DateTime EndTime { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastShort ShortForecast { get; set; }
        [ObservableProperty]
        public partial string StormWeatherStatement { get; set; }
        //Temperature
        [ObservableProperty]
        [Ignore]
        public partial ForecastTemperature Temp { get; set; }
        [ObservableProperty]
        public partial string TempTrend { get; set; }
        [ObservableProperty]
        public partial string TempStatement { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastApparentTemperature ApparentTemperature { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastDewPoint Dewpoint { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastRelativeHumidity RelativeHumidity { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastWindChill? WindChill { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastHeatIndex? HeatIndex { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastHeatRisk HeatRisk { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastWbgt WetBulbGlobeTemp { get; set; }
        //Wind
        [ObservableProperty]
        [Ignore]
        public partial ForecastWind Wind { get; set; }
        [ObservableProperty]
        public partial string WindTrend { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastGusts? WindGusts { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastTransportWind TransportWind { get; set; }
        [ObservableProperty]
        public partial string WindGustStatement { get; set; }
        //Precipitation
        [Ignore]
        public ObservableCollection<WeatherStatementsValue> Weather { get; set; } = new ObservableCollection<WeatherStatementsValue>();
        [ObservableProperty]
        [Ignore]
        public partial ForecastPrecipitation Precipitation { get; set; }
        [ObservableProperty]
        public partial string QuantitativePrecipitationTrend { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastIceAccumulation? IceAccumulation { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastSnowfallAmount? SnowFallAmount { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastThunder? Thunder { get; set; }
        [ObservableProperty]
        public partial string ThunderTrend { get; set; }
        //Clouds
        [ObservableProperty]
        [Ignore]
        public partial ForecastSkyCover SkyCover { get; set; }
        [ObservableProperty]
        public partial string SkyCoverStatement { get; set; }
        [ObservableProperty]
        public partial string SkyCoverTrend { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastLowOccuranceVisibilityIndex? Lovri { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastVisibility? Visibility { get; set; }
        [ObservableProperty]
        public partial string VisibilityTrend { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastCeilingHeight CeilingHeight { get; set; }
        [ObservableProperty]
        public partial string CeilingHeightTrend { get; set; }
        [ObservableProperty]
        [Ignore]
        public partial ForecastMixingHeight MixingHeight { get; set; }

        public static readonly string[] LayerNames =
        [
            nameof(Temp),
            nameof(ApparentTemperature),
            nameof(Dewpoint),
            nameof(RelativeHumidity),
            nameof(WindChill),
            nameof(HeatIndex),
            nameof(HeatRisk),
            nameof(WetBulbGlobeTemp),
            nameof(WindGusts),
            nameof(IceAccumulation),
            nameof(SnowFallAmount),
            nameof(Thunder),
            nameof(SkyCover),
            nameof(Lovri),
            nameof(Visibility),
            nameof(CeilingHeight),
            nameof(MixingHeight),
        ];

        public (string Name, IForecastLayerValue? Data)[] GetLayers() =>
        [
            (nameof(Temp), Temp),
            (nameof(ApparentTemperature), ApparentTemperature),
            (nameof(Dewpoint), Dewpoint),
            (nameof(RelativeHumidity), RelativeHumidity),
            (nameof(WindChill), WindChill),
            (nameof(HeatIndex), HeatIndex),
            (nameof(HeatRisk), HeatRisk),
            (nameof(WetBulbGlobeTemp), WetBulbGlobeTemp),
            (nameof(WindGusts), WindGusts),
            (nameof(IceAccumulation), IceAccumulation),
            (nameof(SnowFallAmount), SnowFallAmount),
            (nameof(Thunder), Thunder),
            (nameof(SkyCover), SkyCover),
            (nameof(Lovri), Lovri),
            (nameof(Visibility), Visibility),
            (nameof(CeilingHeight), CeilingHeight),
            (nameof(MixingHeight), MixingHeight),
        ];
    }

    public interface IForecastLayerValue
    {
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        double Value { get; }
        string UnitCode { get; }
    }
}
