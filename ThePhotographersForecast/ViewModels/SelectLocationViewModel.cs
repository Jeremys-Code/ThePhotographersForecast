using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class SelectLocationViewModel : ObservableObject
    {
        [ObservableProperty]
        private GfsWeatherModel gfsData = new GfsWeatherModel();
        [ObservableProperty]
        private EventModel eventsModel = new EventModel();
        [ObservableProperty]
        private double latitude;
        [ObservableProperty]
        private double longitude;
        [ObservableProperty]
        private string location = string.Empty;
        [ObservableProperty]
        private List<PhotoLocation> suggestions = new List<PhotoLocation>();
        [ObservableProperty]
        private bool showSuggestions = false;
        [ObservableProperty]
        private bool isWorking = false;
        [ObservableProperty]
        private string progressMessage = string.Empty;
        [ObservableProperty]
        private int forecastCount;
        [ObservableProperty]
        private int hourlyCount;
        [ObservableProperty]
        private int eventCount;
        [ObservableProperty]
        private PhotoLocation selectedLocation;
        [ObservableProperty]
        private DateTime photoStartDate;
        [ObservableProperty]
        private DateTime photoEndDate;
        [ObservableProperty]
        private int numberOfPhotoDays;
        [ObservableProperty]
        private PhotoDayModel photoDays = new PhotoDayModel();
        [ObservableProperty]
        private bool isComplete = false;
        [ObservableProperty]
        private ObservableCollection<DaySchedule> schedules = new ObservableCollection<DaySchedule>();
        [ObservableProperty]
        private ObservableCollection<DateTime> calendarDays = new ObservableCollection<DateTime>();


        private HttpClient _client;
        private readonly GetGfsData _gfs;
        private readonly GetTimesOfDayData _gtodd;
        private readonly GetPhotoDay _gpd;
        private readonly GetDaySchedules _schedules;
        public Command<PhotoLocation> SuggestionTappedCommand { get; }
        public Command UseGpsCommand { get; }

        //public event PropertyChangedEventHandler? PropertyChanged;
        //protected void OnPropertyChanged(string name) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public SelectLocationViewModel(GetGfsData gfs, GetTimesOfDayData gtodd, GetPhotoDay gpd, GetDaySchedules schedules)
        {
            _gfs = gfs;
            _gtodd = gtodd;
            _gpd = gpd;
            _schedules = schedules;
            _client = new HttpClient();
            SuggestionTappedCommand = new Command<PhotoLocation>(async (parameter) => await SuggestionTapped(parameter));
            UseGpsCommand = new Command(async () => await UseGps());
        }

        partial void OnForecastCountChanging(int value)
        {
            ProgressMessage = $"Receiving {value} daily forecasts.";
        }

        partial void OnHourlyCountChanging(int value)
        {
            ProgressMessage = $"Receiving {value} hourly forecasts.";
        }
        partial void OnEventCountChanging(int value)
        {
            ProgressMessage = $"Receiving {value} day events.";
        }

        private async Task<GfsWeatherModel> GetWeatherDataAsync(double latitude, double longitude)
        {
            try
            {
                var weatherResults = await _gfs.GetWeatherAsync(latitude, longitude);
                if (weatherResults != null)
                {
                    ForecastCount = weatherResults.DailyForecasts.Length;
                    HourlyCount = weatherResults.HourlyForecasts.Length;
                    return weatherResults;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getting the weather data. Exception: " + ex.Message);
            }
            return null;
        }
        private async Task<EventModel> GetEventDataAsync(double latitude, double longitude)
        {

            try
            {
                var eventsResults = await _gtodd.GetDaysAsync(latitude, longitude);
                if (eventsResults != null)
                {
                    EventCount = eventsResults.EventTimes.Count;
                    return eventsResults;
                }
            }
            catch (Exception ex)
            {
                ProgressMessage = $"Error getting event data. Please try again. {ex}";
            }
            return null;
        }

        private async Task GoToCalendarAsync()
        {
            if (GfsData != null && ForecastCount > 0 && EventCount > 0 && HourlyCount > 0 && GfsData.DailyForecasts.Length == ForecastCount && EventsModel.EventTimes.Count == EventCount && GfsData.HourlyForecasts.Length == HourlyCount)
            {
                List<DateTime> dates = new List<DateTime>();
                var dailyStartDate = GfsData.DailyForecasts.Min(s => s.StartTime);
                var hourlyStartDate = GfsData.HourlyForecasts.Min(n => n.StartTime);
                var eventStartDate = EventsModel.EventTimes.Min(e => e.WeatherHour);
                dates.AddRange(dailyStartDate, hourlyStartDate, eventStartDate);
                PhotoStartDate = dates.Max();
                List<DateTime> dates2 = new List<DateTime>();
                ;
                var dailyEndDate = GfsData.DailyForecasts.Max(m => m.EndTime);
                var hourlyEndDate = GfsData.HourlyForecasts.Max(m => m.EndTime);
                var eventEndDate = EventsModel.EventTimes.Max(e => e.WeatherHour);
                dates2.AddRange(dailyEndDate, hourlyEndDate, eventEndDate);
                PhotoEndDate = dates2.Min();

                NumberOfPhotoDays = PhotoEndDate.Date.Subtract(PhotoStartDate.Date).Days + 1;
                try
                {
                    var results = await GetPhotoDay.CreatePhotoDayAsync();
                    if (results != null)
                    {
                        PhotoDays = results;
                        if (PhotoDays.PhotoDays.Length > 0 && PhotoDays.PhotoDays.Length == NumberOfPhotoDays)
                        {
                            ProgressMessage = "All data received.";
                            IsWorking = false;
                            await Shell.Current.DisplayAlertAsync("Success", "All data received. You can now view the timeline.", "OK");

                        }

                        var data = _schedules.GetSchedules();
                        if (data != null)
                        {
                            CalendarDays = new ObservableCollection<DateTime>(data.Select(s => s.PhotoDate).ToArray());
                            var sorted = data.OrderBy(x => x.PhotoDate).ToArray();
                            Schedules = new ObservableCollection<DaySchedule>(sorted);
                            if (Schedules.Count > 0)
                            {
                                await Shell.Current.DisplayAlertAsync("PhotoSchedule", "Finished.", "OK");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProgressMessage = $"Error creating photo days. Please try again. {ex}";
                }
                await NavigationService.NavigateToPhotoTimeTrackAsync();
                return;
            }
            else
            {
                ProgressMessage = "Error receiving data. Please try again.";
            }
        }

        private async Task SuggestionTapped(PhotoLocation parameter)
        {
            SelectedLocation = parameter;
            IsWorking = true;
            ProgressMessage = "Getting location info...";

            Suggestions.Clear();
            ShowSuggestions = false;
            ServiceHelper.GetService<PhotoLocation>().Latitude = parameter.Latitude;
            ServiceHelper.GetService<PhotoLocation>().Longitude = parameter.Longitude;
            var weatherData = await GetWeatherDataAsync(parameter.Latitude, parameter.Longitude);
            if (weatherData != null)
            {
                GfsData = weatherData;
            }
            var eventData = await GetEventDataAsync(parameter.Latitude, parameter.Longitude);
            if (eventData != null)
            {
                EventsModel = eventData;
            }

            await GoToCalendarAsync();
            return;
        }



        private async Task UseGps()
        {

            IsWorking = true;
            ProgressMessage = "Getting GPS location...";
            var location = await CurrentLocation.VerifyPermissionAsync();
            if (location != null)
            {
                ServiceHelper.GetService<PhotoLocation>().Latitude = location.Value.lat;
                ServiceHelper.GetService<PhotoLocation>().Longitude = location.Value.lon;
                var weatherData = await GetWeatherDataAsync(location.Value.lat, location.Value.lon);
                if (weatherData != null)
                {
                    GfsData = weatherData;
                }
                var eventData = await GetEventDataAsync(location.Value.lat, location.Value.lon);
                if (eventData != null)
                {
                    EventsModel = eventData;
                }

                await GoToCalendarAsync();
                return;
            }
            else
            {
                return;
            }
        }
    }
}