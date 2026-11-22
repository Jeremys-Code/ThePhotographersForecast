using ThePhotographersForecast.Models;

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

    // Numeric gridpoint layers inherit uom/values (and validTime/value per entry)
    // from NwsGridpointProperty<T> so they can be passed straight to ExpandMetric<T>.
    public class Temperature : NwsGridpointProperty<double> { }

    public class Dewpoint : NwsGridpointProperty<double> { }

    public class Maxtemperature : NwsGridpointProperty<double> { }

    public class MinTemperature : NwsGridpointProperty<double> { }

    public class Relativehumidity : NwsGridpointProperty<double> { }

    public class Apparenttemperature : NwsGridpointProperty<double> { }

    public class Wetbulbglobetemperature : NwsGridpointProperty<double> { }

    // Heatindex represents the apparent temperature (temerapture and Relative Humidity combined)
    public class Heatindex : NwsGridpointProperty<double> { }

    public class Windchill : NwsGridpointProperty<double> { }

    public class Skycover : NwsGridpointProperty<double> { }

    public class Winddirection : NwsGridpointProperty<int> { }

    public class Windspeed : NwsGridpointProperty<double> { }

    public class Windgust : NwsGridpointProperty<double> { }
    public class Transportwindspeed : NwsGridpointProperty<double> { }
    public class Transportwinddirection : NwsGridpointProperty<int> { }
    public class Visibility1 : NwsGridpointProperty<double> { }
    public class Ceilingheight : NwsGridpointProperty<double> { }

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

    // HeatRisk values are 0-4 representing heat anonomlies based on the recorded average temperature of the day.
    public class Heatrisk : NwsGridpointProperty<int> { }

    public class Probabilityofprecipitation : NwsGridpointProperty<double> { }

    public class Quantitativeprecipitation : NwsGridpointProperty<double> { }

    public class Iceaccumulation : NwsGridpointProperty<double> { }

    public class Snowfallamount : NwsGridpointProperty<double> { }

    public class Snowlevel
    {
        public object[] values { get; set; }
    }

    public class CeilingHeightValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class VisibilityValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class TransportWindSpeedValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class TransportWindDirectionValues
    {
        public string validTime { get; set; }
        public double value { get; set; }
    }

    public class Mixingheight : NwsGridpointProperty<double> { }

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

    public class Probabilityofthunder : NwsGridpointProperty<double> { }

    public class Davisstabilityindex
    {
        public object[] values { get; set; }
    }

    public class Atmosphericdispersionindex
    {
        public object[] values { get; set; }
    }

    public class Lowvisibilityoccurrenceriskindex : NwsGridpointProperty<double> { }

    public class Stability
    {
        public object[] values { get; set; }
    }

    public class Redflagthreatindex
    {
        public object[] values { get; set; }
    }

}
