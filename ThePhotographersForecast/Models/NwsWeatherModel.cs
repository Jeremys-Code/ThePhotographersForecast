using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.ML;

namespace ThePhotographersForecast.Models;

// Translates the weather api model into what is stored in the database
// In order to make a day one object instead of several parts from various files ( <6 am, 6am - 6 pm, >6pm)
// The National Weather Service Day Model will be based off off the hourly and supplemental.
// Step 1 - Process all the hourly and supplemental information
// Step 2 - Add the supplemental information to the hourly so all the weather info is in one object for each hour.
// Step 3 - Group the hours into dates
// Step 4 - Group the hours into TimeOfDay or some interval because one and done isn't right.
/// </summary>
// Step 4 - Weather briefings are ShortForecast, (Instead of temperature use Temperature Category), wind speed, and gusts.
// 
public partial class NwsWeatherModel : ObservableObject
{
    [ObservableProperty]
    public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();
    [ObservableProperty]
    public partial TimeZoneInfo Timezone { get; set; }
    [ObservableProperty]
    public partial int TimezoneOffsetSeconds { get; set; }

    public double HourlyTemperatureChangeRate { get; set; }
    [ObservableProperty]
    public partial NwsDailyForecast[] DailyForecasts { get; set; } = Array.Empty<NwsDailyForecast>();
    [ObservableProperty]
    public partial NwsHourlyForecast[] HourlyForecasts { get; set; } = Array.Empty<NwsHourlyForecast>();
    [ObservableProperty]
    public partial NwsHourlySupplemental[] HourlySupplementals { get; set; } = Array.Empty<NwsHourlySupplemental>();
    [ObservableProperty]
    public partial NwsTimeOfDayForecast[]? TimeOfDayForecasts { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<WeatherStatementsValue> StormSummary { get; set; } = new ObservableCollection<WeatherStatementsValue>();


    public NwsWeatherModel()
    {

    }
    public NwsWeatherModel(NwsWeatherModel _nwsData)
    {

        if (_nwsData == null)
            throw new ArgumentNullException(nameof(_nwsData), "Source object cannot be null.");

        SelectedLocation = _nwsData.SelectedLocation;
        Timezone = _nwsData.Timezone;
        TimezoneOffsetSeconds = _nwsData.TimezoneOffsetSeconds;
        DailyForecasts = new NwsDailyForecast[_nwsData.DailyForecasts.Length];
        for (int i = 0; i < _nwsData.DailyForecasts.Length; i++) { DailyForecasts[i] = _nwsData.DailyForecasts[i]; }
        ;
        HourlyForecasts = new NwsHourlyForecast[_nwsData.HourlyForecasts.Length];
        for (int h = 0; h < _nwsData.HourlyForecasts.Length; h++) { HourlyForecasts[h] = _nwsData.HourlyForecasts[h]; }
        ;
        HourlySupplementals = new NwsHourlySupplemental[_nwsData.HourlySupplementals.Length];
        for (int h = 0; h < _nwsData.HourlySupplementals.Length; h++) { HourlySupplementals[h] = _nwsData.HourlySupplementals[h]; }
        if (_nwsData.TimeOfDayForecasts != null)
        {
            TimeOfDayForecasts = _nwsData.TimeOfDayForecasts != null ? _nwsData.TimeOfDayForecasts.ToArray() : Array.Empty<NwsTimeOfDayForecast>();
        }

    }
}
public class NwsDailyForecast
{
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Number { get; set; }

    public bool IsEmpty { get; set; }

    public string Name { get; set; }

    public bool IsDaytime { get; set; }

    public double Temperature
    {
        get => field;
        set
        {
            field = (int)Math.Round(value * 1.8 + 32);
        }
    }
    public string TemperatureUnit => "°F";

    public string TempAndUnit => GetTempString();
    public double ChanceOfPrecipitation { get; set; }

    public string PrecipitationUnitCode { get; set; }

    public string DailyIcon { get; set; }

    public string ShortForecast { get; set; }

    public string DetailForecast { get; set; }

    public DailyWindSpeedValue WindSpeed { get; set; }

    public string WindDirection { get; set; }
    public WindGustValue? WindGusts { get; set; }
    public string WindSpeedStatement { get; set; }

