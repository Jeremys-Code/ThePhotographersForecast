using System.Collections.ObjectModel;
using static ThePhotographersForecast.Data.GridDetailData;

namespace ThePhotographersForecast.Models
{
    public class NwsWeatherDetail
    {
        public DateTime Date { get; set; }
        public DateOnly DateOnly => DateOnly.FromDateTime(this.Date);
        public string TimeOfDay => Services.TimeOfDayService.CalcTimeOfDay(this.Date.Hour);
        public string WeatherSummary { get; set; }
        public ObservableCollection<WeatherAttributeValues> WeatherAttributesCollection { get; set; }
    }

    //public class Measurement3
    //{
    //    public  Value { get; set; }
    //    public string UnitCode { get; set; }
    //}
    //public class IntUnitValue
    //{
    //    public int Value { get; set; }
    //    public string UnitCode { get; set; }
    //}
}