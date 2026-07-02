namespace ThePhotographersForecast.Data;

public class GridDetailData
{
    public class Rootobject
    {
        public object[] context { get; set; } = null!;

        public string id { get; set; } = null!;
        public string type { get; set; } = null!;
        public Geometry geometry { get; set; } = null!;
        public Properties properties { get; set; } = null!;
    }

    public class Geometry
    {
        public string type { get; set; } = null!;
        public double[][][] coordinates { get; set; } = null!;
    }

    public class Properties
    {
        public string id { get; set; } = null!;
        public string type { get; set; } = null!;
        public string updateTime { get; set; } = null!;
        public string validTimes { get; set; } = null!;
        public Elevation elevation { get; set; } = null!;
        public string forecastOffice { get; set; } = null!;
        public string gridId { get; set; } = null!;
        public int gridX { get; set; }
        public int gridY { get; set; }
        public Temperature temperature { get; set; } = null!;
        public Dewpoint dewpoint { get; set; } = null!;
        public Maxtemperature maxTemperature { get; set; } = null!;
        public Mintemperature minTemperature { get; set; } = null!;
        public Relativehumidity relativeHumidity { get; set; } = null!;
        public Apparenttemperature apparentTemperature { get; set; } = null!;
        public Wetbulbglobetemperature wetBulbGlobeTemperature { get; set; } = null!;
        public Heatindex heatIndex { get; set; } = null!;
        public Windchill windChill { get; set; } = null!;
        public Skycover skyCover { get; set; } = null!;
        public Winddirection windDirection { get; set; } = null!;
        public Windspeed windSpeed { get; set; } = null!;
        public Windgust windGust { get; set; } = null!;
        public Weather weather { get; set; } = null!;
        public Hazards hazards { get; set; } = null!;
        public Heatrisk heatRisk { get; set; } = null!;
        public Probabilityofprecipitation probabilityOfPrecipitation { get; set; } = null!;
        public Quantitativeprecipitation quantitativePrecipitation { get; set; } = null!;
        public Iceaccumulation iceAccumulation { get; set; } = null!;
        public Snowfallamount snowfallAmount { get; set; } = null!;
        public Snowlevel snowLevel { get; set; } = null!;
        public Ceilingheight ceilingHeight { get; set; } = null!;
        public Visibility visibility { get; set; } = null!;
        public Transportwindspeed transportWindSpeed { get; set; } = null!;
        public Transportwinddirection transportWindDirection { get; set; } = null!;
        public Mixingheight mixingHeight { get; set; } = null!;
        public Hainesindex hainesIndex { get; set; } = null!;
        public Lightningactivitylevel lightningActivityLevel { get; set; } = null!;
        public Twentyfootwindspeed twentyFootWindSpeed { get; set; } = null!;
        public Twentyfootwinddirection twentyFootWindDirection { get; set; } = null!;
        public Waveheight waveHeight { get; set; } = null!;
        public Waveperiod wavePeriod { get; set; } = null!;
        public Wavedirection waveDirection { get; set; } = null!;
        public Primaryswellheight primarySwellHeight { get; set; } = null!;
        public Primaryswelldirection primarySwellDirection { get; set; } = null!;
        public Secondaryswellheight secondarySwellHeight { get; set; } = null!;
        public Secondaryswelldirection secondarySwellDirection { get; set; } = null!;
        public Waveperiod2 wavePeriod2 { get; set; } = null!;
        public Windwaveheight windWaveHeight { get; set; } = null!;
        public Dispersionindex dispersionIndex { get; set; } = null!;
        public Pressure pressure { get; set; } = null!;
        public Probabilityoftropicalstormwinds probabilityOfTropicalStormWinds { get; set; } = null!;
        public Probabilityofhurricanewinds probabilityOfHurricaneWinds { get; set; } = null!;
        public Potentialof15mphwinds potentialOf15mphWinds { get; set; } = null!;
        public Potentialof25mphwinds potentialOf25mphWinds { get; set; } = null!;
        public Potentialof35mphwinds potentialOf35mphWinds { get; set; } = null!;
        public Potentialof45mphwinds potentialOf45mphWinds { get; set; } = null!;
        public Potentialof20mphwindgusts potentialOf20mphWindGusts { get; set; } = null!;
        public Potentialof30mphwindgusts potentialOf30mphWindGusts { get; set; } = null!;
        public Potentialof40mphwindgusts potentialOf40mphWindGusts { get; set; } = null!;
        public Potentialof50mphwindgusts potentialOf50mphWindGusts { get; set; } = null!;
        public Potentialof60mphwindgusts potentialOf60mphWindGusts { get; set; } = null!;
        public Grasslandfiredangerindex grasslandFireDangerIndex { get; set; } = null!;
        public Probabilityofthunder probabilityOfThunder { get; set; } = null!;
        public Davisstabilityindex davisStabilityIndex { get; set; } = null!;
        public Atmosphericdispersionindex atmosphericDispersionIndex { get; set; } = null!;
        public Lowvisibilityoccurrenceriskindex lowVisibilityOccurrenceRiskIndex { get; set; } = null!;
        public Stability stability { get; set; } = null!;
        public Redflagthreatindex redFlagThreatIndex { get; set; } = null!;
    }