    public ImageSource DailyImageSource { get; set; } = ImageSource.FromFile("unknown.png");

    public SKFileLottieImageSource SkDailyAnimation { get; set; }

    public string HighTemperature { get; set; }
    public string LowTemperature { get; set; }

    public NwsHourlyForecast[] HourlyWeathers { get; set; }
    public NwsTimeOfDayForecast[] TimeOfDayForecasts { get; set; }
    public NwsHourlySupplemental[] SupplementalData { get; set; }
    public LinearGradientBrush SkyColor { get; set; }
    public DailyForecastSummary DailyScoring { get; set; }
    public ObservableCollection<EventTime> Events { get; set; } = new ObservableCollection<EventTime>();
    public string EnrichedForecast { get; set; }
    public Color TimeOfDayTextColor => TimeOfDayService.GetTimeOfDayTextColor(this.StartTime.Hour);
    public LinearGradientBrush DailyBackgroundBrush => GetGradient();
    private LinearGradientBrush GetGradient()
    {
        if (this.IsDaytime)
        {
            return Application.Current.Resources["GpsBackground"] as LinearGradientBrush;
        }
        else
        {
            return Application.Current.Resources["GpsCircleBorder"] as LinearGradientBrush;
        }
    }
    private string GetTempString()
    {
        return $"{this.Temperature}{this.TemperatureUnit}";
    }

    //private string GetHighLowStatement()
    //{
    //    if (this.IsDaytime == true)
    //    {
    //        var hiTemp = this.HourlyWeathers.Where(d => d.IsDaytime == true).MaxBy(h => h.Temperature);
    //        return $"Temperature up to {hiTemp.Temperature} at {hiTemp.StartTime:h tt}.";
    //    }
    //    else
    //    {
    //        var lowTemp = this.HourlyWeathers.Where(d => d.IsDaytime == false).MinBy(l => l.Temperature);
    //        return $"Temperature down to {lowTemp.Temperature} at {lowTemp.StartTime:h tt}.";
    //    }
    //}



}
public partial class NwsHourlyForecast : ObservableObject
{
    public int Number { get; set; }
    public string Name => String.Concat(this.StartTime.ToString("dddd"), " ", Services.TimeOfDayService.CalcTimeOfDay(this.StartTime.Hour).ToString(), " @ ", this.StartTime.ToString("h tt"));
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsDaytime { get; set; }
    public TemperatureHourly Temperature { get; set; }
    public ProbabilityOfPrecipitationHourly ProbabilityOfPrecipitation { get; set; }
    public DewPointHourly DewPoint { get; set; }
    public RelativeHumidityHourly RelativeHumidity { get; set; }
    public WindSpeedHourly WindSpeed { get; set; }
    public WindGustHourly? WindGust { get; set; }
    public string WindDirection { get; set; }
    public string Icon { get; set; }
    public string ShortForecast { get; set; }
    public string DetailedForecast { get; set; }
    public LinearGradientBrush TimeOfDayBrush => TimeOfDayService.GetTimeOfDayBackground(this.StartTime.Hour);
    public Color TimeOfDayTextColor => TimeOfDayService.GetTimeOfDayTextColor(this.StartTime.Hour);
    public string TimeOfDay => TimeOfDayService.CalcTimeOfDay(this.StartTime.Hour).ToString();
    public string TempAndUnit => GetTempString();
    public bool IsHigh { get; set; } = false;
    public bool IsLow { get; set; } = false;
    private bool _isBestPhotoOrDroneHour;
    public bool IsBestPhotoOrDroneHour
    {
        get => _isBestPhotoOrDroneHour;
        set
        {
            if (SetProperty(ref _isBestPhotoOrDroneHour, value))
            {
                OnPropertyChanged(nameof(HourlyBorderBrush));
                OnPropertyChanged(nameof(HourlyBorderThickness));
            }
        }
    }

    private string _bestHourBadgeText = string.Empty;
    public string BestHourBadgeText
    {
        get => _bestHourBadgeText;
        set => SetProperty(ref _bestHourBadgeText, value);
    }

    private string _bestHourReason = string.Empty;
    public string BestHourReason
    {
        get => _bestHourReason;
        set => SetProperty(ref _bestHourReason, value);
    }

