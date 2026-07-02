using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public class GetPhotoDay
    {

        public GetPhotoDay(Shared.ISessionStore ss)
        {
            _ss = ss;
        }
        // Groups weather and events by day for the Calendar and Forecast views.
        public async Task BuildPhotoDayAsync(int utcOffset)
        {
            var start = _ss.StartDateTime;
            var end = _ss.EndDateTime;
            int dayCount = (int)end.Date.Subtract(start.Date).TotalDays + 1;
            var current = start.Date;
            try
            {

                PhotoDay[] photoDays = new PhotoDay[dayCount];
                for (int d = 0; d < _ss.DailyCollection.Length; d++)
                {
                    var startTime = _ss.DailyCollection[d].StartTime;
                    var endTime = _ss.DailyCollection[d].EndTime;
                    var daily = _ss.DailyCollection[d];
                    //var dailyCollection = _ss.GfsData.DailyForecasts.Where(e => e.StartTime.Date == current).Select(d => new Daily
                    //{
                    //    StartTime = d.StartTime,
                    //    EndTime = d.EndTime,
                    //    Name = d.Name,
                    //    DailyTemperature = d.Temperature,
                    //    DailyTemperatureUnits = d.TemperatureUnit,
                    //    DailyHigh = $"with a high of{Environment.NewLine}{_ss.GfsData.HourlyTimeTemps.Where(x => x.High.Time.Date == d.StartTime.Date).First().High.Temp}°{d.TemperatureUnit}at {_ss.GfsData.HourlyTimeTemps.Where(x => x.High.Time.Date == d.StartTime.Date).First().High.Time.ToString("h tt")}",
                    //    DailyLow = $"with a low of{Environment.NewLine}{_ss.GfsData.HourlyTimeTemps.Where(x => x.Low.Time.Date == d.StartTime.Date).First().Low.Temp}°{d.TemperatureUnit}at {_ss.GfsData.HourlyTimeTemps.Where(x => x.Low.Time.Date == d.StartTime.Date).First().Low.Time.ToString("h tt")}",
                    //    DailyImageSource = d.DailyImageSource,
                    //    DailyForecast = d.DetailForecast
                    //}).OrderBy(x => x.StartTime).ToArray<Daily>();
                    //_ss.DailyCollection = new ObservableCollection<Daily>(dailyCollection);
                    //var hourlyCollection = _ss.GfsData.HourlyForecasts.Where(w => w.StartTime.Date == current).OrderBy(d => d.StartTime).Select(y => new Hourly
                    //{
                    //    StartTime = y.StartTime,
                    //    EndTime = y.EndTime,
                    //    HourlyTemperature = y.Temperature,
                    //    HourlyTemperatureUnits = y.TemperatureUnit,
                    //    IsDailyHigh = y.IsHigh,
                    //    IsDailyLow = y.IsLow,
                    //    HourlyForecast = y.DetailForecast,
                    //    HourlyImageSource = y.HourlyImageSource,
                    //    Events = _ss.EventsData.EventTimes.Where(e => e.WeatherHour == y.StartTime).OrderBy(o => o.OrderTime).ToObservableCollection()
                    //}).OrderBy(r => r.StartTime).ToArray<Hourly>();
                    //_ss.HourlyCollection = new ObservableCollection<Hourly>(hourlyCollection);
                    photoDays[d] = new PhotoDay
                    {
                        Day = current.Day,
                        Date = current.Date,

                        Hourlies = _ss.HourlyCollection.Where(h => h.StartTime >= startTime && h.EndTime <= endTime).OrderBy(y => y.StartTime).ToArray()
                        //                        Groups = _ss.HourlyCollection.OrderBy(h => h.StartTime).GroupBy(d => new GroupedHourly { Date = d.StartTime.Date, TimeOfDay = d.TimeOfDay }).ToObservableCollection()
                    };


                }//For Loop Ends

                // _ss.PhotoDays = new ObservableCollection<PhotoDay>(photoDays);

                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.Now.ToFileTime()} ERROR: Get Photo Day Service :{ex} ");
            }
        }
    }
}