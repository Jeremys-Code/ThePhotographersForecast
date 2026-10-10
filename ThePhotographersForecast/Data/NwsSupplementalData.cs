namespace ThePhotographersForecast.Data
{
    public class NwsSupplementalData
    {

        public class Rootobject
        {
            public object[] context { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public Geometry geometry { get; set; }
            public Properties properties { get; set; }
        }

        public class Geometry
        {
            public string type { get; set; }
            public float[][][] coordinates { get; set; }
        }

        public class Properties
        {
            public string id { get; set; }
            public string type { get; set; }
            public DateTime updateTime { get; set; }
            public string validTimes { get; set; }
            public Elevation elevation { get; set; }
            public string forecastOffice { get; set; }
            public string gridId { get; set; }
            public int gridX { get; set; }
            public int gridY { get; set; }
            public Temperature temperature { get; set; }
            public Dewpoint dewpoint { get; set; }
            public Maxtemperature maxTemperature { get; set; }
            public Mintemperature minTemperature { get; set; }
            public Relativehumidity relativeHumidity { get; set; }
            public Apparenttemperature apparentTemperature { get; set; }
            public Wetbulbglobetemperature wetBulbGlobeTemperature { get; set; }
            public Heatindex heatIndex { get; set; }
            public Windchill windChill { get; set; }
            public Skycover skyCover { get; set; }
            public Winddirection windDirection { get; set; }
            public Windspeed windSpeed { get; set; }
            public Windgust windGust { get; set; }
            public Weather weather { get; set; }
            public Hazards hazards { get; set; }
            public Heatrisk heatRisk { get; set; }
            public Probabilityofprecipitation probabilityOfPrecipitation { get; set; }
            public Quantitativeprecipitation quantitativePrecipitation { get; set; }
            public Iceaccumulation iceAccumulation { get; set; }
            public Snowfallamount snowfallAmount { get; set; }
            public Snowlevel snowLevel { get; set; }
            public Ceilingheight ceilingHeight { get; set; }
            public Visibility1 visibility { get; set; }
            public Transportwindspeed transportWindSpeed { get; set; }
            public Transportwinddirection transportWindDirection { get; set; }
            public Mixingheight mixingHeight { get; set; }
            public Hainesindex hainesIndex { get; set; }
            public Lightningactivitylevel lightningActivityLevel { get; set; }
            public Twentyfootwindspeed twentyFootWindSpeed { get; set; }
            public Twentyfootwinddirection twentyFootWindDirection { get; set; }
            public Waveheight waveHeight { get; set; }
            public Waveperiod wavePeriod { get; set; }
            public Wavedirection waveDirection { get; set; }
            public Primaryswellheight primarySwellHeight { get; set; }
            public Primaryswelldirection primarySwellDirection { get; set; }
            public Secondaryswellheight secondarySwellHeight { get; set; }
            public Secondaryswelldirection secondarySwellDirection { get; set; }
            public Waveperiod2 wavePeriod2 { get; set; }
            public Windwaveheight windWaveHeight { get; set; }
            public Dispersionindex dispersionIndex { get; set; }
            public Pressure pressure { get; set; }
            public Probabilityoftropicalstormwinds probabilityOfTropicalStormWinds { get; set; }
            public Probabilityofhurricanewinds probabilityOfHurricaneWinds { get; set; }
            public Potentialof15mphwinds potentialOf15mphWinds { get; set; }
            public Potentialof25mphwinds potentialOf25mphWinds { get; set; }
            public Potentialof35mphwinds potentialOf35mphWinds { get; set; }
            public Potentialof45mphwinds potentialOf45mphWinds { get; set; }
            public Potentialof20mphwindgusts potentialOf20mphWindGusts { get; set; }
            public Potentialof30mphwindgusts potentialOf30mphWindGusts { get; set; }
            public Potentialof40mphwindgusts potentialOf40mphWindGusts { get; set; }
            public Potentialof50mphwindgusts potentialOf50mphWindGusts { get; set; }
            public Potentialof60mphwindgusts potentialOf60mphWindGusts { get; set; }
            public Grasslandfiredangerindex grasslandFireDangerIndex { get; set; }
            public Probabilityofthunder probabilityOfThunder { get; set; }
            public Davisstabilityindex davisStabilityIndex { get; set; }
            public Atmosphericdispersionindex atmosphericDispersionIndex { get; set; }
            public Lowvisibilityoccurrenceriskindex lowVisibilityOccurrenceRiskIndex { get; set; }
            public Stability stability { get; set; }
            public Redflagthreatindex redFlagThreatIndex { get; set; }
        }

        public class Elevation
        {
            public string unitCode { get; set; }
            public float value { get; set; }
        }

        public class Temperature
        {
            public string uom { get; set; }
            public Value[] values { get; set; }
        }

        public class Value
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Dewpoint
        {
            public string uom { get; set; }
            public Value1[] values { get; set; }
        }

        public class Value1
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Maxtemperature
        {
            public string uom { get; set; }
            public Value2[] values { get; set; }
        }

        public class Value2
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Mintemperature
        {
            public string uom { get; set; }
            public Value3[] values { get; set; }
        }

        public class Value3
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Relativehumidity
        {
            public string uom { get; set; }
            public Value4[] values { get; set; }
        }

        public class Value4
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Apparenttemperature
        {
            public string uom { get; set; }
            public Value5[] values { get; set; }
        }

        public class Value5
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Wetbulbglobetemperature
        {
            public string uom { get; set; }
            public Value6[] values { get; set; }
        }

        public class Value6
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Heatindex
        {
            public string uom { get; set; }
            public Value7[] values { get; set; }
        }

        public class Value7
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Windchill
        {
            public string uom { get; set; }
            public Value8[] values { get; set; }
        }

        public class Value8
        {
            public string validTime { get; set; }
            public float? value { get; set; }
        }

        public class Skycover
        {
            public string uom { get; set; }
            public Value9[] values { get; set; }
        }

        public class Value9
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Winddirection
        {
            public string uom { get; set; }
            public Value10[] values { get; set; }
        }

        public class Value10
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Windspeed
        {
            public string uom { get; set; }
            public Value11[] values { get; set; }
        }

        public class Value11
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Windgust
        {
            public string uom { get; set; }
            public Value12[] values { get; set; }
        }

        public class Value12
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Weather
        {
            public Value13[] values { get; set; }
        }

        public class Value13
        {
            public string validTime { get; set; }
            public Value14[] value { get; set; }
        }

        public class Value14
        {
            public string coverage { get; set; }
            public string weather { get; set; }
            public string intensity { get; set; }
            public Visibility visibility { get; set; }
            public object[] attributes { get; set; }
        }

        public class Visibility
        {
            public string unitCode { get; set; }
            public object value { get; set; }
        }

        public class Hazards
        {
            public object[] values { get; set; }
        }

        public class Heatrisk
        {
            public Value15[] values { get; set; }
        }

        public class Value15
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Probabilityofprecipitation
        {
            public string uom { get; set; }
            public Value16[] values { get; set; }
        }

        public class Value16
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Quantitativeprecipitation
        {
            public string uom { get; set; }
            public Value17[] values { get; set; }
        }

        public class Value17
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Iceaccumulation
        {
            public string uom { get; set; }
            public Value18[] values { get; set; }
        }

        public class Value18
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Snowfallamount
        {
            public string uom { get; set; }
            public Value19[] values { get; set; }
        }

        public class Value19
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Snowlevel
        {
            public object[] values { get; set; }
        }

        public class Ceilingheight
        {
            public string uom { get; set; }
            public Value20[] values { get; set; }
        }

        public class Value20
        {
            public string validTime { get; set; }
            public float? value { get; set; }
        }

        public class Visibility1
        {
            public string uom { get; set; }
            public Value21[] values { get; set; }
        }

        public class Value21
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Transportwindspeed
        {
            public string uom { get; set; }
            public Value22[] values { get; set; }
        }

        public class Value22
        {
            public string validTime { get; set; }
            public float value { get; set; }
        }

        public class Transportwinddirection
        {
            public string uom { get; set; }
            public Value23[] values { get; set; }
        }

        public class Value23
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Mixingheight
        {
            public string uom { get; set; }
            public Value24[] values { get; set; }
        }

        public class Value24
        {
            public string validTime { get; set; }
            public float value { get; set; }
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
            public Value25[] values { get; set; }
        }

        public class Value25
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Waveperiod
        {
            public string uom { get; set; }
            public Value26[] values { get; set; }
        }

        public class Value26
        {
            public string validTime { get; set; }
            public int value { get; set; }
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
            public Value27[] values { get; set; }
        }

        public class Value27
        {
            public string validTime { get; set; }
            public int value { get; set; }
        }

        public class Windwaveheight
        {
            public string uom { get; set; }
            public Value28[] values { get; set; }
        }

        public class Value28
        {
            public string validTime { get; set; }
            public int value { get; set; }
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
            public Value29[] values { get; set; }
        }

        public class Value29
        {
            public string validTime { get; set; }
            public int value { get; set; }
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
            public Value30[] values { get; set; }
        }

        public class Value30
        {
            public string validTime { get; set; }
            public int value { get; set; }
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
}
