using CommunityToolkit.Mvvm.ComponentModel;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial PhotoDay[] PhotoDays { get; set; } = Array.Empty<PhotoDay>();
        [ObservableProperty]
        public partial PhotoDay SelectedPhotoDay { get; set; } = new PhotoDay();
        [ObservableProperty]
        public partial PhotoHour[] PhotoHours { get; set; } = Array.Empty<PhotoHour>();
        [ObservableProperty]
        public partial PhotoHour SelectedPhotoHour { get; set; } = new PhotoHour();
        [ObservableProperty]
        public partial GfsDailyForecast[] DailyForecasts { get; set; } = Array.Empty<GfsDailyForecast>();
        [ObservableProperty]
        public partial GfsDailyForecast SelectedDailyForecast { get; set; } = new GfsDailyForecast();
        [ObservableProperty]
        public partial GfsHourlyForecast[] HourlyForecasts { get; set; } = Array.Empty<GfsHourlyForecast>();

        [ObservableProperty]
        public partial GfsHourlyForecast SelectedHourlyForecast { get; set; } = new GfsHourlyForecast();
        [ObservableProperty]
        public partial string PlaceName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string CurrentTime { get; set; } = string.Empty;

        private SelectLocationViewModel _slvm;
        private PhotoLocation _location;
        public ForecastViewModel(SelectLocationViewModel slvm, PhotoLocation location)
        {
            _slvm = slvm;
            _location = location;
            PhotoDays = new PhotoDay[_slvm.PhotoDays.Length];
            for (int a = 0; a < _slvm.PhotoDays.Length; a++)
            {
                PhotoDays[a] = _slvm.PhotoDays[a];
            }
            SelectedPhotoDay = PhotoDays.First();
            HourlyForecasts = SelectedPhotoDay.HourlyForecasts;
            SelectedHourlyForecast = SelectedPhotoDay.HourlyForecasts.First();
            DailyForecasts = SelectedPhotoDay.DailyForecasts;
            SelectedDailyForecast = SelectedPhotoDay.DailyForecasts.First();

            IsWorking = false;
            PlaceName = ServiceHelper.GetService<Models.PhotoLocation>().DisplayName;

            CurrentTime = DateTime.UtcNow.AddHours(_location.TimeZoneOffset).ToString("F");
            _timer = new System.Timers.Timer(1000); // 1 second interval
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
            _location = location;
        }

        private bool IsAvailable()
        {

            if (_slvm.GfsData.DailyForecasts.Count() == _slvm.ForecastCount && _slvm.EventsModel.EventTimes.Count == _slvm.EventCount)
            {
                return true;
            }
            else
            {
                IsAvailable();
            }
            return false;
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.UtcNow.AddHours(ServiceHelper.GetService<Models.PhotoLocation>().TimeZoneOffset).ToString("F");
        }
    }
}