    public Brush HourlyBorderBrush => IsBestPhotoOrDroneHour
        ? Brush.LimeGreen
        : (Application.Current?.Resources["Noon"] as Brush ?? Brush.DeepSkyBlue);
    public double HourlyBorderThickness => IsBestPhotoOrDroneHour ? 5 : 4;
    public NwsHourlySupplemental Supplemental { get; set; }
    public SKFileLottieImageSource SkHourlyAnimation { get; set; }
    public string WindStatement => BuildWindStatement();
    public string IntroTimeOfDay => TimeOfDayService.TimesOfDayGreeting(this.StartTime.Hour);
    private string GetTempString()
    {
        return $"{this.Temperature.Value}{this.Temperature.UnitCode}";
    }

    public class TemperatureHourly
    {
        public string UnitCode => "°F";
        public double Value
        {
            get => field;
            set
            {
                field = (int)Math.Round(value * 1.8 + 32);
            }
        }
    }

    public class ProbabilityOfPrecipitationHourly
    {
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    public class DewPointHourly
    {
        public string UnitCode { get; set; }
        public double Value
        {
            get => field;
            set
            {
                field = Math.Round(value * 1.8 + 32);
            }
        }
    }

    public class RelativeHumidityHourly
    {
        public string UnitCode => "%";
        public double Value { get; set; }
    }

    public class WindSpeedHourly
    {
        public string UnitCode => "mph";
        public double Value
        {
            get => field;
            set
            {
                field = Math.Round((value / 1.609344), 2);
            }
        }
    }

    public class WindGustHourly
    {
        public string GustStatement => $", and gusts up to {Value}{UnitCode}";
        public string UnitCode => "mph";
        public double Value
        {
            get => field;
            set
            {
                field = Math.Round((value / 1.609344), 2);
            }
        }
    }

    private string BuildWindStatement()
    {
        var wind = this.WindDirection.Substring(0, 1).ToUpperInvariant() + this.WindDirection.Substring(1, this.WindDirection.Length - 1).ToLowerInvariant();
        string intensity = this.WindSpeed.Value switch
        {
            >= 0 and <= 1 => "calm winds",
            >= 1 and <= 3 => "light airy winds",
            >= 4 and <= 7 => "light winds",
            >= 8 and <= 12 => "gentle winds",
            >= 13 and <= 18 => "moderate winds",
            >= 19 and <= 24 => "steady winds",
            >= 25 and <= 31 => "strong winds",
            >= 32 and <= 38 => "near gale force winds",
            >= 39 and <= 46 => "gale force winds",
            >= 47 and <= 54 => "severe gale force winds",
            >= 55 and <= 63 => "storm winds",
            >= 64 and <= 72 => "violent storm winds",
            >= 73 => "hurricane force winds",
            _ => null
        };
        var wb = new StringBuilder();
        wb.Append($"There are {intensity} coming from the {this.WindDirection} at {this.WindSpeed.Value}{this.WindSpeed.UnitCode}");
        if (this.WindGust != null && this.WindGust.Value > 0)
        {
            wb.Append($", and wind gusts up to {this.WindGust.Value}{this.WindGust.UnitCode}.");
            return wb.ToString();
        }
        wb.Append(".");
        return wb.ToString();
    }
}

public class NwsFullDayForecast
{
    public DateOnly Date { get; set; }
    public HoursGroups Hours { get; set; }
    public TimeOfDayGroup TimesOfDay { get; set; }
    public string DayHigh { get; set; }
    public string DayLow { get; set; }
    public string Forecasts { get; set; }
    public string Name { get; set; }
}

public class NwsTimeOfDayForecast
{
    public NwsHourlyForecast[] Forecasts { get; set; }
    public ForecastInputs[] ForecastInputs { get; set; }
    public string DescForecasts { get; set; }
    public string LongForecast { get; set; }
    public bool IsDay { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string High { get; set; }
    public string Low { get; set; }
    public double MaxTemp { get; set; }
    public double MinTemp { get; set; }
    public string TempStatement => MinTemp != MaxTemp ? $"{this.MinTemp}°F to {this.MaxTemp}°F" : $"{this.MaxTemp}°F";
    public double DewPointAvg { get; set; }
    public double RelativeHumidityAvg { get; set; }
    public double MaxProbabilityOfPrecipitation { get; set; }
    public double QuantitativePrecipitation { get; set; }
    public double MinWindSpeed { get; set; }
    public string MinWindDirection { get; set; }
    public double MaxWindSpeed { get; set; }
    public string MaxWindDirection { get; set; }
    public double MinWindGusts { get; set; }
    public double MaxWindGusts { get; set; }
    // Time of day name
    public string TimeOfDay { get; set; }
    public LinearGradientBrush TimeOfDayGradientBrush { get; set; }
    public SKFileLottieImageSource SKTimeOfDayAnimation { get; set; }
    public ObservableCollection<LottieAnimation> SkLottieCollection { get; set; }
    public ObservableCollection<EventTime> Events { get; set; }
    public Color TimeOfDayTextColor => TimeOfDayService.GetTimeOfDayTextColor(StartTime.Hour);
    public string TimeOfDayName => $@"{this.StartTime.ToString("dddd")} {this.TimeOfDay}, {this.StartTime.ToString("MMMM dd")} ({this.StartTime.ToString("h tt")} - {this.EndTime.ToString("h tt")}:";
}
// 1 Create a day from time of day group
public class TimeOfDayGroup : ObservableCollection<NwsTimeOfDayForecast>
{
    public DateOnly Date { get; set; }
    public string TimeOfDay { get; set; }

