using CommunityToolkit.Maui.Core.Extensions;
using PhotographersDay.Services;
using PhotographersDay.ViewModels;
using PhotographersDay.Views;
using System.Collections.ObjectModel;

namespace PhotographersDay.Models
{
    public class EventForecastCollection
    {
        private DateTime dailyStartTime { get; set; }
        private DateTime dailyEndTime { get; set; }
        public DateOnly[] Dates { get; set; }
        public ObservableCollection<Daily> Dailies { get; set; } = new ObservableCollection<Daily>();
        public ObservableCollection<Hourly> HourlyForecast { get; set; } = new ObservableCollection<Hourly>();
        public ObservableCollection<(DateOnly, EventsModel[])> EventTimes { get; set; } = new ObservableCollection<(DateOnly, EventsModel[])>();
        public ObservableCollection<DateOnly> GetDates()
        {
            var gfsDaily = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.DailyForecasts;
            dailyStartTime = gfsDaily.Min(x => x.StartTime);
            dailyEndTime = gfsDaily.Max(x => x.EndTime);
            Dates = new DateOnly[gfsDaily.Length];
            for (int i = 0; i < gfsDaily.Length; i++)
            {
                Dates[i] = DateOnly.FromDateTime(gfsDaily[i].StartTime);
                dailyStartTime = gfsDaily[i].StartTime < dailyStartTime ? gfsDaily[i].StartTime : dailyStartTime;
                dailyEndTime = gfsDaily[i].EndTime > dailyEndTime ? gfsDaily[i].EndTime : dailyEndTime;
            }
            return Dates;
        }

        public ObservableCollection<Daily> GetDailyWeathers(DateTime dailyStartTime, DateTime dailyEndTime)
        {
            var gfsDaily = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.DailyForecasts.Where(x => x.StartTime == dailyStartTime && x.EndTime == dailyEndTime).ToArray();
            Daily[] Daily = new Daily[gfsDaily.Length];
            for (int i = 0; i < gfsDaily.Length; i++)
            {
                //12 Hour weather summary, with the first and last 6 hours of
                //the day summarized. The hourly
                //forecast will be used to fill in the details of the daily summary,
                //and the event times will be used to fill in any events that may
                //occur during the day.
                Daily[i] = new Daily
                {
                    DailyStartTime = gfsDaily[i].StartTime,
                    DailyDetailForecast = gfsDaily[i].DetailForecast,
                    DailyIcon = gfsDaily[i].WeatherImage,
                    DailyName = gfsDaily[i].Name,
                    DailyEndTime = gfsDaily[i].EndTime
                };
                GetHourlyWeather(Daily[i].DailyStartTime, Daily[i].DailyEndTime);
            }
            return new ObservableCollection<Daily>(Daily);
        }
        public ObservableCollection<Hourly> GetHourlyWeather(DateTime startTime, DateTime endTime)
        {
            var hourlyData = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.HourlyForecasts.Where(x => x.StartTime >= startTime && x.EndTime <= endTime).OrderBy(y => y.StartTime).ToArray();
            var detail = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.DetailForecastWeathers.Where(x => x.Time >= startTime && x.Time <= endTime).OrderBy(y => y.Time).ToArray();
            var weatherDetails = Array.FindAll(detail, x => x.Time >= dailyStartTime && x.Time <= dailyEndTime);
            ObservableCollection<Hourly> hours = new ObservableCollection<Hourly>();
            int hourlyCount = hourlyData.Length;
            Parallel.For(0, hourlyData.Length, (i, ct) =>
            {
                hours[i] = new Hourly
                {
                    HourlyShortForecast = hourlyData[i].ShortForecast,
                    HourlyDetailForecast = detail.Where(d => (d.Time == DateTime.Parse(hourlyData[i].StartTime.ToString("o")))).FirstOrDefault() != null ? (detail.Where(d => (d.Time == DateTime.Parse(hourlyData[i].StartTime.ToString("o")))).FirstOrDefault().WeatherSummary) : string.Empty,
                    HourlyIcon = hourlyData[i].Icon,
                    HourlyTemperature = hourlyData[i].Temperature,
                    HourlyChanceOfPrecipitation = hourlyData[i].ChanceOfPrecipitation,
                    HourlyWindSpeedDirection = hourlyData[i].WindSpeedDirection,
                    HourlyStartTime = DateTime.Parse(hourlyData[i].StartTime.ToString("o")),
                    HourlyEndTime = DateTime.Parse(hourlyData[i].EndTime.ToString("o"))
                };

            });
            var hourlies = hours.OrderBy(x => x.HourlyStartTime).ToList().ToObservableCollection();
            return hourlies;
        }

        public ObservableCollection<(DateOnly, EventsModel[])> GetEvents()
        {
            List<(DateOnly, EventsModel[])> eventTimesList = new();
            var eventTimes = ServiceHelper.GetService<SelectLocationViewModel>().DayEvents.EventTimes.Where(x => x.WeatherHour >= dailyStartTime && x.WeatherHour <= dailyEndTime).OrderBy(y => y.WeatherHour).ToArray();
            for (int i = 0; i < eventTimes.Length; i++)
            {
                var events = Array.FindAll(eventTimes, x => x.WeatherHour >= dailyStartTime && x.WeatherHour <= dailyEndTime);
                DateOnly date = DateOnly.FromDateTime(eventTimes[i].WeatherHour);
                eventTimesList.Add((date, events));
            }
            var times = new ObservableCollection<(DateOnly, EventsModel[])>(eventTimesList);
            return times;
        }

        public class Daily
        {
            public string DailyDetailForecast { get; set; } = string.Empty;
            public string DailyIcon { get; set; } = string.Empty;
            public string DailyName { get; set; } = string.Empty;
            public DateTime DailyStartTime { get; set; } = default;
            public DateTime DailyEndTime { get; set; } = default;
            public ObservableCollection<Hourly> HourlyForecasts { get; set; } = new ObservableCollection<Hourly>();
        }
        public class Hourly
        {
            public string HourlyShortForecast { get; set; } = string.Empty;
            public string HourlyDetailForecast { get; set; } = string.Empty;
            public string HourlyIcon { get; set; } = string.Empty;
            public string HourlyTemperature { get; set; } = string.Empty;
            public string HourlyChanceOfPrecipitation { get; set; } = string.Empty;
            public string HourlyWindSpeedDirection { get; set; } = string.Empty;
            public DateTime HourlyStartTime { get; set; } = default;
            public DateTime HourlyEndTime { get; set; } = default;
            public ObservableCollection<(DateOnly, EventsModel)> EventTimes { get; set; } = new ObservableCollection<(DateOnly, EventsModel)>();
        }
    }
