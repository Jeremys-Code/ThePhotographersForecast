using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Models;

// Translates the weather api model into what is stored in the database
public partial class NwsWeatherModel : ObservableObject
{
    [ObservableProperty]
    public partial NwsDailyForecast[] DailyForecasts { get; set; }
    [ObservableProperty]
    public partial NwsHourlyForecast[] HourlyForecasts { get; set; }
    [ObservableProperty]
    public partial NwsHourlySupplemental[] HourlySupplementals { get; set; }

    public NwsWeatherModel()
    {

    }
    public NwsWeatherModel(NwsWeatherModel _nwsData)
    {

        if (_nwsData == null)
            throw new ArgumentNullException(nameof(_nwsData), "Source object cannot be null.");

        DailyForecasts = _nwsData.DailyForecasts.ToArray();
        HourlyForecasts = _nwsData.HourlyForecasts.ToArray();
        HourlySupplementals = _nwsData.HourlySupplementals.ToArray();
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
    public double Temperature { get; set; }
    public string TemperatureUnit { get; set; }

    public string TempAndUnit => GetTempString();
    public double ChanceOfPrecipitation { get; set; }

    public string PrecipitationUnitCode { get; set; }

    public string DailyIcon { get; set; }

    public string ShortForecast { get; set; }

    public string DetailForecast { get; set; }

    public WindSpeedValue WindSpeed { get; set; }

    public string WindDirection { get; set; }
    public WindGustValue? WindGusts { get; set; }
    public string WindSpeedStatement { get; set; }

    public ImageSource DailyImageSource { get; set; } = ImageSource.FromFile("unknown.png");

    public SKFileLottieImageSource SkDailyAnimation { get; set; }

    public NwsHourlyForecast[] HourlyWeathers { get; set; }
    public NwsHourlySupplemental[] SupplementalData { get; set; }

    public string EnrichedForecast { get; set; }

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
        return $"{(int)Math.Round(this.Temperature, 0)}{this.TemperatureUnit ?? string.Empty}";
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
public class NwsHourlyForecast
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

    public NwsHourlySupplemental? Supplemental { get; set; }
    public SKFileLottieImageSource SkHourlyAnimation { get; set; }
    public string WindStatement => BuildWindStatement();

    public string IntroTimeOfDay => TimeOfDayService.TimesOfDayGreeting(this.StartTime.Hour);

    private string GetTempString()
    {
        return $"{(int)Math.Round(this.Temperature.Value, 0)}{this.Temperature.UnitCode}";
    }

    public class TemperatureHourly
    {
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    public class ProbabilityOfPrecipitationHourly
    {
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    public class DewPointHourly
    {
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    public class RelativeHumidityHourly
    {
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    public class WindSpeedHourly
    {
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    public class WindGustHourly
    {
        public string GustStatement { get; set; }
        public string UnitCode { get; set; }
        public double Value { get; set; }
    }

    private string BuildWindStatement()
    {

        return String.Concat("Winds from the ", this.WindDirection, " at ", this.WindSpeed.Value, " mph");
    }
}

public class NwsTimeOfDayForecast
{
    public NwsHourlyForecast[] Forecasts { get; set; }
    public string Condition { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeOfDayService.TimeOfDay TimeOfDay => TimeOfDayService.GetTimeOfDay(this.StartTime.Hour);
}

public class NwsHourlySupplemental
{
    public DateTime Time { get; set; }
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
    public WindGustValue? WindGust { get; set; }
    public WeatherStatementsValue? Weather { get; set; }
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

}

public class NwsLayer
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class TemperatureValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class DewPointValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class MaxTemperatureValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}
public class MinTemperatureValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class RelativeHumidityValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class ApparentTemperatureValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class WetBulbGlobeTemperatureValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }

}

public class HeatIndexValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class WindchillValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
    public string StatementC => $"{Math.Round(Value, 0)}{UnitCode}";
    public string StatementF => $"{Math.Round((Value) * 1.8 + 32, 0)}°F";
    public double ValueF => (Value) * 1.8 + 32;
}

public class SkyCoverValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
    public DateTime Time { get; set; }
    public string Description => GetSkyCoverDescription();
    public string Trend { get; set; }
    private string GetSkyCoverDescription()
    {
        string desc = string.Empty;
        if (this.Value is double s)
        {
            if (s < 10) desc = $"A clear{(this.Time.Hour >= 6 || this.Time.Hour < 18 ? " sunny day" : " stary night")}.";
            if (10 <= s && s < 30) desc = $"A mostly {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "sunny day" : "stary night")}.";
            if (30 <= s && s < 70) desc = $"A partly cloudy {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "day" : " night")}.";
            if (70 <= s && s < 90) desc = $"A mostly cloudy {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "day" : " night")}.";
            if (s >= 90) desc = $"An overcast {(this.Time.Hour >= 6 || this.Time.Hour < 18 ? "day" : " night")}.";
        }
        return desc;
    }
}

public class WindDirectionValue
{
    public string UnitCode { get; set; }
    public int Value { get; set; }
}

public class WindSpeedValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
    public double Max { get; set; }
    public double Min { get; set; }
}

public class WindGustValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class Weather
{
    public WeatherValue?[] Values { get; set; }
}

public class WeatherValue
{

    public WeatherStatementsValue?[] Value { get; set; }
}

public class WeatherStatementsValue
{
    public DateTime Time { get; set; }
    public string Statement { get; set; }
    public string Coverage { get; set; }
    public string Intensity { get; set; }
    public string Weather { get; set; }
    public string[] Attributes { get; set; }
}

public class VisibilityValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
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
    public string phenomenon { get; set; }
    public string significance { get; set; }
    public double event_number { get; set; }
}

public class Heatrisk
{
    public HeatRiskValue[] Values { get; set; }
}

public class HeatRiskValue
{
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
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class QuantitativePrecipitationValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class IceAccumulationValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class SnowFallAmountValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class SnowlevelValue
{
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
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class VisibilitiesValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class TransportWindSpeedValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class TransportWindDirectionValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
}

public class MixingHeightValue
{
    public string UnitCode { get; set; }
    public double Value { get; set; }
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
    public string UnitCode { get; set; }
    public double Value { get; set; }
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
    public string UnitCode { get; set; }
    public double Value { get; set; }
    public string description { get; set; }
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
    public string UnitCode { get; set; }
    public double Value { get; set; }
    public string description { get; set; }
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
    Rain,
    Rain_Showers,
    // Very fine, uniform droplets (<0.5 mm) that appear to float in the air but
    // fall to the ground. Often occurs with fog.
    Drizzle,
    Drizzle_Showers,
    // Transparent or translucent hard grains of ice, formed when raindrops
    // freeze before reaching the ground or refreeze after melting.
    Ice_Pellets,
    // Balls or irregular lumps of ice, ≥¼ in (5 mm) in diameter, usually in
    // strong thunderstorms; ≥1 in (2.5 cm) indicates severe storms.
    Hail,
    //  (Small Hail/Snow Pellets) – White, opaque, round or conical ice grains <¼ in (5 mm),
    //  often with a slushy texture.
    Graupel,
    // Crystalline ice particles, usually six‑pointed flakes, falling when the entire
    // atmospheric column is below freezing.
    Snow,
    Snow_Showers,
    // Very small, opaque ice grains—essentially frozen drizzle.
    Snow_Grain,
    // Needles, columns, or plates falling in very cold air; also called “diamond dust,”
    // producing optical effects like light pillars.
    Ice_Crystals,
    Fog
}

public enum ShortForecastPhrase
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
