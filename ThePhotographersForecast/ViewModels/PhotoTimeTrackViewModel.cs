using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class PhotoTimeTrackViewModel : ObservableObject
    {
        [ObservableProperty]
        private System.Timers.Timer _timer;
        [ObservableProperty]
        private string locationTime;
        [ObservableProperty]
        private PhotoLocation selectedLocation = new PhotoLocation();
        [ObservableProperty]
        private DateTime selectedPhotoDate = new DateTime();
        [ObservableProperty]
        private ObservableCollection<EventTime> events = new ObservableCollection<EventTime>();
        [ObservableProperty]
        private EventTime selectedEvent = new EventTime();
        [ObservableProperty]
        private PhotoSchedule[] hourSchedules;
        [ObservableProperty]
        private PhotoSchedule selectedHourSchedule = new PhotoSchedule();
        [ObservableProperty]
        private ObservableCollection<DaySchedule> schedules = new ObservableCollection<DaySchedule>();
        [ObservableProperty]
        private DaySchedule selectedDaySchedule = new DaySchedule();
        [ObservableProperty]
        private ObservableCollection<DateTime> calendarDays = new ObservableCollection<DateTime>();
        [ObservableProperty]
        private DateTime selectedCalendarDay;
        [ObservableProperty]
        private bool isWorking = false;
        [ObservableProperty]
        private string progressMessage;
        public RelayCommand<DateTime> OnSelectedCalendarDayChangedCommand { get; }

        private SelectLocationViewModel _slvm;

        public PhotoTimeTrackViewModel(SelectLocationViewModel slvm)
        {
            _slvm = slvm;
            OnSelectedCalendarDayChangedCommand = new RelayCommand<DateTime>(param => { OnSelectedCalendarDayChanged(param); });
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsWorking = true;
                if (_slvm.CalendarDays != null)
                {
                    CalendarDays = calendarDays;
                    SelectedCalendarDay = CalendarDays.First();
                }
                if (_slvm.Schedules != null)
                {
                    Schedules = _slvm.Schedules;
                    SelectedLocation = ServiceHelper.GetService<PhotoLocation>();
                    SelectedDaySchedule = Schedules.First(c => c.PhotoDate == SelectedCalendarDay);
                    SelectedPhotoDate = SelectedDaySchedule.PhotoDate;
                    HourSchedules = SelectedDaySchedule.PhotoSchedules;
                    SelectedHourSchedule = HourSchedules.First();
                    Events = SelectedHourSchedule.Events;
                    SelectedEvent = Events.First();
                }
            });

            //PhotoSchedulesCollection = new PhotoDayScheduleCollection();
            //PhotoSchedulesCollection.PhotoSchedules = new ObservableCollection<PhotoSchedule>();
            //// 2. Connect the data on the main thread
            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    var data = _getPhotoTimeLine.GetSchedules();
            //    if (data != null)
            //    {
            //        var sorted = data.OrderBy(x => x.PhotoDateTime).ToArray();
            //        Schedules = new ObservableCollection<PhotoSchedule>(sorted);
            //        SelectedSchedule = Schedules.First();
            //        //SelectedPhotoDateTime = SelectedSchedule.PhotoDateTime;
            //        //SelectedHourlyForecast = SelectedSchedule.HourlyForecast;
            //        //SelectedDailyForecast = SelectedSchedule.DailyForecast;
            //        Events = SelectedSchedule.Events;
            //        SelectedEvent = Events.First();
            Shell.Current.DisplayAlertAsync("Finished", "Did it work?", "OK");
            //    }
            //});

            LocationTime = DateTime.UtcNow.AddHours(ServiceHelper.GetService<PhotoLocation>().TimeZoneOffset).ToString("F");
            _timer = new System.Timers.Timer(1000); // 1 second interval
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

        }

        partial void OnSelectedCalendarDayChanged(DateTime value)
        {
            SelectedDaySchedule = Schedules.Where(c => c.PhotoDate == value).First();
            SelectedPhotoDate = SelectedDaySchedule.PhotoDate;
            HourSchedules = SelectedDaySchedule.PhotoSchedules;
            SelectedHourSchedule = HourSchedules.First();
            Events = SelectedHourSchedule.Events;
            SelectedEvent = Events.First();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            LocationTime = DateTime.UtcNow.AddHours(ServiceHelper.GetService<PhotoLocation>().TimeZoneOffset).ToString("F");
        }

        // The CommunityToolkit automatically generates OnPhotoTimeScheduleChangeCommand
        // No need for a manual RelayCommand property or async Task here since it's just setting a property.

    }
}