using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        [ObservableProperty]
        private System.Timers.Timer _timer;
        [ObservableProperty]
        public partial string GreetingLine1 { get; set; }
        [ObservableProperty]
        public partial string GreetingLine2 { get; set; }
        [ObservableProperty]
        public partial string GreetingLine3 { get; set; }
        //   [ObservableProperty]
        //   public partial ObservableCollection<PhotoDay> photoDays { get; set; }
        //     [ObservableProperty]
        //     public partial PhotoDay currentPhotoDay { get; set; }
        public int UtcOffset => SharedSession.SelectedTimeZoneOffset;
        [ObservableProperty]
        public partial string SelectedLocationTime { get; set; }
        //[ObservableProperty]
        //public partial string currentTime { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DayViewModel> Days { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast SelectedDayDayWeather { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedDayCurrentEventTimeHourlyWeather { get; set; }
        [ObservableProperty]
        public partial DailyForecast[] DayWeather { get; set; }
        [ObservableProperty]
        public partial EventModel SelectedDayEventModel { get; set; }
        [ObservableProperty]
        public partial EventTime[] SelectedDayEventTimes { get; set; }
        [ObservableProperty]
        public partial EventTime SelectedDayCurrentEventTime { get; set; }

        [ObservableProperty]
        public partial DateTime CurrentDate { get; set; }

        [ObservableProperty]
        public partial string MonthYear { get; set; }

        [ObservableProperty]
        public partial DayViewModel SelectedDay { get; set; }

        public IAsyncRelayCommand<DayViewModel> FlipCommand { get; }
        public Command SelectedDayChangedCommand { get; }

        public Session SharedSession { get; set; }
        public CalendarViewModel(Session sharedSession)
        {
            SharedSession = sharedSession;
            MainThread.BeginInvokeOnMainThread(() => Days = SharedSession.Days.ToObservableCollection());
            MainThread.BeginInvokeOnMainThread((Action)(() =>
            {
                this.Days = [.. SharedSession.Days];

                SelectedDay = Days[0];
                SelectedDayEventModel = SelectedDay.Events;
                SelectedDayEventTimes = [.. SelectedDayEventModel.EventTimes];
                SelectedDayCurrentEventTime = SelectedDayEventTimes[0];

                SelectedDayCurrentEventTimeHourlyWeather = SelectedDayCurrentEventTime.Forecast;


                //SelectedDayDayWeather = SelectedDay.WeatherDetails.Where(x=>x.Time==

                CurrentDate = DateTime.Today;
                SelectedLocationTime = DateTime.UtcNow.AddSeconds(UtcOffset).ToString("F");
                MonthYear = CurrentDate.ToString("MMMM") + " " + CurrentDate.ToString("yyyy");





                GenerateCalendar();

                // The page greeting message.
                GreetingLine1 = $"Here is your forecast for:";
                GreetingLine2 = $@"{SharedSession.SelectedLocation.City}, {(SharedSession.SelectedLocation.State != null ? SharedSession.SelectedLocation.State + ", " : "")} {SharedSession.SelectedLocation.Country}";
                GreetingLine3 = $@"{SharedSession.StartDate.DayOfWeek}, {SharedSession.StartDate.ToString("MMMM d")} - {SharedSession.EndDate.DayOfWeek}, {SharedSession.EndDate.ToString("MMMM d")}";

                // The Local Time of the selected location.
                SelectedLocationTime = DateTime.UtcNow.AddSeconds(UtcOffset).ToString("F");
                _timer = new System.Timers.Timer(1000); // 1 second interval
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();


            }));
        }

        //[RelayCommand]
        //private void OnSelectedDayChanged(DayViewModel Value)
        //{
        //    if (Value == null) return;
        //    //SelectedDayEventModel = Value.EventModel;
        //    SelectedDayEventTimes = Value.Events.EventTimes;

        //    SelectedDayCurrentEventTime = Value.Events.EventTimes[0];
        //    if (SelectedDayEventModel != null)
        //    {
        //        Value.IsEventWindowDay = true;
        //    }

        //    SelectedDayCurrentEventTimeHourlyWeather = SelectedDayCurrentEventTime.Forecast;
        //    ;
        //}
        //[RelayCommand]
        //private void OnSelectDayChanged(DayViewModel day)
        //{
        //    if (day == null) return;

        //    // 1. Assign it directly, no need tdo search the list!
        //    SelectedDay = day;
        //    SelectedDayEventTimes = SelectedDay.EventsData;
        //    SelectedDayCurrentEventModel = SelectedDayEventTimes.First();
        //    SelectedDayCurrentEventTimeHourlyWeather = SelectedDayCurrentEventModel.HourlyWeather;
        //    SelectedDayDayWeather = SelectedDay.WeatherDetails;
        //}
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            SelectedLocationTime = DateTime.UtcNow.AddSeconds(UtcOffset).ToString("F");
            //            CurrentTime = DateTime.Now.ToString("F");
        }
        private async void Init()
        {

        }
        private async Task OnFlip(DayViewModel day)
        {
            if (day == null || day.IsEmpty || !day.IsEventDay)
                return;

            day.IsFlipped = !day.IsFlipped;

            //if (day.IsFlipped && day.EventsTimes != null)
            {
                SelectedDay = day;
                SelectedDayEventModel = SelectedDay.Events;
                SelectedDayEventTimes = SelectedDayEventModel.EventTimes;

                SelectedDayDayWeather = SelectedDay.SelectedDailyForecast;
                if (SelectedDayCurrentEventTime != null)
                {
                    SelectedDayCurrentEventTimeHourlyWeather = SelectedDayCurrentEventTime.Forecast;
                }

            }
        }

        private void OnWeatherServiceChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Days))
            {
                //Days = new ObservableCollection<DayViewModel>();
                //foreach (var day in SharedSession.Days)
                //{
                //    Days.Add(day);
                //}
                if (Days != null && Days.Any())
                {
                    SelectedDay = Days.First();
                    GenerateCalendar();
                }
            }
        }

        private void GenerateCalendar()
        {
            MonthYear = CurrentDate.ToString("MMMM") + " " + CurrentDate.ToString("yyyy");
            var daysInMonth = DateTime.DaysInMonth(CurrentDate.Year, CurrentDate.Month);
            var firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            var startingDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Build a lookup of any existing day information from the weather service by date
            var weatherLookup = (Days ?? new ObservableCollection<DayViewModel>())
                .Where(d => d != null)
                .ToDictionary(d => d.Date.Date, d => d);

            Days.Clear();
            if (Days.Any(f => f.Day == 1) || Days.First().Date.Month != Days.Last().Date.Month)
            {

                // Add empty cells for days before the first of the month
                for (int i = 0; i < startingDayOfWeek; i++)
                {
                    Days.Add(new DayViewModel { IsEmpty = true });
                }

                // Add each day of the month, prefilling from weatherLookup when available
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var dayDate = new DateTime(CurrentDate.Year, CurrentDate.Month, day);

                    weatherLookup.TryGetValue(dayDate.Date, out var existing);

                    DayViewModel vm = existing != null ? existing : new DayViewModel();

                    vm.Day = day;
                    vm.Date = dayDate;
                    vm.IsToday = dayDate.Date == DateTime.Today;
                    //vm.IsEventDay = existing != null && existing.EventModel != null && existing.EventModel.EventTimes.Any();

                    // Event window: any existing event within +/- 3 days
                    vm.IsEventWindowDay = false;
                    if (!vm.IsEventDay && weatherLookup.Keys.Any(k => k != dayDate.Date && Math.Abs((k - dayDate.Date).TotalDays) <= 3))
                    {
                        vm.IsEventWindowDay = true;
                    }

                    Days.Add(vm);
                }

                // Replace the observable collection so bindings update to the new month
                Days = Days;
            }
        }


        public void NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            GenerateCalendar();
        }

        public void PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            GenerateCalendar();
        }
    }
}
