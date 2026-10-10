namespace ThePhotographersForecast.Data;

public class GridDetailData
{
    public class Rootobject
    {
        public string id { get; set; }
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public double[][][] coordinates { get; set; }
    }

    public class Properties
    {
        public string id { get; set; }
        public string type { get; set; }
        public string updateTime { get; set; }
        public string validTimes { get; set; }
        public Elevation elevation { get; set; }
        public string forecastOffice { get; set; }
        public string gridId { get; set; }
        public double gridX { get; set; }
        public double gridY { get; set; }
        public Temperature? temperature { get; set; }
        public Dewpoint? dewpoint { get; set; }
        public Maxtemperature? maxTemperature { get; set; }
        public MinTemperature? minTemperature { get; set; }
        public Relativehumidity? relativeHumidity { get; set; }
        public Apparenttemperature? apparentTemperature { get; set; }
        public Wetbulbglobetemperature? wetBulbGlobeTemperature { get; set; }
        public Heatindex? heatIndex { get; set; }
        public Windchill? windChill { get; set; }
        public Skycover? skyCover { get; set; }
        public Winddirection? windDirection { get; set; }
        public Windspeed? windSpeed { get; set; }
        public Windgust? windGust { get; set; }
        public Weather? weather { get; set; }
        public Hazards? hazards { get; set; }
        public Heatrisk? heatRisk { get; set; }
        public Probabilityofprecipitation? probabilityOfPrecipitation { get; set; }
        public Quantitativeprecipitation? quantitativePrecipitation { get; set; }
        public Iceaccumulation? iceAccumulation { get; set; }
        public Snowfallamount? snowfallAmount { get; set; }
        public Snowlevel? snowLevel { get; set; }
        public Ceilingheight? ceilingHeight { get; set; }
        public Visibility1? visibility { get; set; }
        public Transportwindspeed? transportWindSpeed { get; set; }
        public Transportwinddirection? transportWindDirection { get; set; }
        public Mixingheight? mixingHeight { get; set; }
        public Hainesindex? hainesIndex { get; set; }
        public Lightningactivitylevel? lightningActivityLevel { get; set; }
        public Twentyfootwindspeed? twentyFootWindSpeed { get; set; }
        public Twentyfootwinddirection? twentyFootWindDirection { get; set; }
        public Waveheight? waveHeight { get; set; }
        public Waveperiod? wavePeriod { get; set; }
        public Wavedirection? waveDirection { get; set; }
        public Primaryswellheight? primarySwellHeight { get; set; }
        public Primaryswelldirection? primarySwellDirection { get; set; }
        public Secondaryswellheight? secondarySwellHeight { get; set; }
        public Secondaryswelldirection? secondarySwellDirection { get; set; }
        public Waveperiod2? wavePeriod2 { get; set; }
        public Windwaveheight? windWaveHeight { get; set; }
        public Dispersionindex? dispersionIndex { get; set; }
        public Pressure? pressure { get; set; }
        public Probabilityoftropicalstormwinds? probabilityOfTropicalStormWinds { get; set; }
        public Probabilityofhurricanewinds? probabilityOfHurricaneWinds { get; set; }
        public Potentialof15mphwinds? potentialOf15mphWinds { get; set; }
        public Potentialof25mphwinds? potentialOf25mphWinds { get; set; }
        public Potentialof35mphwinds? potentialOf35mphWinds { get; set; }
        public Potentialof45mphwinds? potentialOf45mphWinds { get; set; }
        public Potentialof20mphwindgusts? potentialOf20mphWindGusts { get; set; }
        public Potentialof30mphwindgusts? potentialOf30mphWindGusts { get; set; }
        public Potentialof40mphwindgusts? potentialOf40mphWindGusts { get; set; }
        public Potentialof50mphwindgusts? potentialOf50mphWindGusts { get; set; }
        public Potentialof60mphwindgusts? potentialOf60mphWindGusts { get; set; }
        public Grasslandfiredangerindex? grasslandFireDangerIndex { get; set; }
        public Probabilityofthunder? probabilityOfThunder { get; set; }
        public Davisstabilityindex? davisStabilityIndex { get; set; }
        public Atmosphericdispersionindex? atmosphericDispersionIndex { get; set; }
        public Lowvisibilityoccurrenceriskindex? lowVisibilityOccurrenceRiskIndex { get; set; }
        public Stability? stability { get; set; }
        public Redflagthreatindex? redFlagThreatIndex { get; set; }
    }

