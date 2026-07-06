using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models
{
    public partial class WeatherForecast : ObservableObject
    {
        [ObservableProperty]
        private DateOnly date;
        [ObservableProperty]
        private double latitude;
        [ObservableProperty]
        private double longitude;
        [ObservableProperty]
        private double generationTimeMs;
        [ObservableProperty]
        private int utcOffsetSeconds;
        [ObservableProperty]
        private string timeZone;
        [ObservableProperty]
        private double elevation;
        [ObservableProperty]
        private DailyForecast dayForecast;
        [ObservableProperty]
        private HourlyForecast[] hourlyForecasts;
        [ObservableProperty]
        private ObservableCollection<EventTime> eventTimes;
    }
    public partial class HourlyForecast : ObservableObject
    {
        public string Name => String.Concat(this.Time.ToString("dddd"), " ", Services.GetTimeOfDay.CalcTimeOfDay(this.Time.Hour), " @ ", this.Time.ToString("h tt"));
        [ObservableProperty]
        private DateTime time;
        public DateOnly Date => DateOnly.FromDateTime(this.Time);
        public string TimeOfDay => String.Concat(Services.GetTimeOfDay.CalcTimeOfDay(this.Time.Hour));
        [ObservableProperty]
        private Measurement temperature2m;
        [ObservableProperty]
        private Measurement dewPoint2m;
        [ObservableProperty]
        private Measurement precipitationProbability;
        [ObservableProperty]
        private Measurement precipitation;
        [ObservableProperty]
        private Measurement rain;
        [ObservableProperty]
        private Measurement showers;
        [ObservableProperty]
        private Measurement snowfall;
        [ObservableProperty]
        private Measurement snowDepth;
        [ObservableProperty]
        private int weatherCode;
        [ObservableProperty]
        private string weatherCodeDescription;
        [ObservableProperty]
        private ImageSource hourlyIcon;
        [ObservableProperty]
        private Measurement cloudCover;
        [ObservableProperty]
        private Measurement visibility;
        [ObservableProperty]
        private Measurement windSpeed10m;
        [ObservableProperty]
        private Measurement2 windDirection10m;
        [ObservableProperty]
        private Measurement windGusts10m;
        [ObservableProperty]
        private int isDay;
        [ObservableProperty]
        private Measurement sunshineDuration;
        [ObservableProperty]
        private Measurement relativeHumidity2m;
        [ObservableProperty]
        private Measurement apparentTemperature;
        [ObservableProperty]
        private Measurement uvIndex;
        [ObservableProperty]
        private Measurement wetBulbTemperature2m;
        [ObservableProperty]
        private string weatherSummary;
        [ObservableProperty]
        private string weatherImage;
        [ObservableProperty]
        private string healthSafetySummary;
        [ObservableProperty]
        private string precipitationSummary;
    }
    public partial class Measurement : ObservableObject
    {
        [ObservableProperty]
        private double value;
        [ObservableProperty]
        private string unit;
    }
    public partial class Measurement2 : ObservableObject
    {
        [ObservableProperty]
        private int value;
        [ObservableProperty]
        private string description;
        [ObservableProperty]
        private string unit;
    }
    public partial class DailyForecast : ObservableObject
    {
        public string TimePeriod => String.Concat(this.Date.ToString("dddd, MMMM dd, yyyy"));
        [ObservableProperty]
        private DateOnly date;
        [ObservableProperty]
        private DateTime sunrise;
        [ObservableProperty]
        private DateTime sunset;
        [ObservableProperty]
        private Measurement dayLightDuration;
        [ObservableProperty]
        private Measurement sunshineDuration;
        [ObservableProperty]
        private int weatherCode;
        [ObservableProperty]
        private string weatherCodeDescription;
        [ObservableProperty]
        private ImageSource dailyIcon;
        [ObservableProperty]
        private Measurement temperature2mMax;
        [ObservableProperty]
        private Measurement temperature2mMin;
        [ObservableProperty]
        private Measurement windSpeed10mMax;
        [ObservableProperty]
        private Measurement windGusts10mMax;
        [ObservableProperty]
        private Measurement2 windDirection10mDominant;
        [ObservableProperty]
        private Measurement precipitationProbabilityMax;
        [ObservableProperty]
        private Measurement precipitationProbabilityMin;
        [ObservableProperty]
        private Measurement rainSum;
        [ObservableProperty]
        private Measurement showersSum;
        [ObservableProperty]
        private Measurement snowfallSum;
        [ObservableProperty]
        private Measurement precipitationSum;
        [ObservableProperty]
        private Measurement dewPoint2mMean;
        [ObservableProperty]
        private Measurement apparentTemperatureMax;
        [ObservableProperty]
        private Measurement apparentTemperatureMin;
        [ObservableProperty]
        private Measurement uvIndexMax;
        [ObservableProperty]
        private Measurement wetBulbTemperature2mMin;
        [ObservableProperty]
        private Measurement wetBulbTemperature2mMax;
        [ObservableProperty]
        private string weatherSummary;
        [ObservableProperty]
        private string healthSafetySummary;
        [ObservableProperty]
        private string precipitationSummary;
        [ObservableProperty]
        private EventModel dailyEvents;
        //[ObservableProperty]
        //private DailyAstronomicalData astronomicalData;
        [ObservableProperty]
        private ObservableCollection<HourlyForecast> hourlyForecasts;
    }
}