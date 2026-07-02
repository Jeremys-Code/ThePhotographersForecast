//using CommunityToolkit.Maui.Core.Extensions;
//using System.Collections.ObjectModel;
//using ThePhotographersForecast.Models;

//namespace ThePhotographersForecast.Services
//{
// The same grouping by day with the same properties as PhotoDays
//    public class GetPhotoSchedules
//    {

//        private Shared.SessionStore _ss;
//        public GetPhotoSchedules(Shared.SessionStore ss)
//        {
//            _ss = ss;
//        }

//        public ObservableCollection<DaySchedule> GetSchedules()
//        {
//            _ss.DaySchedules = new ObservableCollection<DaySchedule>();
//            _ss.PhotoSchedules = new ObservableCollection<PhotoSchedule>();

//            var eventCollection = _ss.EventsData.EventTimes.OrderBy(e => e.WeatherHour).ToObservableCollection();

//            //var scheduleList = hourlyList.Select(h => new Models.PhotoSchedule
//            //{
//            //    PhotoDate = DateOnly.FromDateTime(h.StartTime),
//            //    StartTime = h.StartTime,
//            //    EndTime = h.EndTime,
//            //    ForecastImageSource = h.HourlyImageSource,
//            //    HourlyTemperature = h.HourlyTemperature,
//            //    HourlyTemperatureUnits = h.HourlyTemperatureUnits,
//            //    Name = h.Name,
//            //    ForecastDetail = h.HourlyForecast,
//            //    IsHigh = h.IsDailyHigh,
//            //    IsLow = h.IsDailyLow,
//            //    Events = eventCollection.Where(x => x.WeatherHour == h.StartTime).ToObservableCollection() ?? new ObservableCollection<EventTime> { new EventTime { AstroTime = h.StartTime, EventName = "No events" } }
//            //}).ToList();
//            //_ss.PhotoSchedules = new ObservableCollection<PhotoSchedule>(scheduleList);

//            var grouped = _ss.PhotoSchedules.OrderBy(x => x.StartTime).GroupBy(y => y.PhotoDate).OrderBy(z => z.Key).ToArray();

//            // Days df hourly forecasts

//            var daySchedules = new List<DaySchedule>();
//            foreach (var photoDay in _ss.PhotoDays)
//            {
//                daySchedules.Add(new DaySchedule
//                {
//                    PhotoDate = date,
//                    //Take the PhotoSchedules array for each grouped date
//                    PhotoSchedules = grouped.FirstOrDefault(x => x.Key == date)?.OrderBy(s => s.StartTime).ToObservableCollection() ?? new ObservableCollection<PhotoSchedule>()
//                });
//            }
//            var schedules = new ObservableCollection<DaySchedule>(daySchedules);

//            return schedules;
//        }
//    }
//}