    public TimeOfDayGroup(DateOnly date, string timeOfDay, ObservableCollection<NwsTimeOfDayForecast> timeOfDayForecasts) : base(timeOfDayForecasts)
    {
        Date = date;
        timeOfDay = timeOfDay;
    }
    public ObservableCollection<TimeOfDayGroup> TimeOfDayForecasts { get; set; } = new ObservableCollection<TimeOfDayGroup>();
}
// 2.Create a day from hourly group
public class HoursGroups : ObservableCollection<NwsHourlyForecast>
{
    public DateOnly Date { get; set; }
    public string TimeOfDay { get; set; }
    public HoursGroups(DateOnly date, string timeOfDay, ObservableCollection<NwsHourlyForecast> hourlyForecasts) : base(hourlyForecasts)
    {
        Date = date;
        TimeOfDay = timeOfDay;

    }
}
//public partial class ForecastLayers : ObservableObject
//{
//    [ObservableProperty]
//    public partial string LayerName { get; set; }
//    [ObservableProperty]
//    public partial string Statement { get; set; }
//    [ObservableProperty]
//    public partial TimeAndQuantity Mean { get; set; }
//    [ObservableProperty]
//    public partial TimeAndQuantity Min { get; set; }
//    [ObservableProperty]
//    public partial TimeAndQuantity Max { get; set; }
//    [ObservableProperty]
//    public partial double Total { get; set; }
//    [ObservableProperty]
//    public partial TimeAndQuantity BeginStorm { get; set; }
//    [ObservableProperty]
//    public partial TimeAndQuantity EndStorm { get; set; }
//    [ObservableProperty]
//    public partial int Duration { get; set; }
//}

public class TimeAndQuantity
{
    public double Quantity { get; set; }
    public string Intensity { get; set; }
    public DateTime Time { get; set; }
}

public class ForecastItem
{
    public int Order { get; set; }
    public string Statement { get; set; }
    public string Condition { get; set; }
    public double Thunder { get; set; }
    public double Precipitation { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class NwsHourlySupplemental
{
    public DateTime Time { get; set; }
    public string[] Weathers { get; set; }
    public string SupplementalWeatherStatement { get; set; }
    public string TimeOfDay => Services.TimeOfDayService.CalcTimeOfDay(this.Time.Hour);
    public TemperatureValue? Temperature { get; set; }
    public ApparentTemperatureValue? ApparentTemperature { get; set; }
    public WetBulbGlobeTemperatureValue? WetBulbGlobeTemperature { get; set; }
    //Heatindex is a temperature
    public HeatIndexValue? HeatIndex { get; set; }
    // Heatrisk is a category 0-4
    public HeatRiskValue? HeatRisk { get; set; }
    public SkyCoverValue? Skycover { get; set; }
    public WindchillValue? WindChill { get; set; }
    public WindDirectionValue? WindDirection { get; set; }
    public WindSpeedValue? WindSpeed { get; set; }
    public WindGustValue? WindGust { get; set; }
    public WeatherStatementsValue?[] Weather { get; set; }
    public MinTemperatureValue? MinTemperature { get; set; }
    public MaxTemperatureValue? MaxTemperature { get; set; }
    public RelativeHumidityValue? RelativeHumidity { get; set; }
    public DewPointValue? DewPoint { get; set; }
    public QuantitativePrecipitationValue? QuantitativePrecipitation { get; set; }
    public ProbabilityOfThunderValue? ProbabilityOfThunder { get; set; }
    public LowVisibilityOccurrenceRiskIndexValue? LowVisibilityOccurrenceRiskIndex { get; set; }
    public IceAccumulationValue? IceAccumulation { get; set; }
    public SnowFallAmountValue? SnowfallAmount { get; set; }
    public ProbabilityOfPrecipitationValue? ProbabilityOfPrecipitation { get; set; }
    public MixingHeightValue? MixingHeight { get; set; }
    public CeilingHeightValue? CeilingHeight { get; set; }
    public TransportWindSpeedValue? TransportWindSpeed { get; set; }
    public TransportWindDirectionValue? TransportWindDirection { get; set; }
    public VisibilitiesValue? Visibility { get; set; }
    public Color TimeOfDayTextColor => TimeOfDayService.GetTimeOfDayTextColor(this.Time.Hour);

}

public class NwsLayer
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class TemperatureValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }
}

public class DewPointValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }
}

public class MaxTemperatureValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }
}
public class MinTemperatureValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }
}

