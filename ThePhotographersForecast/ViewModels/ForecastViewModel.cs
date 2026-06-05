using CommunityToolkit.Mvvm.ComponentModel;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastViewModel : ObservableObject
    {
        //public event PropertyChangedEventHandler? PropertyChanged;
        //protected void OnPropertyChanged(string name) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        [ObservableProperty]
        private bool isWorking;
        [ObservableProperty]
        private PhotoDayModel photoDays = new PhotoDayModel();
        [ObservableProperty]
        private PhotoDay selectedPhotoDay = new PhotoDay();
        [ObservableProperty]
        private PhotoHour[] photoHours;
        [ObservableProperty]
        private PhotoHour selectedPhotoHour;
        [ObservableProperty]
        private GfsDailyForecast[] dailyForecasts;
        [ObservableProperty]
        private GfsDailyForecast selectedDailyForecast;
        [ObservableProperty]
        private GfsHourlyForecast[] hourlyForecasts;
        [ObservableProperty]
        private GfsHourlyForecast selectedHourlyForecast;
        [ObservableProperty]
        private string placeName;
        [ObservableProperty]
        private System.Timers.Timer _timer;
        [ObservableProperty]
        private string currentTime;

        private readonly SelectLocationViewModel _slvm;
        public ForecastViewModel(SelectLocationViewModel slvm)
        {
            _slvm = slvm;
            PhotoDays = new PhotoDayModel();
            PhotoDays = _slvm.PhotoDays;
            SelectedPhotoDay = PhotoDays.PhotoDays[0];
            HourlyForecasts = SelectedPhotoDay.HourlyForecasts;
            SelectedHourlyForecast = SelectedPhotoDay.HourlyForecasts[0];
            DailyForecasts = SelectedPhotoDay.DailyForecasts;
            SelectedDailyForecast = SelectedPhotoDay.DailyForecasts[0];

            IsWorking = false;
            PlaceName = ServiceHelper.GetService<Models.PhotoLocation>().DisplayName;

            currentTime = DateTime.UtcNow.AddHours(ServiceHelper.GetService<Models.PhotoLocation>().TimeZoneOffset).ToString("F");
            _timer = new System.Timers.Timer(1000); // 1 second interval
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

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
