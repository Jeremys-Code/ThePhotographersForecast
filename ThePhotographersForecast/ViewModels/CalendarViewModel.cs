using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        [ObservableProperty]
        private GetPhotoDay _photoDayService;
        [ObservableProperty]
        private System.Timers.Timer _timer;
        [ObservableProperty]
        private string greetingLine1;
        [ObservableProperty]
        private string greetingLine2;
        [ObservableProperty]
        private string greetingLine3;
        [ObservableProperty]
        private SelectLocationViewModel slvm;
        [ObservableProperty]
        private int utcOffset;
        [ObservableProperty]
        private string locationTime;
        [ObservableProperty]
        private string currentTime;
        [ObservableProperty]
        private PhotoDay[] photoDays;
        [ObservableProperty]
        private GfsDailyForecast[] selectedDayDailyForecasts;
        [ObservableProperty]
        private GfsDailyForecast selectedDayCurrentDailyForecast;
        [ObservableProperty]
        private GfsHourlyForecast[] selectedDayHourlyForecasts;
        [ObservableProperty]
        private GfsHourlyForecast selectedDayCurrentHourlyForecast;
        //[ObservableProperty]
        //private ObservableCollection<EventTime> selectedEvent;
        [ObservableProperty]
        private PhotoDay selectedDay;
        [ObservableProperty]
        private bool isEventDay = false;
        [ObservableProperty]
        private bool isEventWindowDay = false;
        [ObservableProperty]
        private bool isToday = false;
        [ObservableProperty]
        private string locationName;
        [ObservableProperty]
        private bool isWorking;
        [ObservableProperty]
        private string _monthYear;
        [ObservableProperty]
        private DateTime currentDate;
        [ObservableProperty]
        private PhotoDay[] days;
        [ObservableProperty]
        private ObservableCollection<PhotoSchedule> schedules = new ObservableCollection<PhotoSchedule>();
        [ObservableProperty]
        private PhotoHour selectedHour;
        [ObservableProperty]
        private bool isReady = false;
        [ObservableProperty]
        private string progressMessage;

        private readonly SelectLocationViewModel _slvm;

        public IAsyncRelayCommand<PhotoDay> FlipCommand { get; }
        public Command SelectedPhotoDayChangedCommand { get; }

        public CalendarViewModel(SelectLocationViewModel slvm)
        {
            _slvm = slvm;

            if (!_slvm.IsComplete)
            {
                return;
            }
            if (_slvm.GfsData.DailyForecasts.Length == _slvm.ForecastCount &&
               _slvm.EventsModel.EventTimes.Count == _slvm.EventCount &&
               _slvm.PhotoDays.Length == _slvm.NumberOfPhotoDays)
            {

                CurrentDate = _slvm.PhotoStartDate;
                if (_slvm.PhotoDays.Length == PhotoDays.Length)
                {
                    IsWorking = true;
                }
                int index = _slvm.PhotoDays.Length;
                Days = new PhotoDay[index];
                _slvm.PhotoDays.CopyTo(Days);

                SelectedDay = Days[0];
                SelectedDayDailyForecasts = SelectedDay.DailyForecasts;
                SelectedDayCurrentDailyForecast = SelectedDay.DailyForecasts[0];
                SelectedDayHourlyForecasts = SelectedDay.HourlyForecasts;
                SelectedDayCurrentHourlyForecast = SelectedDay.HourlyForecasts[0];

                SelectedDay.PropertyChanged += OnDateChanged;

                LocationTime = DateTime.UtcNow.AddSeconds(ServiceHelper.GetService<PhotoLocation>().TimeZoneOffset).ToString("F");
                FlipCommand = new AsyncRelayCommand<PhotoDay>(OnFlip);
                GenerateCalendar();

                // The page greeting message.

                LocationName = ServiceHelper.GetService<PhotoLocation>().DisplayName;


                // The Local Time of the selected location.
                LocationTime = DateTime.UtcNow.AddSeconds(UtcOffset).ToString("F");
                CurrentTime = DateTime.Now.ToString("F");
                _timer = new System.Timers.Timer(1000); // 1 second interval
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();

            }
        }

        private void OnDateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is PhotoDay changedDay && e.PropertyName == nameof(PhotoDay.Day))
            {
                SelectedDay = Days.FirstOrDefault(d => d.Day == changedDay.Day);
                SelectedDayDailyForecasts = SelectedDay.DailyForecasts;
                SelectedDayCurrentDailyForecast = SelectedDay.DailyForecasts.FirstOrDefault();
                SelectedDayHourlyForecasts = SelectedDay.HourlyForecasts;
                SelectedDayCurrentHourlyForecast = SelectedDay.HourlyForecasts.FirstOrDefault();
            }
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            LocationTime = DateTime.UtcNow.AddSeconds(UtcOffset).ToString("F");
            CurrentTime = DateTime.Now.ToString("F");
        }
        private async Task OnFlip(PhotoDay day)
        {

            if (day == null)
                return;

            day.IsFlipped = !day.IsFlipped;
            SelectedDay = Days.FirstOrDefault(d => d.Day == day.Day);
            if (day.IsFlipped && day.DailyForecasts != null)
            {
                var photoDay = Days.FirstOrDefault(p => p.Day == day.Day);
            }
        }

        private void GenerateCalendar()
        {
            MonthYear = CurrentDate.ToString("MMMM") + " " + CurrentDate.ToString("yyyy");
            var daysInMonth = DateTime.DaysInMonth(CurrentDate.Year, CurrentDate.Month);
            var firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            var startingDayOfWeek = (int)firstDayOfMonth.DayOfWeek;


            for (int i = 0; i < Days.Length; i++)
            {
                Days[i].IsEventDay = true;
                Days[i].IsFlipped = true;
            }
            ;

            // Add empty cells for days before the first of the month
            for (int i = 0; i < startingDayOfWeek; i++)
            {
                Days.Prepend(new PhotoDay { IsEmpty = true });
            }

            var eventDates = new HashSet<DateTime>();
            if (Days != null)
            {
                foreach (var day in Days)
                {
                    eventDates.Add(new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, 0, 0, 0));
                }
            }

            for (int i = 1; i <= daysInMonth; i++)
            {
                var dayDate = new DateTime(Days[i].Date.Year, Days[i].Date.Month, Days[i].Date.Day, 0, 0, 0);
                //var isEvent = Days.Contains(new DateTime(dayDate.Year, dayDate.Month, dayDate.Day, 0, 0, 0)) ?? false;

                var isEventWindow = false;
                if (!Days[i].IsEventDay && Days[i] != null)
                {
                    isEventWindow = Days.Any(pd =>
                        (dayDate.Date >= new DateTime(pd.Date.Year, pd.Date.Month, pd.Date.Day, 0, 0, 0).AddDays(-3) && dayDate.Date < new DateTime(pd.Date.Year, pd.Date.Month, pd.Date.Day, 0, 0, 0)) ||
                        (dayDate.Date > new DateTime(pd.Date.Year, pd.Date.Month, pd.Date.Day, 0, 0, 0) && dayDate.Date <= new DateTime(pd.Date.Year, pd.Date.Month, pd.Date.Day, 0, 0, 0).AddDays(3)));
                }


                Days[i].Day = i;
                Days[i].Date = dayDate;
                Days[i].IsToday = dayDate.Date == DateTime.Today;

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