public class RelativeHumidityValue
{
    public int Duration { get; set; }
    public string UnitCode => "%";
    public double Value { get; set; }
}

public class ApparentTemperatureValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }
}

public class WetBulbGlobeTemperatureValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }

}

public class HeatIndexValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {

            field = Math.Round(value != null ? value : 0 * 1.8 + 32);
        }
    }
}

public class WindchillValue
{
    public int Duration { get; set; }
    public string UnitCode => "°F";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 1.8 + 32);
        }
    }
}

public class SkyCoverValue
{
    public int Duration { get; set; }
    public string UnitCode => "%";
    public double Value { get; set; }
    public DateTime Time { get; set; }
    public string Description => GetSkyCoverDescription();
    public string Trend { get; set; }
    private string GetSkyCoverDescription()
    {
        string desc = string.Empty;
        if (this.Value is double s)
        {
            if (s < 10) desc = $"Look up to the sky and see a clear{(this.Time.Hour >= 6 || this.Time.Hour < 18 ? " sunny day" : " stary night")}.";
            if (10 <= s && s < 30) desc = $"Look up to the sky and see a mostly {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "sunny day" : "stary night")}.";
            if (30 <= s && s < 60) desc = $"Look up to the sky and see a partly {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "sunny day" : "stary night")}.";
            if (60 <= s && s < 70) desc = $"Look up to the sky and see a partly cloudy {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "day" : "night")}.";
            if (70 <= s && s < 90) desc = $"Look up to the sky and see a mostly cloudy {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "day" : " night")}.";
            if (s >= 90) desc = $"Look up to the sky and see an overcast {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "day" : " night")}.";
        }
        return desc;
    }
}