    public class Elevation
    {
        public string uom { get; set; }
        public double value { get; set; }
    }

    public class Temperature
    {
        public string uom { get; set; }
        public TemperatureValues[] values { get; set; }
    }

    public class TemperatureValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Dewpoint
    {
        public string uom { get; set; }
        public DewPointValues[] values { get; set; }
    }

    public class DewPointValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Maxtemperature
    {
        public string uom { get; set; }
        public MaxTemperatureValues[] values { get; set; }
    }

    public class MaxTemperatureValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class MinTemperature
    {
        public string uom { get; set; }
        public MinTemperatureValues[] values { get; set; }
    }

    public class MinTemperatureValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Relativehumidity
    {
        public string uom { get; set; }
        public RelativeHumidityValues[] values { get; set; }
    }

    public class RelativeHumidityValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Apparenttemperature
    {
        public string uom { get; set; }
        public ApparentTemperatureValues[] values { get; set; }
    }

    public class ApparentTemperatureValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Wetbulbglobetemperature
    {
        public string uom { get; set; }
        public WetBulbGlobeTemperatureValues[] values { get; set; }
    }

    public class WetBulbGlobeTemperatureValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Heatindex
    {
        public string uom { get; set; }
        public HeatIndexValues[] values { get; set; }
    }
    // Heatindex represents the apparent temperature (temerapture and Relative Humidity combined)
    public class HeatIndexValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Windchill
    {
        public string uom { get; set; }
        public WindchillValues[] values { get; set; }
    }

    public class WindchillValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Skycover
    {
        public string uom { get; set; }
        public SkyCoverValues[] values { get; set; }
    }

    public class SkyCoverValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Winddirection
    {
        public string uom { get; set; }
        public WindDirectionValues[] values { get; set; }
    }

    public class WindDirectionValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Windspeed
    {
        public string uom { get; set; }
        public WindSpeedValues[] values { get; set; }
    }

    public class WindSpeedValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Windgust
    {
        public string uom { get; set; }
        public WindGustValues[] values { get; set; }
    }

    public class WindGustValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Weather
    {
        public WeatherValues[] values { get; set; }
    }

    public class WeatherValues
    {
        public string validTime { get; set; }
        public WeatherAttributeValues[] value { get; set; }
    }

    public class WeatherAttributeValues
    {
        public string coverage { get; set; }
        public string weather { get; set; }
        public string intensity { get; set; }
        public string[] attributes { get; set; }
    }
    public class Hazards
    {
        public HazardValues[] values { get; set; }
    }
    public class HazardValues
    {
        public string validTime { get; set; }
        public HazardAttributeValues[] value { get; set; }
    }
    public class HazardAttributeValues
    {
        public string phenomenon { get; set; }
        public string significance { get; set; }
        public double event_number { get; set; }
    }

    public class Heatrisk
    {
        public HeatRiskValues[] values { get; set; }
    }
    // HeatRisk values are 0-4 representing heat anonomlies based on the recorded average temperature of the day.
    public class HeatRiskValues
    {
        public string validTime { get; set; }
        public int value { get; set; }
    }

    public class Probabilityofprecipitation
    {
        public string uom { get; set; }
        public ProbabilityOfPrecipitationValues[] values { get; set; }
    }

    public class ProbabilityOfPrecipitationValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Quantitativeprecipitation
    {
        public string uom { get; set; }
        public QuantitativePrecipitationValues[] values { get; set; }
    }

    public class QuantitativePrecipitationValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Iceaccumulation
    {
        public string uom { get; set; }
        public IceAccumulationValues[] values { get; set; }
    }

    public class IceAccumulationValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Snowfallamount
    {
        public string uom { get; set; }
        public SnowFallAmountValues[] values { get; set; }
    }

