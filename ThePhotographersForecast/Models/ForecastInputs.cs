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
        public partial string WmoForecast { get; set; }
        [ObservableProperty]
        public partial string StormWeatherStatement { get; set; }
        //Temperature
        [ObservableProperty]
        public partial ForecastTemperature Temp { get; set; }
        [ObservableProperty]
        public partial string TempTrend { get; set; }
        [ObservableProperty]
        public partial string TempStatement { get; set; }
        [ObservableProperty]
        public partial ForecastApparentTemperature ApparentTemperature { get; set; }
        [ObservableProperty]
        public partial ForecastDewPoint Dewpoint { get; set; }
        [ObservableProperty]
        public partial ForecastRelativeHumidity RelativeHumidity { get; set; }
        [ObservableProperty]
        public partial ForecastWindChill? WindChill { get; set; }
        [ObservableProperty]
        public partial ForecastHeatIndex? HeatIndex { get; set; }
        [ObservableProperty]
        public partial ForecastHeatRisk HeatRisk { get; set; }
        [ObservableProperty]
        public partial ForecastWbgt WetBulbGlobeTemp { get; set; }
        //Wind
        [ObservableProperty]
        public partial ForecastWind Wind { get; set; }
        [ObservableProperty]
        public partial string WindTrend { get; set; }
        [ObservableProperty]
        public partial ForecastGusts? WindGusts { get; set; }
        [ObservableProperty]
        public partial ForecastTransportWind TransportWind { get; set; }
        [ObservableProperty]
        public partial string WindGustStatement { get; set; }
        //Precipitation
        public ObservableCollection<WeatherStatementsValue> Weather { get; set; } = new ObservableCollection<WeatherStatementsValue>();
        [ObservableProperty]
        public partial ForecastPrecipitation Precipitation { get; set; }
        [ObservableProperty]
        public partial string QuantitativePrecipitationTrend { get; set; }
        [ObservableProperty]
        public partial ForecastIceAccumulation? IceAccumulation { get; set; }
        [ObservableProperty]
        public partial ForecastSnowfallAmount? SnowFallAmount { get; set; }
        [ObservableProperty]
        public partial ForecastThunder? Thunder { get; set; }
        [ObservableProperty]
        public partial string ThunderTrend { get; set; }
        //Clouds
        [ObservableProperty]
        public partial ForecastSkyCover SkyCover { get; set; }
        [ObservableProperty]
        public partial string SkyCoverStatement { get; set; }
        [ObservableProperty]
        public partial string SkyCoverTrend { get; set; }
        [ObservableProperty]
        public partial ForecastLowOccuranceVisibilityIndex? Lovri { get; set; }
        [ObservableProperty]
        public partial ForecastVisibility? Visibility { get; set; }
        [ObservableProperty]
        public partial string VisibilityTrend { get; set; }
        [ObservableProperty]
        public partial ForecastCeilingHeight CeilingHeight { get; set; }
        [ObservableProperty]
        public partial string CeilingHeightTrend { get; set; }
        [ObservableProperty]
        public partial ForecastMixingHeight MixingHeight { get; set; }
    }
}