public class WindDirectionValue
{
    public int Duration { get; set; }
    public string UnitCode { get; set; }
    public int Value { get; set; }
    public string Description => WindDirection(Value);
    private string WindDirection(int windDirection)
    {
        string direction = string.Empty;
        switch (windDirection)
        {
            case >= 349 or < 11:
                direction = "North";
                break;
            case >= 11 and < 34:
                direction = "North-Northeast";
                break;
            case >= 34 and < 56:
                direction = "Northeast";
                break;
            case >= 56 and < 79:
                direction = "East-Northeast";
                break;
            case >= 79 and < 101:
                direction = "East";
                break;
            case >= 101 and < 124:
                direction = "East-Southeast";
                break;
            case >= 124 and < 146:
                direction = "Southeast";
                break;
            case >= 146 and < 169:
                direction = "South-Southeast";
                break;
            case >= 169 and < 191:
                direction = "South";
                break;
            case >= 191 and < 214:
                direction = "South-Southwest";
                break;
            case >= 214 and < 236:
                direction = "Southwest";
                break;
            case >= 236 and < 259:
                direction = "West-Southwest";
                break;
            case >= 259 and < 281:
                direction = "West";
                break;
            case >= 281 and < 304:
                direction = "West-Northwest";
                break;
            case >= 304 and < 326:
                direction = "Northwest";
                break;
            case >= 326 and < 349:
                direction = "North-Northwest";
                break;
        }
        return direction;
    }
}
public class WindSpeedValue
{
    public int Duration { get; set; }
    public string WindSpeedStatment => $" winds around {Value}{UnitCode}";
    public string UnitCode => "mph";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 1.609344), 2);
        }
    }
}
public class DailyWindSpeedValue
{
    public string WindSpeedStatment => Value > 0 ? $" winds around {Value}{UnitCode}" : $" winds from {Min} to {Max}{UnitCode}";
    public string UnitCode => "mph";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 1.609344), 2);
        }
    }
    public double Max
    {
        get => field;
        set
        {
            field = Math.Round((value / 1.609344), 2);
        }
    }
    public double Min
    {
        get => field;
        set
        {
            field = Math.Round((value / 1.609344), 2);
        }
    }
}

public class WindGustValue
{
    public int Duration { get; set; }
    public string GustStatement => $", and gust up to {Value}{UnitCode}.";
    public string UnitCode => "mph";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 1.609344), 2);
        }
    }
}

public class Weather
{
    public WeatherValue?[] Values { get; set; }
}

public class WeatherValue
{

    public WeatherStatementsValue?[] Value { get; set; }
}

public partial class WeatherStatementsValue : ObservableObject
{
    [ObservableProperty]
    public partial int Duration { get; set; }
    [ObservableProperty]
    public partial DateTime Time { get; set; }
    [ObservableProperty]
    public partial string Statement { get; set; }
    [ObservableProperty]
    public partial string Coverage { get; set; }
    [ObservableProperty]
    public partial string Intensity { get; set; }
    [ObservableProperty]
    public partial string Weather { get; set; }
    [ObservableProperty]
    public partial string[] Attributes { get; set; }
    public string Category => GetCategories(this.Weather);

    private string GetCategories(string weather)
    {
        foreach (var p in Enum.GetValues(typeof(PrecipitationType)))
        {
            if (weather.StartsWith(p.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return p.ToString().ToLowerInvariant();
            }
        }
        // Implement your logic to determine the category based on the weather string
        return string.Empty;
    }

}



public class VisibilityValue
{
    public int Duration { get; set; }
    public string UnitCode => "ft";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 3.2808, 2);
        }
    }
}


public class Hazard
{
    public HazardValue?[] Values { get; set; }
}
public class HazardValue
{

    public HazardAttributeValue[] Value { get; set; }
}

public class HazardAttributeValue
{
    public int Duration { get; set; }
    public string Phenomenon { get; set; }
    public string Significance { get; set; }
    public double EventNumber { get; set; }
}

public class Heatrisk
{
    public HeatRiskValue[] Values { get; set; }
}

public class HeatRiskValue
{
    public int Duration { get; set; }
    public DateTime Time { get; set; }
    public string UnitCode { get; set; }
    public int Value { get; set; }
    public string HeatRiskDescription => GetHeatRiskDescription();
    public Color HeatRiskColor => GetHeatRiskColor();
    private Color GetHeatRiskColor()
    {
        switch (this.Value)
        {
            case 0:
                return Color.FromRgb(0, 255, 0);
            case 1:
                return Color.FromRgb(255, 255, 0);
            case 2:
                return Color.FromRgb(255, 165, 0);
            case 3:
                return Color.FromRgb(255, 0, 0);
            case 4:
                return Color.FromRgb(255, 0, 255);
            default:
                return Color.FromRgb(0, 255, 0);
        }

    }
    private string GetHeatRiskDescription()
    {
        switch (this.Value)
        {
            case 0:
                return "There is a category 0 (Green) ForecastHeatRisk. Little to no risk from expected heat.";
            case 1:
                return "There is a category 1 (Yellow) ForecastHeatRisk. Minor - Primarily affects those who are extremely sensitive to heat and without cooling/hydration.";
            case 2:
                return "There is a category 2 (Orange) ForecastHeatRisk. Moderate - Affects those who are sensitive to heat, especially those without cooling/hydration, and some health systems and industries.";
            case 3:
                return "There is a category 3 (Red) ForecastHeatRisk. Major - Affects anyone without cooling/hydration as well as health systems and industries.";
            case 4:
                return "There is a category 4 (Magenta/Purple) ForecastHeatRisk. Extreme - Rare and/or long-duration extreme heat with no overnight relief affecting anyone without cooling/hydration as well as health systems, industries, and infrastructure.";
            default:
                return "There is no ForecastHeatRisk category issued today.";
        }
    }
}

