using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.ViewModels
{
    public partial class SelectLocationViewModel : ObservableObject
    {
        // Use shared session store's GfsData and EventsData to avoid shadowing and ensure persistence
        [ObservableProperty]
        public partial string AutoComplete { get; set; }
        [ObservableProperty]
        public partial List<PhotoLocation> Suggestions { get; set; } = new List<PhotoLocation>();
        [ObservableProperty]
        public partial bool ShowSuggestions { get; set; } = false;
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial string ProgressMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool IsComplete { get; set; } = false;

        public DateTime MinDate => DateTime.Now.Date;

        public DateTime MaxDate => DateTime.Now.AddDays(16).Date;
        [ObservableProperty]
        public partial DateTime StartDate { get; set; } = DateTime.Now.Date;
        [ObservableProperty]
        public partial DateTime EndDate { get; set; } = DateTime.Now.AddDays(7).Date;
        [ObservableProperty]
        public partial bool IsInvalidDates { get; set; } = false;
        [ObservableProperty]
        public partial string InvalidDates { get; set; } = string.Empty;
        [ObservableProperty]
        public partial ObservableCollection<DateTime> Dates { get; set; } = new ObservableCollection<DateTime>();
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> WeatherForecasts { get; set; } = new ObservableCollection<WeatherForecast>();
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();

        /// <summary>
        /// Former ForecastViewModel Properties
        /// </summary>
        [ObservableProperty]
        public partial ImageSource CurrentDailyIcon { get; set; }
        [ObservableProperty]
        public partial string CurrentDailyName { get; set; }
        [ObservableProperty]
        public partial string CurrentDailyWeatherSummary { get; set; }
        [ObservableProperty]
        public partial string CurrentDailySafetySummary { get; set; }
        [ObservableProperty]
        public partial HourlyForecast EventsForecast { get; set; }

        //[ObservableProperty]
        //public partial DateTime currentDailyStart { get; set; } =
        //[ObservableProperty]
        //public partial DateTime currentDailyEnd { get; set; } =
        [ObservableProperty]
        public partial DateTime CurrentHourlyStart { get; set; } = new DateTime();
        [ObservableProperty]
        public partial ImageSource CurrentHourlyIcon { get; set; }
        [ObservableProperty]
        public partial string CurrentHourlyWeatherSummary { get; set; }
        [ObservableProperty]
        public partial string CurrentHourlyName { get; set; }
        [ObservableProperty]
        public partial string CurrentEventName { get; set; }
        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string CurrentTime { get; set; }
        [ObservableProperty]
        public partial string DisplayName { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> Dailys { get; set; } = new ObservableCollection<WeatherForecast>();
        [ObservableProperty]
        public partial ObservableGroupedCollection<string, EventTime> SelectedDailyEvents { get; set; }
        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; } = DateTime.Now.Date;
        [ObservableProperty]
        public partial DailyForecast SelectedDaily { get; set; }

        [ObservableProperty]
        public partial HourlyForecast SelectedEventHourlyWeather { get; set; }
        [ObservableProperty]
        public partial string TimePeriod { get; set; }

        private GetWeatherService _gfs;
        private HttpClient _client;

        //    public Command<PhotoLocation> SuggestionTappedCommand { get; }

        public SelectLocationViewModel(GetWeatherService gfs, IHttpClientFactory client)
        {
            _gfs = gfs;
            AutoComplete = "";
            _client = client.CreateClient("photographer");

            StartDate = DateTime.Now.Date;
            EndDate = DateTime.Now.AddDays(7).Date;

            //          SuggestionTappedCommand = new Command<PhotoLocation>(async (parameter) => await SuggestionTapped(parameter));

        }

        partial void OnSelectedDateChanged(DateTime oldValue, DateTime newValue)
        {
            SelectedDaily = Dailys.Select(x => x.DayForecast).Where(y => new DateTime(y.Date, TimeOnly.MinValue).Date == newValue.Date).First() ?? new DailyForecast();
        }

        // Call Weather Service
        private async Task<bool> GetWeatherDataAsync(double latitude, double longitude)
        {
            if (Dates == null)
            {
                ValidateDates();
            }

            ProgressMessage = "Collecting forecast and astronomical data...";
            try
            {
                WeatherForecasts = await _gfs.BuildWeatherForecastsAsync(latitude, longitude, StartDate, EndDate);
                if (WeatherForecasts.Count > 0)
                {
                    CreateForecastViewBindings();
                    Debug.WriteLine($"[SelectLocationVM] Weather data retrieved successfully");
                    return true;
                }
                Debug.WriteLine($"[SelectLocationVM] GetWeatherAsync returned null");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLocationVM] Error getting weather data. Exception: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
                ProgressMessage = $"Error: {ex.Message}";
            }
            return false;
        }

        private void CreateForecastViewBindings()
        {
            if (WeatherForecasts == null || WeatherForecasts.Count == 0)
            {
                Debug.WriteLine($"[SelectLocationVM] No weather forecasts available to bind");
                return;
            }
            Dailys = new ObservableCollection<WeatherForecast>(WeatherForecasts.OrderBy(x => x.Date));
            SelectedDaily = Dailys.Select(x => x.DayForecast).First() ?? new DailyForecast();
            SelectedDailyEvents = new ObservableGroupedCollection<string, EventTime>(SelectedDaily.DailyEvents.OrderBy(e => e.OrderTime).GroupBy(y => y.TimeOfDay).OrderBy(k => k.Key).ToObservableCollection());


            SelectedDaily = WeatherForecasts.Select(x => x.DayForecast).OrderBy(y => y.TimePeriod).First() ?? new DailyForecast();
            if (SelectedDaily != null)
            {
                SelectedDailyEvents = new ObservableGroupedCollection<string, EventTime>(SelectedDaily.DailyEvents.OrderBy(e => e.OrderTime).GroupBy(y => y.TimeOfDay).OrderBy(k => k.Key).ToObservableCollection());
            }
        }

        [RelayCommand]
        partial void OnStartDateChanged(DateTime value)
        {
            if (value == default)
            {
                return;
            }
            IsInvalidDates = false;
            ValidateDates();
        }
        [RelayCommand]
        partial void OnEndDateChanged(DateTime value)
        {
            if (value == default)
            {
                return;
            }
            IsInvalidDates = false;
            ValidateDates();
        }

        private void ValidateDates()
        {
            if (StartDate > EndDate)
            {
                IsInvalidDates = true;
                InvalidDates = "Please select a Start Date that's before or the same as the End Date.";
                if (EndDate.AddDays(7) <= MaxDate)
                {
                    EndDate = EndDate.AddDays(7);
                }
                else
                {
                    EndDate = MaxDate;
                }
            }

            int nbrOfDays = (int)Math.Round(EndDate.Date.Subtract(StartDate.Date).TotalDays + 1, 0);
            var currentDate = StartDate.Date;
            Dates.Clear();
            for (int i = 0; i < nbrOfDays; i++)
            {
                currentDate = currentDate.AddDays(i).Date;
                Dates.Add(currentDate);
            }
        }

        private async Task GetDataAsync(double latitude, double longitude)
        {
            try
            {
                IsWorking = true;
                Debug.WriteLine($"[SelectLocationVM] Starting GetDataAsync for lat:{latitude} lon:{longitude}");

                var weatherTask = GetWeatherDataAsync(latitude, longitude);
                await weatherTask.ConfigureAwait(false);

                if (weatherTask.IsCompletedSuccessfully && weatherTask.Result)
                {
                    ProgressMessage = "Forecast finished processing";
                    Debug.WriteLine($"[SelectLocationVM] Weather task completed successfully");
                    IsWorking = false;
                }
                else
                {
                    ProgressMessage = "Failed to get weather data";
                    Debug.WriteLine($"[SelectLocationVM] Weather task failed or returned false");
                    IsWorking = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLocationVM] ERROR in GetDataAsync: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
            }
        }


        [RelayCommand]
        private async Task SuggestionTapped(PhotoLocation parameter)
        {
            try
            {
                Debug.WriteLine($"[SelectLocationVM] SuggestionTapped called with parameter: {parameter?.DisplayName}");

                var selectLocation = new PhotoLocation();

                if (parameter != null)
                {
                    selectLocation = parameter;
                    SelectedLocation = selectLocation;
                    Debug.WriteLine($"[SelectLocationVM] SelectedLocation set to: {SelectedLocation.DisplayName}");
                }
                //AutoComplete = SelectedLocation.DisplayName;
                IsWorking = true;
                ProgressMessage = "Getting location info...";

                Suggestions.Clear();
                ShowSuggestions = false;

                Task dataTask = GetDataAsync(SelectedLocation.Latitude, SelectedLocation.Longitude);
                await dataTask;
                Debug.WriteLine($"[SelectLocationVM] SuggestionTapped completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLocationVM] ERROR in SuggestionTapped: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
                ProgressMessage = $"Error: {ex.Message}";
                IsWorking = false;
            }
        }




        //private async Task<List<PhotoLocation?>> GetLocationSuggestions(string userinput)
        //{
        //    var cts = new CancellationTokenSource();

        //    string input = userinput;
        //    Suggestions.Clear();

        //    if (input.Length < 3 || string.IsNullOrEmpty(input))
        //    {

        //        return null;
        //    }
        //    if (input.Length >= 3)
        //    {

        // string apiString = String.Concat($@"https://geocoding-api.open-meteo.com/v1/search?name=", input, @"&count=10&language=en&format=json&countryCode=US");
        //TODO :: Try out a new autocomplete provider
        //           string apiString = String.Concat($@"https://photon.komoot.io/api/?q={input}&limit=10&countrycode=US";
        //        string apiString = $@"https://nominatim.openstreetmap.org/search?q={input}&format=json&addressdetails=1&mapzoom=3&viewbox=-175.53229,70.25859,-40.70808,6.83663";
        //        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(apiString));
        //        var response = await _client.SendAsync(request);
        //        if (!response.IsSuccessStatusCode)
        //            return null;

        //        var results = await response.Content.ReadFromJsonAsync<GeocodeResult.Rootobject>();
        //        Suggestions.Clear();
        //        ShowSuggestions = false;
        //        if (results != null)
        //        {
        //            for (int i = 0; i < results.Property1.Length; i++)
        //            {
        //                var result = results.Property1[i];
        //                var bbox = result.boundingbox;

        //                var coords = new LocationCoordinates
        //                {
        //                    SouthWest = (Latitude: bbox[0], Longitude: bbox[2]),
        //                    NorthEast = (Latitude: bbox[1], Longitude: bbox[3])
        //                };
        //                Suggestions.Add(new Models.PhotoLocation
        //                {
        //                    Latitude = Double.Parse(result.lat),
        //                    Longitude = Double.Parse(result.lon),
        //                    Village = result.address.village ?? string.Empty,
        //                    Town = result.address.town ?? string.Empty,
        //                    Suburb = result.address.suburb ?? string.Empty,
        //                    Neighborhood = result.address.neighbourhood ?? string.Empty,
        //                    City = result.address.city ?? string.Empty,
        //                    State = result.address.state,
        //                    Country = result.address.country,
        //                    SpatialCoordinates = coords
        //                });
        //            }

        //            ShowSuggestions = true;
        //            return Suggestions;
        //        }
        //    }
        //}

        [RelayCommand]
        private async Task UseGps()
        {

            IsWorking = true;
            ProgressMessage = "Getting GPS location...";
            var location = await CurrentLocation.VerifyPermissionAsync();
            if (location != null)
            {
                SelectedLocation.Latitude = location.Value.lat;
                SelectedLocation.Longitude = location.Value.lon;
            }
            //    foreach( var a in AltitudeReferenceSystem)

            Task dataTask = GetDataAsync(SelectedLocation.Latitude, SelectedLocation.Longitude);
            await dataTask;
            if (dataTask.IsCompletedSuccessfully)
            {
                IsWorking = false;
            }
            IsComplete = true;
        }
    }
}
