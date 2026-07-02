using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Timers;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string GreetingLine1 { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string GreetingLine2 { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string GreetingLine3 { get; set; } = string.Empty;
        [ObservableProperty]
        public partial int UtcOffset { get; set; }
        [ObservableProperty]
        public partial string LocationTime { get; set; } = string.Empty;
        [ObservableProperty]
        public partial DateTime CurrentTime { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<PhotoDay> Days { get; set; } = new ObservableCollection<PhotoDay>();
        [ObservableProperty]
        public partial PhotoDay SelectedDay { get; set; }
        [ObservableProperty]
        public partial Daily[] SelectedDayDailies { get; set; }
        [ObservableProperty]
        public partial Daily SelectedDayCurrentDaily { get; set; }
        [ObservableProperty]
        public partial Hourly[] SelectedDayHourlies { get; set; }
        [ObservableProperty]
        public partial Hourly SelectedDayCurrentHourly { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<EventTime> SelectedDayCurrentHourlyEvents { get; set; } = new ObservableCollection<EventTime>();
        [ObservableProperty]
        public partial EventTime SelectedDayCurrentHourlyCurrentEvent { get; set; } = new EventTime();

        [ObservableProperty]
        public partial bool IsEventDay { get; set; } = false;
        [ObservableProperty]
        public partial bool IsEventWindowDay { get; set; } = false;
        [ObservableProperty]
        public partial bool IsToday { get; set; } = false;
        [ObservableProperty]
        public partial string LocationName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial string MonthYear { get; set; }
        [ObservableProperty]
        public partial DateTime CurrentDate { get; set; } = new DateTime();

        [ObservableProperty]
        public partial ObservableCollection<PhotoSchedule> Schedules { get; set; } = new ObservableCollection<PhotoSchedule>();
        //[ObservableProperty]
        //public partial Hourly SelectedHourly { get; set; }
        [ObservableProperty]
        public partial bool IsReady { get; set; }
        [ObservableProperty]
        public partial string ProgressMessage { get; set; } = string.Empty;

        private Shared.SessionStore _ss;

        public IAsyncRelayCommand<PhotoDay> FlipCommand { get; }
        public Command<PhotoDay> OnDateChangedCommand { get; }

        public CalendarViewModel(Shared.SessionStore ss)
        {
            _ss = ss;
            LocationName = _ss.SelectedLocation.DisplayName;
            UtcOffset = _ss.SelectedLocation.TimeZoneOffset;
            CurrentDate = _ss.StartDateTime;
            CurrentTime = DateTime.UtcNow.AddHours(UtcOffset);
            MonthYear = CurrentDate.ToString("MMMM of yyyyy");
            IsWorking = true;

            Days = new ObservableCollection<PhotoDay>(_ss.PhotoDays.OrderBy(x => x.Date));
            SelectedDay = Days.OrderBy(x => x.Date).First();
            SelectedDayDailies = SelectedDay.Dailies;
            SelectedDayCurrentDaily = SelectedDay.Dailies.First();
            SelectedDayHourlies = SelectedDay.Hourlies;
            SelectedDayCurrentHourly = SelectedDay.Hourlies.First();
            SelectedDayCurrentHourlyEvents = SelectedDayCurrentHourly.Events;
            SelectedDayCurrentHourlyCurrentEvent = SelectedDayCurrentHourlyEvents.First();


            LocationTime = DateTime.UtcNow.AddHours(UtcOffset).ToString("F");

            _timer = new System.Timers.Timer(1000); // 1 second interval
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            GenerateCalendar();
            IsWorking = false;
            FlipCommand = new AsyncRelayCommand<PhotoDay>(OnFlip);
            OnDateChangedCommand = new Command<PhotoDay>(OnDateChanged);
        }

        private void OnDateChanged(PhotoDay changedDay)
        {

            SelectedDay = Days.First(d => d.Day == changedDay.Day);
            SelectedDayDailies = SelectedDay.Dailies;
            SelectedDayCurrentDaily = SelectedDay.Dailies.First();
            SelectedDayHourlies = SelectedDay.Hourlies;
            SelectedDayCurrentHourly = SelectedDay.Hourlies.First();
            SelectedDayCurrentHourlyEvents = SelectedDayCurrentHourly.Events;
            SelectedDayCurrentHourlyCurrentEvent = SelectedDayCurrentHourlyEvents.First();

        }
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            LocationTime = DateTime.UtcNow.AddHours(UtcOffset).ToString("F");
        }

        private async Task OnFlip(PhotoDay day)
        {

            if (day == null)
                return;

            day.IsFlipped = !day.IsFlipped;
            SelectedDay = Days.FirstOrDefault(d => d.Day == day.Day) ?? new PhotoDay();
            if (day.IsFlipped && day.Dailies != null)
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


            if (Days != null)
            {
                for (int i = 0; i < Days.Count; i++)
                {
                    var d = Days[i];
                    if (d == null) continue;
                    d.IsEventDay = true;
                    d.IsFlipped = true;
                }
            }
            ;

            // Add empty cells for days before the first of the month
            for (int i = 0; i < startingDayOfWeek; i++)
            {
                Days.Insert(0, new PhotoDay { IsEmpty = true });
            }

            var eventDates = new HashSet<DateTime>();
            if (Days != null)
            {
                foreach (var day in Days)
                {
                    eventDates.Add(new DateTime(day.Date.Year, day.Date.Month, day.Date.Day, 0, 0, 0));
                }
            }

            for (int dayNum = 1; dayNum <= daysInMonth; dayNum++)
            {
                var dayDate = new DateTime(CurrentDate.Year, CurrentDate.Month, dayNum, 0, 0, 0);

                // Find a matching PhotoDay by date
                var dayEntry = Days.FirstOrDefault(d => d != null && d.Date != default && d.Date.Date == dayDate.Date);
                if (dayEntry == null)
                {
                    // No matching PhotoDay for this date - skip
                    continue;
                }

                var isEventWindow = false;
                if (!dayEntry.IsEventDay)
                {
                    isEventWindow = Days.Any(pd => pd != null && pd.Date != default &&
                        ((dayDate.Date >= pd.Date.Date.AddDays(-3) && dayDate.Date < pd.Date.Date) ||
                         (dayDate.Date > pd.Date.Date && dayDate.Date <= pd.Date.Date.AddDays(3))));
                }
                else
                {
                    dayEntry.Day = dayNum;
                    dayEntry.Date = dayDate;
                    dayEntry.IsToday = dayDate.Date == DateTime.Today;
                    // if PhotoDay has an IsEventWindow property it can be set here; keep ViewModel-level flag unchanged
                    dayEntry.IsEventDay = true;
                    dayEntry.IsEventWindow = isEventWindow;
                }


            }

        }
        [RelayCommand]
        public void NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            GenerateCalendar();
        }

        [RelayCommand]
        public void PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            GenerateCalendar();
        }



    }
}