public class ProbabilityOfPrecipitationValue
{
    public int Duration { get; set; }
    public string UnitCode => "%";
    public double Value { get; set; }
}

public class QuantitativePrecipitationValue
{
    public int Duration { get; set; }
    public string UnitCode => Value <= 1 ? "inch" : "inches";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 25.4), 2);
        }
    }
    public DateTime Time { get; set; }
    public string Description => $"{this.Value} {this.UnitCode}";
}

public class IceAccumulationValue
{
    public int Duration { get; set; }
    public string UnitCode => Value <= 1 ? "inch" : "inches";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 25.4), 2);
        }
    }
}

public class SnowFallAmountValue
{
    public int Duration { get; set; }
    public string UnitCode => Value <= 1 ? "inch" : "inches";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 25.4), 2);
        }
    }
}

public class SnowlevelValue
{
    public int Duration { get; set; }
    public string UnitCode { get; set; }
    public double Value { get; set; }
}
/// <summary>
/// About CeilingHeightValue
/// Use for:
/// -Predicting whether the sky will open up enough for color
/// -Determining if low stratus will block sunrise / sunset beams
/// -Assessing fog‑like conditions even when Visibility is okay
/// -Narrative cloud‑cover descriptions(“low ceiling”, “high ceiling”, etc.)
/// </summary>
public class CeilingHeightValue
{
    public int Duration { get; set; }
    public string UnitCode => "ft";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 3.2808, 2);
        }
    }
}

public class VisibilitiesValue
{
    public int Duration { get; set; }
    public string UnitCode => "ft";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round(value * 3.2808, 2);
        }
    }
}

public class TransportWindSpeedValue
{
    public int Duration { get; set; }
    public string UnitCode => "mph";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value / 1.609344), 2);
        }
    }
}

public class TransportWindDirectionValue
{
    public int Duration { get; set; }
    public string Direction => GetWeatherService.WindDirection((int)this.Value);
    public double Value { get; set; }
}

public class MixingHeightValue
{
    public int Duration { get; set; }
    public string UnitCode => "ft";
    public double Value
    {
        get => field;
        set
        {
            if (value > 0)
            {
                field = Math.Round(value * 3.2808399, 2);
            }
            else
            {
                field = 0;
            }
        }
    }
}

public class HainesindexValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class Lightningactivitylevel
{
    public object[] Values { get; set; }
}

public class Twentyfootwindspeed
{
    public object[] Values { get; set; }
}

public class Twentyfootwinddirection
{
    public object[] Values { get; set; }
}

public class WaveHeightValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class WavePeriodValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class Wavedirection
{
    public object[] Values { get; set; }
}

public class Primaryswellheight
{
    public object[] Values { get; set; }
}

public class Primaryswelldirection
{
    public object[] Values { get; set; }
}

public class Secondaryswellheight
{
    public object[] Values { get; set; }
}

public class Secondaryswelldirection
{
    public object[] Values { get; set; }
}

public class WavePeriod2Value
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class WindWaveHeightValue
{
    public string UnitCode => "ft";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value * 3.2808399), 2);
        }
    }
}

public class Dispersionindex
{
    public object[] Values { get; set; }
}

public class Pressure
{
    public object[] Values { get; set; }
}

public class Probabilityoftropicalstormwind
{
    public object[] Values { get; set; }
}

public class Probabilityofhurricanewind
{
    public object[] Values { get; set; }
}

public class Potentialof15mphwind
{
    public object[] Values { get; set; }
}

public class Potentialof25mphwind
{
    public object[] Values { get; set; }
}

public class Potentialof35mphwind
{
    public object[] Values { get; set; }
}

