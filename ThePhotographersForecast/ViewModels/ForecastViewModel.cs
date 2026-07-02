using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> Dailys { get; set; } = new ObservableCollection<WeatherForecast>();
        [ObservableProperty]
        public partial ImageSource CurrentDailyIcon { get; set; }
        [ObservableProperty]
        public partial string CurrentDailyName { get; set; }
        [ObservableProperty]
        public partial string CurrentDailySummary { get; set; }
        [ObservableProperty]
        public partial DateTime CurrentEventTime { get; set; } = new DateTime();
        [ObservableProperty]
        public partial DateTime CurrentEventEndTime { get; set; } = new DateTime();
        //[ObservableProperty]
        //public partial DateTime currentDailyStart { get; set; } =
        //[ObservableProperty]
        //public partial DateTime currentDailyEnd { get; set; } =
        [ObservableProperty]
        public partial DateTime CurrentHourlyStart { get; set; } = new DateTime();
        [ObservableProperty]
        public partial ImageSource CurrentHourlyIcon { get; set; }
        [ObservableProperty]
        public partial string CurrentHourlySummary { get; set; }
        [ObservableProperty]
        public partial string CurrentHourlyName { get; set; }
        [ObservableProperty]
        public partial string CurrentEventName { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<DateTime> Dates { get; set; } = new ObservableCollection<DateTime>();
        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string CurrentTime { get; set; }
        [ObservableProperty]
        public partial string DisplayName { get; set; }
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();
        [ObservableProperty]
        public partial ObservableCollection<EventTime> SelectedDailyEvents { get; set; } = new ObservableCollection<EventTime>();
        [ObservableProperty]
        public partial EventTime SelectedDailyCurrentEvent { get; set; } = new EventTime();

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdateSelectedDaily();
                }
            }
        }

        private DailyForecast _selectedDaily;
        public DailyForecast SelectedDaily
        {
            get => _selectedDaily;
            private set => SetProperty(ref _selectedDaily, value);
        }
        [ObservableProperty]
        public partial HourlyForecast SelectedEventHourlyWeather { get; set; }
        [ObservableProperty]
        public partial string TimePeriod { get; set; }

        private SelectLocationViewModel _slvm;
        public ForecastViewModel(SelectLocationViewModel slvm)
        {
            try
            {
                IsWorking = true;

                _slvm = slvm;
                Dailys = _slvm.Dailys;
                Debug.WriteLine($"[ForecastVM] Initializing with SelectedLocation: {_slvm.SelectedLocation?.DisplayName}");

                // DailyStartTime initialization
                Dates = _slvm.Dates.OrderBy(x => x).ToObservableCollection();
                Debug.WriteLine($"[ForecastVM] DailyStartTime count: {Dates?.Count() ?? 0}");

                // SelectedDate initialization - guard against empty dates
                if (Dates.Count() > 0)
                {
                    SelectedDate = Dates.First();
                    SelectedDaily = Dailys.Where(x => new DateTime(x.Date, TimeOnly.MinValue).Date == SelectedDate).Select(d => d.DayForecast).First() ?? new DailyForecast();
                    Debug.WriteLine($"[ForecastVM] SelectedDate set to: {SelectedDate:F}");
                }
                else
                {
                    Debug.WriteLine("[ForecastVM] WARNING: DailyStartTime collection is empty!");
                    SelectedDate = DateTime.Now.Date;
                }

                // Dailys initialization
                //Dailys = ss.DailyCollection.ToArray();
                // Debug.WriteLine($"[ForecastVM] Dailys count: {Dailys?.Length ?? 0}");

                // SelectedDaily initialization - match by date
                //if (ss.DailyCollection.Length > 0 && SelectedDate != default)
                //{
                TimePeriod = SelectedDaily.TimePeriod;
                //    Debug.WriteLine($"[ForecastVM] SelectedDaily set to: {SelectedDaily?.StartTime:F}");
                //}
                //else
                //{
                //    Debug.WriteLine("[ForecastVM] WARNING: Dailys collection is empty! Creating default Daily.");
                //    SelectedDaily = new Daily();
                //}

                // SelectedDailyEvents initialization
                SelectedDailyEvents = SelectedDaily.DailyEvents;
                Debug.WriteLine($"[ForecastVM] SelectedDailyEvents.Count == SelectedDailyEvents.Count ?? 0}}");

                //                // SelectedDailyCurrentEvent - guard against empty events
                //if (SelectedDailyEvents.Length > 0)
                //{
                SelectedDailyCurrentEvent = SelectedDailyEvents.FirstOrDefault() ?? new EventTime();
                //  Debug.WriteLine($"[ForecastVM] SelectedDailyCurrentEvent set to: {SelectedDailyCurrentEvent?.EventName}");
                //}
                //else
                //{
                //  Debug.WriteLine("[ForecastVM] WARNING: SelectedDailyEvents collection is empty! Creating default EventTime.");
                //  SelectedDailyCurrentEvent = new EventTime();
                //  }

                // SelectedEventHourlyWeather initialization
                SelectedEventHourlyWeather = SelectedDaily.HourlyForecasts.Where(h => h.Time == SelectedDailyCurrentEvent.WeatherHour).First() ?? new HourlyForecast();
                // Debug.WriteLine($"[ForecastVM] SelectedEventHourlyWeather initialized");

                // Location initialization
                SelectedLocation = SelectedLocation;
                Debug.WriteLine($"[ForecastVM] SelectedLocation: {SelectedLocation?.DisplayName}");

                DisplayName = SelectedLocation.DisplayName;

                CurrentTime = DateTime.UtcNow.AddHours(SelectedLocation?.TimeZoneOffset ?? 0).ToString("F");
                Debug.WriteLine($"[ForecastVM] CurrentTime: {CurrentTime}");

                // Initialize timer
                _timer = new System.Timers.Timer(1000); // 1 second interval
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();

                IsWorking = false;
                Debug.WriteLine("[ForecastVM] Initialization completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForecastVM] ERROR in constructor: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[ForecastVM] StackTrace: {ex.StackTrace}");
                IsWorking = false;
                throw; // Re-throw so the error propagates and we can see it
            }
        }


        //private string GetTimePeriodTitle(Daily daily)
        //{
        //    string timePeriod = string.Empty;
        //    if (daily.IsDay)
        //    {
        //        timePeriod = $"{daily.StartTime.ToString("dddd, MMMM dd, yyyy")}{Environment.NewLine}during the day";
        //    }
        //    else
        //    {
        //        timePeriod = $"{daily.StartTime.ToString("dddd, MMMM dd, yyyy")} - {daily.EndTime.ToString("dddd, MMMM dd, yyyy")}{Environment.NewLine}overnight";
        //    }
        //    return timePeriod;
        //}

        private void UpdateSelectedDaily()
        {
            if (SelectedDate != null)
            {
                SelectedDaily = Dailys.Where(x => new DateTime(x.Date, TimeOnly.MinValue) == SelectedDate.Date).First().DayForecast;
                //var eventWeatherHours = _ss.EventsData.EventTimes
                //    .Select(x => x.WeatherHour).ToHashSet();
                //var currentDaily = Dailys
                //    .Where(m => m.StartTime == SelectedDate)
                //    .FirstOrDefault();
                //var currentHourlys = _ss.HourlyCollection
                //    .Where(h => eventWeatherHours.Contains(h.StartTime)).ToArray();
                //SelectedDay = new Daily
                //{
                //    StartTime = currentDaily.StartTime,
                //    EndTime = currentDaily.EndTime,
                //    Name = currentDaily.Name,
                //    IsDay = currentDaily.IsDay,
                //    DailyImageSource = currentDaily.DailyImageSource,
                //    //t= GetTimePeriodTitle(currentDaily),
                //    //DisplayName = currentDaily.Name,
                //    DailyTemperature = currentDaily.DailyTemperature,
                //    DailyTemperatureUnits = currentDaily.DailyTemperatureUnits,
                //    DailyEvents = currentDaily.DailyEvents.Select(e => new EventsViewModel
                //    {
                //        GetEvent = e,

                //        //EventName = e.EventName,
                //        //EventTime = e.AstroTime,
                //        //EndTime = e.OtherTime,
                //        //TimeOfDay = e.TimeOfDay,
                //        CurrentHourly = hour.First(x => x.StartTime == e.WeatherHour),
                //        HourlyStartTime = e.WeatherHour,
                //        HourlyIcon = currentHourlys.First(x => x.StartTime == e.WeatherHour).HourlyImageSource,
                //        HourlyForecast = currentHourlys.First(x => x.StartTime == e.WeatherHour).HourlyForecast,
                //        HourlyName = currentHourlys.First(x => x.StartTime == e.WeatherHour).Name ?? string.Empty
                //    }).ToArray()
                //};
            }
        }
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.UtcNow.AddHours(_slvm.SelectedLocation.TimeZoneOffset).ToString("F");
        }

        //private async Task OnSelectedDateChangedAsync(DateTime value)
        //{

        //    SelectedDaily = Dailys.Where(x => x.StartTime == value).First() ?? Dailys.First();
        //    SelectedDailyEvents = SelectedDaily.DailyEvents;
        //    SelectedDailyCurrentEvent = SelectedDailyEvents[0];
        //    SelectedEventHourlyWeather = _ss.HourlyCollection.Where(h => SelectedDailyCurrentEvent.WeatherHour == h.StartTime).First() ?? new Hourly();
        //    return;
        //}
    }
}
