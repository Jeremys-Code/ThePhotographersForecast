using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models
{
    public partial class WeatherForecast : ObservableObject
    {
        [ObservableProperty]
        public partial DailyForecast[] DailyForecasts { get; set; }
        [ObservableProperty]
        public partial HourlyForecast[] HourlyForecasts { get; set; }
        public WeatherForecast()
        {
            this.DailyForecasts = Array.Empty<DailyForecast>();
            this.HourlyForecasts = Array.Empty<HourlyForecast>();
        }
        public WeatherForecast(WeatherForecast weather)
        {
            if (weather == null)
                throw new ArgumentNullException(nameof(weather), "Source object cannot be null.");

            this.DailyForecasts = weather.DailyForecasts.ToArray();
            this.HourlyForecasts = weather.HourlyForecasts.ToArray();
        }
    }
    public class HourlyForecast
    {
        public string Name => String.Concat(this.HourlyTime.ToString("dddd"), " ", Services.TimeOfDayService.CalcTimeOfDay(this.HourlyTime.Hour), " @ ", this.HourlyTime.ToString("h tt"));
        public DateTime HourlyTime { get; set; }
        public DateTime Date => this.HourlyTime;
        public string TimeOfDay => String.Concat(Services.TimeOfDayService.CalcTimeOfDay(this.HourlyTime.Hour));
        public DoubleUnitValue Temperature2m { get; set; }
        public DoubleUnitValue DewPoint2m { get; set; }
        public DoubleUnitValue PrecipitationProbability { get; set; }
        public DoubleUnitValue Precipitation { get; set; }
        public DoubleUnitValue Rain { get; set; }
        public DoubleUnitValue Showers { get; set; }
        public DoubleUnitValue Snowfall { get; set; }
        public DoubleUnitValue SnowDepth { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherCodeDescription { get; set; }
        public ImageSource HourlyIcon { get; set; }
        public SkyCoverValue CloudCover { get; set; }
        public DoubleUnitValue DistanceVisible { get; set; }
        public DoubleUnitValue WindSpeed10m { get; set; }
        public IntUnitValue WindDirection10m { get; set; }
        public DoubleUnitValue WindGusts10m { get; set; }
        public int IsDay { get; set; }
        public DoubleUnitValue SunshineDuration { get; set; }
        public DoubleUnitValue RelativeHumidity2m { get; set; }
        public DoubleUnitValue ApparentTemperature { get; set; }
        public DoubleUnitValue UvIndex { get; set; }
        public DoubleUnitValue WetBulbTemperature2m { get; set; }
        public string WeatherImage { get; set; }
        public SKFileLottieImageSource SkHourlyMeteocon { get; set; }
        public ForecastSummaries HourlyForecastSummary { get; set; }
    }
    public class DoubleUnitValue
    {
        public double Value { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
    }
    public class IntUnitValue
    {
        public int Value { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
    }
    public class StringUnitValue
    {
        public string Value { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
    }
    public class DailyForecast
    {
        public string TimePeriod => String.Concat(this.Date.ToString("dddd, MMMM dd, yyyy"));
        public DateOnly Date { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public DoubleUnitValue DayLightDuration { get; set; }
        public DoubleUnitValue SunshineDuration { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherCodeDescription { get; set; }
        public ImageSource DailyIcon { get; set; }
        public DoubleUnitValue Temperature2mMax { get; set; }
        public string TimeTempMax { get; set; }
        public DoubleUnitValue Temperature2mMin { get; set; }
        public string TimeTempMin { get; set; }
        public DoubleUnitValue WindSpeed10mMax { get; set; }
        public DoubleUnitValue WindGusts10mMax { get; set; }
        public IntUnitValue WindDirection10mDominant { get; set; }
        public DoubleUnitValue PrecipitationProbabilityMax { get; set; }
        public DoubleUnitValue PrecipitationProbabilityMin { get; set; }
        public DoubleUnitValue RainSum { get; set; }
        public DoubleUnitValue ShowersSum { get; set; }
        public DoubleUnitValue SnowfallSum { get; set; }
        public DoubleUnitValue PrecipitationSum { get; set; }
        public DoubleUnitValue DewPoint2mMean { get; set; }
        public DoubleUnitValue ApparentTemperatureMax { get; set; }
        public DoubleUnitValue ApparentTemperatureMin { get; set; }
        public DoubleUnitValue UvIndexMax { get; set; }
        public DoubleUnitValue WetBulbTemperature2mMin { get; set; }
        public DoubleUnitValue WetBulbTemperature2mMax { get; set; }
        public EventModel DailyEvents { get; set; } = new EventModel();
        public HourlyForecast[] HourlyForecasts { get; set; }
        public SKFileLottieImageSource SkDailyMeteocon { get; set; }
        public ForecastSummaries DailyForecastSummary { get; set; }
        public SKFileLottieImageSource SkUvIndexIcon => GetSkUvIndex();
        private SKFileLottieImageSource GetSkUvIndex()
        {

            var lottieFile =
            this.UvIndexMax.Value switch
            {
                1 => "uv_index_1.json",
                2 => "uv_index_2.json",
                3 => "uv_index_3.json",
                4 => "uv_index_4.json",
                5 => "uv_index_5.json",
                6 => "uv_index_6.json",
                7 => "uv_index_7.json",
                8 => "uv_index_8.json",
                9 => "uv_index_9.json",
                10 => "uv_index_10.json",
                11 => "uv_index_11.json",
                > 11 => "uv_index_11_plus.json",
                _ => "uv_index.json",
            };

            var animation = new SKFileLottieImageSource { File = lottieFile };


            return animation;

        }
    }
    public class ForecastSummaries
    {
        public DateTime Time { get; set; }
        public bool IsDaily { get; set; }
        public string WeatherSummary { get; set; } = string.Empty;
        public string HealthSafetySummary { get; set; } = string.Empty;
        public string StormSummary { get; set; } = string.Empty;
        public string DailyHighAndTime { get; set; }
        public string DailyLowAndTime { get; set; }
        public Color UVIndexColor
        {
            get => field;
            set
            {
                field = value;
            }
        } = Colors.Green;
    }
}