public class Potentialof45mphwind
{
    public object[] Values { get; set; }
}

public class Potentialof20mphwindgust
{
    public object[] Values { get; set; }
}

public class Potentialof30mphwindgust
{
    public object[] Values { get; set; }
}

public class Potentialof40mphwindgust
{
    public object[] Values { get; set; }
}

public class Potentialof50mphwindgust
{
    public object[] Values { get; set; }
}

public class Potentialof60mphwindgust
{
    public object[] Values { get; set; }
}

public class Grasslandfiredangerindex
{
    public object[] Values { get; set; }
}

public class ProbabilityOfThunderValue
{
    public int Duration { get; set; }
    public string UnitCode => "%";
    public double Value { get; set; }
    public string Description => Value switch
    {
        > 75 => " there is an extremely high risk wide spread and intense thunderstorms with strong winds.",
        > 50 and <= 75 => " there is a high risk of thunderstorms and strong winds.",
        > 30 and <= 50 => " there is a moderate risk of thunderstorms and strong winds.",
        > 10 and <= 30 => " there is a slight risk of scattered thunderstorms possible.",
        > 1 and <= 10 => " there is a low chance of thunderstorms.",
        _ => " No thunderstorms expected."
    };
    public DateTime Time { get; set; }
}

public class Davisstabilityindex
{
    public object[] Values { get; set; }
}

public class Atmosphericdispersionindex
{
    public object[] Values { get; set; }
}

public class LowVisibilityOccurrenceRiskIndexValue
{
    public int Duration { get; set; }
    public string UnitCode => "ft";
    public double Value
    {
        get => field;
        set
        {
            field = Math.Round((value * 3.2808399), 2);
        }
    }
    public string Description => Value switch
    {
        1 or 2 or 3 => "Low risk of very low Visibility.",
        4 or 5 or 6 => "Moderate risk of low Visibility.",
        7 or 8 or 9 => "High risk of low Visibility, possibility of super fog. Super fog occurs when smoke from a fire mixes with fog.",
        10 => "Exreme risk  of low to no visibility expected. possibility of super fog. Super fog occurs when smoke from a fire mixes with fog.",
        _ => "No low Visibility risk issued."
    };
}

// T is constrained to struct so that "T?" below means Nullable<T> (e.g. double?),
// allowing NWS null values to deserialize and be checked with HasValue.
public class NwsGridpointProperty<T> where T : struct
{
    [JsonPropertyName("uom")]
    public string? Uom { get; set; }

    [JsonPropertyName("values")]
    public NwsGridpointValue<T>[]? Values { get; set; }
}
public class NwsGridpointValue<T> where T : struct
{
    [JsonPropertyName("validTime")]
    public string ValidTime { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public T? Value { get; set; }
}

public class NwsWeatherCondition
{
    public string? coverage { get; set; }
    public string? weather { get; set; }
    public string? intensity { get; set; }
}

public class Stability
{
    public object[] Values { get; set; }
}

public class Redflagthreatindex
{
    public object[] Values { get; set; }
}

public enum PrecipitationType
{
    // Liquid water drops larger than 0.5 mm (0.02 in) in diameter.
    // The most common form, falling when air temperatures are above freezing.
    rain,
    // Very fine, uniform droplets (<0.5 mm) that appear to float in the air but
    // fall to the ground. Often occurs with fog.
    drizzle,
    // Transparent or translucent hard grains of ice, formed when raindrops
    // freeze before reaching the ground or refreeze after melting.
    ice_pellets,
    // Balls or irregular lumps of ice, ≥¼ in (5 mm) in diameter, usually in
    // strong thunderstorms; ≥1 in (2.5 cm) indicates severe storms.
    hail,
    //  (Small Hail/Snow Pellets) – White, opaque, round or conical ice grains <¼ in (5 mm),
    //  often with a slushy texture.
    graupel,
    // Crystalline ice particles, usually six‑pointed flakes, falling when the entire
    // atmospheric column is below freezing.
    snow
    // Very small, opaque ice grains—essentially frozen drizzle
    // Needles, columns, or plates falling in very cold air; also called “diamond dust,”
    // producing optical effects like light pillars.
}

public enum NwsWeatherIntensity
{
    light = 0,
    very_light = 1,
    moderate = 2,
    heavy = 3
}



