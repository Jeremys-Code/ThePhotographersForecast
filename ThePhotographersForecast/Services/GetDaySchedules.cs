using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Services
{
    public partial class GetDaySchedules : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<DateTime> DatesCollection { get; set; } = new ObservableCollection<DateTime>();
        [ObservableProperty]
        public partial ObservableCollection<DaySchedule> DaySchedules { get; set; } = new ObservableCollection<DaySchedule>();
        [ObservableProperty]
        public partial ObservableCollection<Hourly> HourlyCollection { get; set; } = new ObservableCollection<Hourly>();
        [ObservableProperty]
        public partial ObservableCollection<EventTime> EventCollection { get; set; } = new ObservableCollection<EventTime>();

        private readonly SelectLocationViewModel _slvm;
        public GetDaySchedules()
        {
            _slvm = ServiceHelper.GetService<SelectLocationViewModel>();
            DaySchedules = new ObservableCollection<DaySchedule>();
            var current = _slvm.PhotoStartDate;
            DatesCollection = new ObservableCollection<DateTime>();
            while (current < _slvm.PhotoEndDate)
            {
                DatesCollection.Add(current);
                current = current.AddHours(1);
            }
        }

        public ObservableCollection<DaySchedule> GetSchedules()
        {
            // Map minimal daily weather data
            //DailyCollection = _slvm.GfsData.DailyForecasts.Select(d => new Models.Daily
            //{
            //    Name = d.Name,
            //    StartTime = d.StartTime,
            //    EndTime = d.EndTime,
            //    DailyTemperature = $"{d.Temperature}°{d.TemperatureUnit}",
            //    DailyHigh = $"with a high near {d.DailyHighTemp}°{d.TemperatureUnit}{Environment.NewLine}Time: {d.StartTime:h tt}",
            //    DailyLow = $"Temp: {d.DailyLowTemp}°{d.TemperatureUnit}{Environment.NewLine}Time: {d.StartTime:h tt}",
            //    DailyIcon = d.DailyIcon,
            //    DailyForecast = d.DetailForecast,
            //    DailyImageSource = new ImageSourceConverter().ConvertFromString(d.DailyIcon) as ImageSource
            //}).ToObservableCollection<Daily>();

            // Map minimal hourly weather data
            HourlyCollection = _slvm.GfsData.HourlyForecasts.Select(h => new Models.Hourly
            {
                StartTime = h.StartTime,
                EndTime = h.EndTime,
                HourlyTemperature = $"{h.Temperature}°{h.TemperatureUnit}",
                HourlyIcon = h.HourlyIcon,
                HourlyForecast = h.DetailForecast,
                HourlyImageSource = h.HourlyIconPath
            }).ToObservableCollection<Hourly>();

            // Map events data
            EventCollection = _slvm.EventsModel.EventTimes.ToObservableCollection();
            var days = DatesCollection.Select(d => d.Date).Distinct().OrderBy(x => x).ToArray();

            for (int d = 0; d < days.Length; d++)
            {
                var hoursOfDay = DatesCollection.Where(j => j.Date == days[d]).ToArray();
                DaySchedule day = new DaySchedule
                {
                    PhotoDate = days[d],
                    PhotoSchedules = new PhotoSchedule[hoursOfDay.Length]
                };

                // Use a standard foreach loop. Parallel execution with ObservableCollections causes race conditions.
                for (int h = 0; h < hoursOfDay.Length; h++)
                {
                    var t = hoursOfDay[h];
                    var hourly = HourlyCollection.FirstOrDefault(h => h.StartTime == t);
                    var eventsForHour = EventCollection.Where(e => e.WeatherHour == t).OrderBy(v => v.OrderTime).ToObservableCollection();
                    if (hourly != null)
                    {
                        var photoSchedule = new PhotoSchedule()
                        {
                            PhotoDateTime = t,
                            StartTime = hourly.StartTime,
                            EndTime = hourly.EndTime,
                            Name = hourly.Name,
                            ForecastIcon = hourly.HourlyIcon,
                            ForecastDetail = hourly.HourlyForecast,
                            //IsHigh = (_slvm.GfsData.HourlyTimeTemps.Where(h => h.High.Time == t).FirstOrDefault()) ? true : false,
                            //IsLow = (_slvm.GfsData.HourlyTimeTemps.Where(l => l.Low.Time == t).Count() == 1) ? true : false,
                            Events = new ObservableCollection<EventTime>(eventsForHour)
                            //HourlyForecast = hourly ?? new Hourly(),
                            //DailyForecast = daily ?? new Daily()
                        };
                        // Handle Empty Events
                        if (photoSchedule.Events.Count == 0)
                        {
                            photoSchedule.Events.Add(new EventTime
                            {
                                Date = DateOnly.FromDateTime(t),
                                WeatherHour = t,
                                EventName = "No events this hour."
                            });

                        }
                        day.PhotoSchedules[h] = photoSchedule;
                    }
                    else
                    {
                        var empty = new PhotoSchedule();
                        day.PhotoSchedules[h] = empty;
                    }

                }
                DaySchedules.Add(day);
            }

            return DaySchedules;
        }
    }
}


//    // Handle Forecast Coalescing 
//    if (dailyWx != null)
//    {
//        photoSchedule.IsDaily = true;
//        photoSchedule.Name = dailyWx.Name;
//        photoSchedule.StartTime = dailyWx.StartTime;
//        photoSchedule.EndTime = dailyWx.EndTime;
//        photoSchedule.Temperature = $"{dailyWx.Temperature}°F";
//        photoSchedule.DailyHigh = dailyWx.DailyHigh;
//        photoSchedule.DailyLow = dailyWx.DailyLow;
//        photoSchedule.ForecastDetail = dailyWx.DailyForecast;
//        photoSchedule.ForecastIcon = dailyWx.DailyIcon;
//        photoSchedule.ForecastImageSource =
//    }
//    else if (hourlyWx != null)
//    {
//        photoSchedule.IsDaily = false;
//        photoSchedule.StartTime = hourlyWx.StartTime;
//        photoSchedule.EndTime = hourlyWx.EndTime;
//        photoSchedule.ForecastDetail = hourlyWx.HourlyForecast;
//        photoSchedule.ForecastIcon = hourlyWx.HourlyIcon;
//        photoSchedule.DailyHigh = string.Empty;
//        photoSchedule.DailyLow = string.Empty;
//        photoSchedule.ForecastImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(hourlyWx.HourlyIcon);
//        photoSchedule.Name = hourlyWx.Name;
//    }
