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
        public partial GfsWeatherModel GfsData { get; set; } = new GfsWeatherModel();
        [ObservableProperty]
        public partial EventModel EventsModel { get; set; } = new EventModel();
        [ObservableProperty]
        public partial List<PhotoLocation> Suggestions { get; set; } = new List<PhotoLocation>();
        [ObservableProperty]
        public partial bool ShowSuggestions { get; set; } = false;
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial string ProgressMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial int ForecastCount { get; set; } = 0;
        [ObservableProperty]
        public partial int HourlyCount { get; set; } = 0;
        [ObservableProperty]
        public partial int EventCount { get; set; } = 0;
        [ObservableProperty]
        public partial DateTime PhotoStartDate { get; set; }
        [ObservableProperty]
        public partial DateTime PhotoEndDate { get; set; }
        [ObservableProperty]
        public partial int NumberOfPhotoDays { get; set; }
        [ObservableProperty]
        public partial PhotoDay[] PhotoDays { get; set; } = Array.Empty<PhotoDay>();
        [ObservableProperty]
        public partial bool IsComplete { get; set; } = false;


        private GetGfsData _gfs;
        private GetTimesOfDayData _gtodd;
        private GetPhotoDay _gpd;
        private PhotoLocation _location;
        public Command<PhotoLocation> SuggestionTappedCommand { get; }
        public Command UseGpsCommand { get; }

        public SelectLocationViewModel(GetGfsData gfs, GetTimesOfDayData gtodd, GetPhotoDay gpd, PhotoLocation location)
        {

            _gfs = gfs;
            _gtodd = gtodd;
            _gpd = gpd;
            _location = location;

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
        // Call Weather Service
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
            return new GfsWeatherModel();
        }
        // Call Event Service
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
            return new EventModel();
        }
        // Organize data into date buckets
        private async Task<PhotoDay[]> CreatePhotoDaysAsync()
        {
            if (GfsData != null && ForecastCount > 0 && EventCount > 0 && HourlyCount > 0 && GfsData.DailyForecasts.Length == ForecastCount && EventsModel.EventTimes.Count == EventCount && GfsData.HourlyForecasts.Length == HourlyCount)
            {
                List<DateTime> startDates = new List<DateTime>();
                var dailyStartDate = GfsData.DailyForecasts.Min(s => s.StartTime);
                var hourlyStartDate = GfsData.HourlyForecasts.Min(n => n.StartTime);
                var eventStartDate = EventsModel.EventTimes.Min(e => e.WeatherHour);
                startDates.AddRange(dailyStartDate, hourlyStartDate, eventStartDate);
                PhotoStartDate = startDates.Max();

                List<DateTime> endDates = new List<DateTime>();
                var dailyEndDate = GfsData.DailyForecasts.Max(m => m.EndTime);
                var hourlyEndDate = GfsData.HourlyForecasts.Max(m => m.EndTime);
                var eventEndDate = EventsModel.EventTimes.Max(e => e.WeatherHour);
                endDates.AddRange(dailyEndDate, hourlyEndDate, eventEndDate);
                PhotoEndDate = endDates.Min();

                NumberOfPhotoDays = PhotoEndDate.Date.Subtract(PhotoStartDate.Date).Days + 1;

                try
                {
                    var results = await _gpd.BuildPhotoDayAsync();
                    if (results.Length == NumberOfPhotoDays)
                    {
                        return results;
                    }
                    else
                    {
                        throw new NullReferenceException("No results returned from Get Photo Days.");
                    }
                }
                catch (ArithmeticException ae)
                {
                    Debug.WriteLine(ae.Message);
                }
                catch (NullReferenceException nr)
                {
                    Debug.WriteLine(nr.Message);
                }
            }

            var emptyPhotoDay = new PhotoDay
            {
                IsEmpty = true
            };

            var emptyPhotoDays = new PhotoDay[0];
            emptyPhotoDays[0] = emptyPhotoDay;
            return emptyPhotoDays;

        }

        private async Task<bool> GetDataAsync(double latitude, double longitude)
        {
            var weatherData = await GetWeatherDataAsync(latitude, longitude);
            if (weatherData != null)
            {
                GfsData = weatherData;
            }
            var eventData = await GetEventDataAsync(latitude, longitude);
            if (eventData != null)
            {
                EventsModel = eventData;
            }
            var daysResult = await CreatePhotoDaysAsync();
            if (daysResult != null && daysResult.Count() > 0)
            {
                PhotoDays = daysResult.ToArray();

                if (daysResult.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task SuggestionTapped(PhotoLocation parameter)
        {
            _location = parameter;
            IsWorking = true;
            ProgressMessage = "Getting location info...";

            Suggestions.Clear();
            ShowSuggestions = false;
            IsComplete = await GetDataAsync(_location.Latitude, _location.Longitude);
            if (IsComplete)
            {
                await NavigationService.NavigateToPhotoTimeTrackAsync();
                return;
            }
            return;
        }

        private async Task UseGps()
        {

            IsWorking = true;
            ProgressMessage = "Getting GPS location...";
            var location = await CurrentLocation.VerifyPermissionAsync();
            if (location != null)
            {
                _location.Latitude = location.Value.lat;
                _location.Longitude = location.Value.lon;
            }
            IsComplete = await GetDataAsync(_location.Latitude, _location.Longitude);
            if (IsComplete)
            {
                await NavigationService.NavigateToPhotoTimeTrackAsync();
                return;
            }
            return;
        }
    }
}