    public class Elevation
    {
        public string unitCode { get; set; } = null!;
        public int value { get; set; }
    }

    public class Temperature
    {
        public string uom { get; set; } = null!;
        public TemperatureValues[] values { get; set; } = null!;
    }

    public class TemperatureValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Dewpoint
    {
        public string uom { get; set; } = null!;
        public DewPointValues[] values { get; set; } = null!;
    }

    public class DewPointValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Maxtemperature
    {
        public string uom { get; set; } = null!;
        public MaxTemperatureValues[] values { get; set; } = null!;
    }

    public class MaxTemperatureValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Mintemperature
    {
        public string uom { get; set; } = null!;
        public MinTemperatureValues[] values { get; set; } = null!;
    }

    public class MinTemperatureValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Relativehumidity
    {
        public string uom { get; set; } = null!;
        public RelativeHumidityValues[] values { get; set; } = null!;
    }

    public class RelativeHumidityValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Apparenttemperature
    {
        public string uom { get; set; } = null!;
        public ApparentTemperatureValues[] values { get; set; } = null!;
    }

    public class ApparentTemperatureValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Wetbulbglobetemperature
    {
        public string uom { get; set; } = null!;
        public WetBulbGlobeTemperatureValues[] values { get; set; } = null!;
    }

    public class WetBulbGlobeTemperatureValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Heatindex
    {
        public string uom { get; set; } = null!;
        public HeatIndexValues[] values { get; set; } = null!;
    }

    public class HeatIndexValues
    {
        public string validTime { get; set; } = null!;
        public object value { get; set; } = null!;
    }

    public class Windchill
    {
        public string uom { get; set; } = null!;
        public WindchillValues[] values { get; set; } = null!;
    }

    public class WindchillValues
    {
        public string validTime { get; set; } = null!;
        public double? value { get; set; } = null!;
    }

    public class Skycover
    {
        public string uom { get; set; } = null!;
        public SkyCoverValues[] values { get; set; } = null!;
    }

    public class SkyCoverValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Winddirection
    {
        public string uom { get; set; } = null!;
        public WindDirectionValues[] values { get; set; } = null!;
    }

    public class WindDirectionValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Windspeed
    {
        public string uom { get; set; } = null!;
        public WindSpeedValues[] values { get; set; } = null!;
    }

    public class WindSpeedValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Windgust
    {
        public string uom { get; set; } = null!;
        public WindGustValues[] values { get; set; } = null!;
    }

    public class WindGustValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Weather
    {
        public WeatherValues[] values { get; set; } = null!;
    }

    public class WeatherValues
    {
        public string validTime { get; set; } = null!;
        public WeatherAttributeValues[] value { get; set; } = null!;
    }

    public class WeatherAttributeValues
    {
        public string coverage { get; set; } = null!;
        public string weather { get; set; } = null!;
        public string intensity { get; set; } = null!;
        public VisibilityValues visibility { get; set; } = null!;
        public object[] attributes { get; set; } = null!;
    }

    public class Visibility
    {
        public string unitCode { get; set; } = null!;
        public double? value { get; set; } = null!;
    }

    public class Hazards
    {
        public object[] values { get; set; } = null!;
    }

    public class Heatrisk
    {
        public HeatRiskValues[] values { get; set; } = null!;
    }

    public class HeatRiskValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Probabilityofprecipitation
    {
        public string uom { get; set; } = null!;
        public ProbabilityOfPrecipitationValues[] values { get; set; } = null!;
    }

    public class ProbabilityOfPrecipitationValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Quantitativeprecipitation
    {
        public string uom { get; set; } = null!;
        public QuantitativePrecipitationValues[] values { get; set; } = null!;
    }

