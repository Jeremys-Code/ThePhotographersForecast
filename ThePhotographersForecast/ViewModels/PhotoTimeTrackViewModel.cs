using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class PhotoTimeTrackViewModel : ObservableObject
    {

        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string LocationTime { get; set; }
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();
        [ObservableProperty]
        public partial DateTime SelectedPhotoDate { set; get; } = new DateTime();
        [ObservableProperty]
        public partial ObservableCollection<EventTime> Events { get; set; } = new ObservableCollection<EventTime>();
        [ObservableProperty]
        public partial EventTime SelectedEvent { get; set; } = new EventTime();
        [ObservableProperty]
        public partial PhotoSchedule[] HourSchedules { get; set; } = Array.Empty<PhotoSchedule>();
        [ObservableProperty]
        public partial PhotoSchedule SelectedHourSchedule { get; set; } = new PhotoSchedule();
        [ObservableProperty]
        public partial ObservableCollection<DaySchedule> Schedules { get; set; } = new ObservableCollection<DaySchedule>();
        [ObservableProperty]
        public partial DaySchedule SelectedDaySchedule { get; set; } = new DaySchedule();
        [ObservableProperty]
        public partial DateTime[] CalendarDays { get; set; } = Array.Empty<DateTime>();
        [ObservableProperty]
        public partial DateTime SelectedCalendarDay { get; set; }
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial string ProgressMessage { get; set; } = string.Empty;
        public Command<DateTime> OnSelectedCalendarDayChangedCommand =>
                                new Command<DateTime>(p => OnSelectedCalendarDayChanged(p));

        private SelectLocationViewModel _slvm;
        private GetDaySchedules _gds;
        private PhotoLocation _location;
        public PhotoTimeTrackViewModel(SelectLocationViewModel slvm, GetDaySchedules gds, PhotoLocation location)
        {
            _slvm = slvm;
            _gds = gds;
            _location = location;
            var results = CreateScheduleAsync().GetAwaiter().GetResult();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsWorking = true;

                if (CalendarDays != null && Schedules != null)
                {
                    SelectedCalendarDay = CalendarDays.First();
                    var daySchedule = Schedules.Where(s => s.PhotoDate == SelectedCalendarDay).First<DaySchedule>();
                    SelectedDaySchedule = daySchedule;
                    SelectedLocation = ServiceHelper.GetService<PhotoLocation>();
                    DateTime photoDate = SelectedDaySchedule.PhotoDate;
                    SelectedPhotoDate = photoDate;
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

            LocationTime = DateTime.UtcNow.AddHours(_location.TimeZoneOffset).ToString("F");
            _timer = new System.Timers.Timer(1000); // 1 second interval
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

        }
        // Organizes the data into Day Scedules
        private async Task<bool> CreateScheduleAsync()
        {
            try
            {
                var data = _gds.GetSchedules();
                if (data.Count() == _slvm.NumberOfPhotoDays)
                {
                    CalendarDays = new DateTime[_slvm.NumberOfPhotoDays];
                    var scheduleDates = data.Select(s => s.PhotoDate).OrderBy(x => x).ToArray();
                    for (int p = 0; p < scheduleDates.Length; p++)
                    {
                        CalendarDays[p] = scheduleDates[p];
                    }
                    var sorted = data.OrderBy(x => x.PhotoDate).ToArray();
                    Schedules = new ObservableCollection<DaySchedule>(sorted);
                    if (Schedules.Count == CalendarDays.Length)
                    {
                        return true;
                    }
                }
                else
                {
                    throw new ArithmeticException("The Photo Days do not equal the correct number of days");
                }
            }
            catch (ArithmeticException ae)
            {
                Debug.WriteLine(ae.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating photo days. Please try again. {ex}");
            }
            return false;
        }

        partial void OnSelectedCalendarDayChanged(DateTime value)
        {
            var daySchedule = Schedules.Where(s => s.PhotoDate == value).First<DaySchedule>();

            SelectedDaySchedule = daySchedule;
            SelectedLocation = ServiceHelper.GetService<PhotoLocation>();
            DateTime photoDate = SelectedDaySchedule.PhotoDate;
            SelectedPhotoDate = photoDate;
            HourSchedules = SelectedDaySchedule.PhotoSchedules;
            SelectedHourSchedule = HourSchedules.First();
            Events = SelectedHourSchedule.Events;
            SelectedEvent = Events.First();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            LocationTime = DateTime.UtcNow.AddHours(ServiceHelper.GetService<PhotoLocation>().TimeZoneOffset).ToString("F");
        }

        // The CommunityToolkit automatically generates OnPhotoTimeScheduleChangeCommand
        // No need for a manual RelayCommand property or async Task here since it's just setting a property.

    }
}