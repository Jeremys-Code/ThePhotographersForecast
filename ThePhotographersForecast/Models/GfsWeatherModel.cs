using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Models
{
    // Translates the weather api model into what is stored in the database
    public partial class GfsWeatherModel : ObservableObject
    {
        [ObservableProperty]
        private GfsDailyForecast[] dailyForecasts;
        [ObservableProperty]
        private GfsHourlyTimeTemp[] hourlyTimeTemps;
        [ObservableProperty]
        private GfsHourlyForecast[] hourlyForecasts;
    }
    public partial class GfsDailyForecast : ObservableObject
    {
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private bool _isToday;
        [ObservableProperty]
        private bool _isEmpty;
        [ObservableProperty]
        private bool _isFlipped;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private DateTime startTime;
        [ObservableProperty]
        private DateTime endTime;
        [ObservableProperty]
        private bool isDaytime;
        [ObservableProperty]
        private double dailyHighTemp;
        [ObservableProperty]
        private DateTime dailyHighTime;
        [ObservableProperty]
        private double dailyLowTemp;
        [ObservableProperty]
        private DateTime dailyLowTime;
        [ObservableProperty]
        private double temperature;
        [ObservableProperty]
        private string temperatureUnit;
        [ObservableProperty]
        private int chanceOfPrecipitation;
        [ObservableProperty]
        private string precipitationUnitCode;
        [ObservableProperty]
        private string dailyIcon;
        [ObservableProperty]
        private string detailForecast;
        [ObservableProperty]
        private string windSpeedDirection;
        [ObservableProperty]
        private GfsHourlyForecast[] hourlyForecasts = Array.Empty<GfsHourlyForecast>();
    }

    public partial class GfsHourlyTimeTemp : ObservableObject
    {
        [ObservableProperty]
        private DailyHigh high;
        [ObservableProperty]
        private DailyLow low;
    }

    public partial class DailyHigh : ObservableObject
    {
        [ObservableProperty]
        private DateTime time;
        [ObservableProperty]
        private Double temp;
    }
    public partial class DailyLow : ObservableObject
    {
        [ObservableProperty]
        private DateTime time;
        [ObservableProperty]
        private Double temp;
    }

    public partial class GfsHourlyForecast : ObservableObject
    {
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private DateTime startTime;
        [ObservableProperty]
        private DateTime endTime;
        [ObservableProperty]
        private bool isDaytime;
        [ObservableProperty]
        private double temperature;
        [ObservableProperty]
        private string temperatureUnit;
        [ObservableProperty]
        private int chanceOfPrecipitation;
        [ObservableProperty]
        private string precipitationUnitCode;
        [ObservableProperty]
        private string dewpoint;
        [ObservableProperty]
        private string dewpointUnit;
        [ObservableProperty]
        private string humidity;
        [ObservableProperty]
        private string humidityUnit;
        [ObservableProperty]
        private string windDirection;
        [ObservableProperty]
        private string windSpeed;
        [ObservableProperty]
        private string hourlyIcon;
        [ObservableProperty]
        private string shortForecast;
        [ObservableProperty]
        private string detailForecast;

    }
}
