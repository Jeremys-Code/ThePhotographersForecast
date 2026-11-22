namespace ThePhotographersForecast.Models
{
    public class SkyHourData
    {
        public string Time { get; set; }
        public SkyLayerFloat Temperature2m { get; set; }
        public SkyLayerInt CloudCover { get; set; }
        public SkyLayerInt CloudCoverLow { get; set; }
        public SkyLayerInt CloudCoverMid { get; set; }
        public SkyLayerInt CloudCoverHigh { get; set; }
        public SkyLayerFloat Visibility { get; set; }
        public SkyLayerInt RelativeHumidity2m { get; set; }
        public SkyLayerInt WindDirection120m { get; set; }
        public SkyLayerFloat WindSpeed120m { get; set; }
        public SkyLayerFloat WindGusts10m { get; set; }
        public SkyLayerInt PrecipitationProbability { get; set; }
        public SkyLayerFloat Precipitation { get; set; }
        public SkyLayerFloat ApparentTemperature { get; set; }
    }

    public class SkyLayerInt
    {
        public int Value { get; set; }
        public string UnitCode { get; set; }
    }
    public class SkyLayerFloat
    {
        public float Value { get; set; }
        public string UnitCode { get; set; }
    }
}