    public class QuantitativePrecipitationValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Iceaccumulation
    {
        public string uom { get; set; } = null!;
        public IceAccumulationValues[] values { get; set; } = null!;
    }

    public class IceAccumulationValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Snowfallamount
    {
        public string uom { get; set; } = null!;
        public SnowFallAmountValues[] values { get; set; } = null!;
    }

    public class SnowFallAmountValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Snowlevel
    {
        public object[] values { get; set; } = null!;
    }

    public class Ceilingheight
    {
        public string uom { get; set; } = null!;
        public CeilingHeightValues[] values { get; set; } = null!;
    }

    public class CeilingHeightValues
    {
        public string validTime { get; set; } = null!;
        public double? value { get; set; } = null!;
    }

    public class Visibility1
    {
        public string uom { get; set; } = null!;
        public VisibilityValues[] values { get; set; } = null!;
    }

    public class VisibilityValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Transportwindspeed
    {
        public string uom { get; set; } = null!;
        public TransportWindSpeedValues[] values { get; set; } = null!;
    }

    public class TransportWindSpeedValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Transportwinddirection
    {
        public string uom { get; set; } = null!;
        public TransportWindDirectionValues[] values { get; set; } = null!;
    }

    public class TransportWindDirectionValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Mixingheight
    {
        public string uom { get; set; } = null!;
        public MixingHeightValues[] values { get; set; } = null!;
    }

    public class MixingHeightValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Hainesindex
    {
        public object[] values { get; set; } = null!;
    }

    public class Lightningactivitylevel
    {
        public object[] values { get; set; } = null!;
    }

    public class Twentyfootwindspeed
    {
        public object[] values { get; set; } = null!;
    }

    public class Twentyfootwinddirection
    {
        public object[] values { get; set; } = null!;
    }

    public class Waveheight
    {
        public string uom { get; set; } = null!;
        public WaveHeightValues[] values { get; set; } = null!;
    }

    public class WaveHeightValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Waveperiod
    {
        public string uom { get; set; } = null!;
        public WavePeriodValues[] values { get; set; } = null!;
    }

    public class WavePeriodValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Wavedirection
    {
        public object[] values { get; set; } = null!;
    }

    public class Primaryswellheight
    {
        public object[] values { get; set; } = null!;
    }

    public class Primaryswelldirection
    {
        public object[] values { get; set; } = null!;
    }

    public class Secondaryswellheight
    {
        public object[] values { get; set; } = null!;
    }

    public class Secondaryswelldirection
    {
        public object[] values { get; set; } = null!;
    }

    public class Waveperiod2
    {
        public string uom { get; set; } = null!;
        public WavePeriod2Values[] values { get; set; } = null!;
    }

    public class WavePeriod2Values
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Windwaveheight
    {
        public string uom { get; set; } = null!;
        public WindWaveHeightValues[] values { get; set; } = null!;
    }

    public class WindWaveHeightValues
    {
        public string validTime { get; set; } = null!;
        public double value { get; set; }
    }

    public class Dispersionindex
    {
        public object[] values { get; set; } = null!;
    }

    public class Pressure
    {
        public object[] values { get; set; } = null!;
    }

    public class Probabilityoftropicalstormwinds
    {
        public object[] values { get; set; } = null!;
    }

    public class Probabilityofhurricanewinds
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof15mphwinds
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof25mphwinds
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof35mphwinds
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof45mphwinds
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof20mphwindgusts
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof30mphwindgusts
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof40mphwindgusts
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof50mphwindgusts
    {
        public object[] values { get; set; } = null!;
    }

    public class Potentialof60mphwindgusts
    {
        public object[] values { get; set; } = null!;
    }

    public class Grasslandfiredangerindex
    {
        public object[] values { get; set; } = null!;
    }

    public class Probabilityofthunder
    {
        public ProbabilityOfThunderValues[] values { get; set; } = null!;
    }

    public class ProbabilityOfThunderValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Davisstabilityindex
    {
        public object[] values { get; set; } = null!;
    }

    public class Atmosphericdispersionindex
    {
        public object[] values { get; set; } = null!;
    }

    public class Lowvisibilityoccurrenceriskindex
    {
        public LowVisibilityOccurrenceRiskIndexValues[] values { get; set; } = null!;
    }

    public class LowVisibilityOccurrenceRiskIndexValues
    {
        public string validTime { get; set; } = null!;
        public int value { get; set; }
    }

    public class Stability
    {
        public object[] values { get; set; } = null!;
    }

    public class Redflagthreatindex
    {
        public object[] values { get; set; } = null!;
    }

}
