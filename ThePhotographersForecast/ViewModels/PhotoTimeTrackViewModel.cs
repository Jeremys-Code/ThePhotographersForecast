using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class PhotoTimeTrackViewModel : ObservableObject
    {

        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string LocationTime { get; set; } = string.Empty;
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();
        [ObservableProperty]
        public partial DateTime SelectedPhotoDate { set; get; } = new DateTime();
        [ObservableProperty]
        public partial ObservableCollection<EventTime> Events { get; set; } = new ObservableCollection<EventTime>();
        [ObservableProperty]
        public partial EventTime SelectedEvent { get; set; } = new EventTime();
        [ObservableProperty]
        public partial ObservableCollection<PhotoSchedule> HourSchedules { get; set; } = new ObservableCollection<PhotoSchedule>();
        [ObservableProperty]
        public partial PhotoSchedule SelectedHourSchedule { get; set; } = new PhotoSchedule();
        [ObservableProperty]
        public partial ObservableCollection<DaySchedule> Schedules { get; set; } = new ObservableCollection<DaySchedule>();
        [ObservableProperty]
        public partial DaySchedule SelectedDaySchedule { get; set; } = new DaySchedule();
        [ObservableProperty]
        public partial DateTime[] CalendarDays { get; set; } = Array.Empty<DateTime>();
        [ObservableProperty]
        public partial DateTime SelectedCalendarDay { get; set; } = new DateTime();
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial string ProgressMessage { get; set; } = string.Empty;
        //        public IRelayCommand<DateOnly> OnSelectedCalendarDayChangedCommand { get; }


        private ISessionStore _ss;
        public PhotoTimeTrackViewModel(ISessionStore ss)
        {
            _ss = ss;

            CalendarDays = _ss.DailyStartTime.OrderBy(x => x).ToArray();


            if (CalendarDays != null && _ss.DaySchedules != null)
            {
                SelectedCalendarDay = CalendarDays.First();
                var daySchedule = _ss.DaySchedules.Where(s => s.PhotoDate == SelectedCalendarDay).First<DaySchedule>();
                SelectedDaySchedule = daySchedule;
                SelectedLocation = _ss.SelectedLocation;
                SelectedPhotoDate = SelectedCalendarDay;
                HourSchedules = SelectedDaySchedule.PhotoSchedules;
                SelectedHourSchedule = HourSchedules.First();
                Events = SelectedHourSchedule.Events;
                SelectedEvent = Events.First();
            }

            LocationTime = DateTime.UtcNow.AddHours(_ss.SelectedLocation.TimeZoneOffset).ToString("F");
            _timer = new System.Timers.Timer(1000); // 1 second interval
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

        }

        [RelayCommand]
        partial void OnSelectedCalendarDayChanged(DateTime value)
        {
            var daySchedule = _ss.DaySchedules.Where(s => s.PhotoDate == value).First<DaySchedule>();

            SelectedDaySchedule = daySchedule;
            SelectedLocation = _ss.SelectedLocation;
            SelectedPhotoDate = value;
            HourSchedules = SelectedDaySchedule.PhotoSchedules;
            SelectedHourSchedule = HourSchedules.First();
            Events = SelectedHourSchedule.Events;
            SelectedEvent = Events.First();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            LocationTime = DateTime.UtcNow.AddHours(_ss.SelectedLocation.TimeZoneOffset).ToString("F");
        }

    }
}