    public class SnowFallAmountValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Snowlevel
    {
        public object[] values { get; set; }
    }
    public class Ceilingheight
    {
        public string uom { get; set; }
        public CeilingHeightValues[] values { get; set; }
    }

    public class CeilingHeightValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Visibility1
    {
        public string uom { get; set; }
        public VisibilityValues[] values { get; set; }
    }

    public class VisibilityValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Transportwindspeed
    {
        public string uom { get; set; }
        public TransportWindSpeedValues[] values { get; set; }
    }

    public class TransportWindSpeedValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Transportwinddirection
    {
        public string uom { get; set; }
        public TransportWindDirectionValues[] values { get; set; }
    }

    public class TransportWindDirectionValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Mixingheight
    {
        public string uom { get; set; }
        public MixingHeightValues[] values { get; set; }
    }

    public class MixingHeightValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Hainesindex
    {
        public object[] values { get; set; }
    }

    public class Lightningactivitylevel
    {
        public object[] values { get; set; }
    }

    public class Twentyfootwindspeed
    {
        public object[] values { get; set; }
    }

    public class Twentyfootwinddirection
    {
        public object[] values { get; set; }
    }

    public class Waveheight
    {
        public string uom { get; set; }
        public WaveHeightValues[] values { get; set; }
    }

    public class WaveHeightValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Waveperiod
    {
        public string uom { get; set; }
        public WavePeriodValues[] values { get; set; }
    }

    public class WavePeriodValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Wavedirection
    {
        public object[] values { get; set; }
    }

    public class Primaryswellheight
    {
        public object[] values { get; set; }
    }

    public class Primaryswelldirection
    {
        public object[] values { get; set; }
    }

    public class Secondaryswellheight
    {
        public object[] values { get; set; }
    }

    public class Secondaryswelldirection
    {
        public object[] values { get; set; }
    }

    public class Waveperiod2
    {
        public string uom { get; set; }
        public WavePeriod2Values[] values { get; set; }
    }

    public class WavePeriod2Values
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Windwaveheight
    {
        public string uom { get; set; }
        public WindWaveHeightValues[] values { get; set; }
    }

    public class WindWaveHeightValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Dispersionindex
    {
        public object[] values { get; set; }
    }

    public class Pressure
    {
        public object[] values { get; set; }
    }

    public class Probabilityoftropicalstormwinds
    {
        public object[] values { get; set; }
    }

    public class Probabilityofhurricanewinds
    {
        public object[] values { get; set; }
    }

    public class Potentialof15mphwinds
    {
        public object[] values { get; set; }
    }

    public class Potentialof25mphwinds
    {
        public object[] values { get; set; }
    }

    public class Potentialof35mphwinds
    {
        public object[] values { get; set; }
    }

    public class Potentialof45mphwinds
    {
        public object[] values { get; set; }
    }

    public class Potentialof20mphwindgusts
    {
        public object[] values { get; set; }
    }

    public class Potentialof30mphwindgusts
    {
        public object[] values { get; set; }
    }

    public class Potentialof40mphwindgusts
    {
        public object[] values { get; set; }
    }

    public class Potentialof50mphwindgusts
    {
        public object[] values { get; set; }
    }

    public class Potentialof60mphwindgusts
    {
        public object[] values { get; set; }
    }

    public class Grasslandfiredangerindex
    {
        public object[] values { get; set; }
    }

    public class Probabilityofthunder
    {
        public ProbabilityOfThunderValues[] values { get; set; }
    }

    public class ProbabilityOfThunderValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Davisstabilityindex
    {
        public object[] values { get; set; }
    }

    public class Atmosphericdispersionindex
    {
        public object[] values { get; set; }
    }

    public class Lowvisibilityoccurrenceriskindex
    {
        public LowVisibilityOccurrenceRiskIndexValues[] values { get; set; }
    }

    public class LowVisibilityOccurrenceRiskIndexValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Stability
    {
        public object[] values { get; set; }
    }

    public class Redflagthreatindex
    {
        public object[] values { get; set; }
    }

}
