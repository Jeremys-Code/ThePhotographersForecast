using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class SelectLocationViewModel : ObservableObject
    {
        private CancellationTokenSource? _cts;
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

        public DateTime MinDate => DateTime.Now.Date;

        public DateTime MaxDate => DateTime.Now.AddDays(16).Date;
        [ObservableProperty]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        public partial DateTime EndDate { get; set; }
        [ObservableProperty]
        public partial bool IsInvalidDates { get; set; } = false;
        [ObservableProperty]
        public partial string InvalidDates { get; set; } = string.Empty;




        private GetWeatherService _gfs;
        private HttpClient _client;
        private Session _session;
        private GetEventService _eventService;

        public SelectLocationViewModel(Session session, GetWeatherService gfs, IHttpClientFactory client, GetEventService eventService)
        {
            _session = session;
            _gfs = gfs;
            AutoComplete = "";
            _eventService = eventService;
            _client = client.CreateClient("photographer");


            StartDate = DateTime.Now.Date;
            EndDate = DateTime.Now.AddDays(7).Date;

        }

        [RelayCommand]
        partial void OnStartDateChanged(DateTime value)
        {
            Debounce(async () =>
            {
                if (value == DateTime.Now.Date)
                {
                    return;
                }
                IsInvalidDates = false;
                ValidateDates();
            }, TimeSpan.FromMilliseconds(800));
        }
        [RelayCommand]
        partial void OnEndDateChanged(DateTime value)
        {

            Debounce(async () =>
            {
                if (value == DateTime.Now.AddDays(7).Date)
                {
                    return;
                }
                IsInvalidDates = false;
                ValidateDates();
            }, TimeSpan.FromMilliseconds(800));
        }

        private void ValidateDates()
        {
            if (_session.StartDate > _session.EndDate)
            {
                IsInvalidDates = true;
                InvalidDates = "Please select a Start Date that's before or the same as the End Date.";
                if (_session.EndDate.AddDays(7) <= MaxDate)
                {
                    _session.EndDate = _session.EndDate.AddDays(7);
                }
                else
                {
                    _session.EndDate = MaxDate;
                }
            }
            _session.StartDate = StartDate;
            _session.EndDate = EndDate;


        }

        [RelayCommand]
        private async Task UseGps()
        {
            int nbrOfDays = (int)Math.Round(_session.EndDate.Date.Subtract(_session.StartDate.Date).TotalDays + 1, 0);
            var currentDate = _session.StartDate.Date;
            _session.Dates = new DateTime[nbrOfDays];
            for (int i = 0; i < nbrOfDays; i++)
            {
                currentDate = currentDate.Date;

                _session.Dates[i] = currentDate;
                currentDate = currentDate.Date.AddDays(1);
            }
            try
            {
                IsWorking = true;
                ProgressMessage = "Getting GPS location...";
                var location = await CurrentLocation.VerifyPermissionAsync();
                if (location != null)
                {
                    _session.SelectedLocation.Latitude = location.Value.lat;
                    _session.SelectedLocation.Longitude = location.Value.lon;
                }
                //    foreach( var a in AltitudeReferenceSystem)

                await GetAstronomyEventsAsync();
                if (_session.EventsData == null)
                    return;

                await GetWeatherDataAsync();
                if (_session.WeatherForecasts != null)
                    return;

                Debug.WriteLine($"[SelectLocationVM] SuggestionTapped completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLocationVM] ERROR in SuggestionTapped: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
                ProgressMessage = $"Error: {ex.Message}";
                IsWorking = false;
            }
            IsWorking = false;
            return;
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
                    _session.SelectedLocation = selectLocation;
                    Debug.WriteLine($"[SelectLocationVM] SelectedLocation set to: {_session.SelectedLocation.DisplayName}");
                }
                //AutoComplete = SelectedLocation.DisplayName;
                IsWorking = true;
                ProgressMessage = "Getting location info...";

                Suggestions.Clear();
                ShowSuggestions = false;

                await GetAstronomyEventsAsync();
                if (_session.EventsData == null)
                    return;

                await GetWeatherDataAsync();
                if (_session.WeatherForecasts != null)
                    return;

                Debug.WriteLine($"[SelectLocationVM] SuggestionTapped completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLocationVM] ERROR in SuggestionTapped: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
                ProgressMessage = $"Error: {ex.Message}";
                IsWorking = false;
            }
            IsWorking = false;
            return;
        }

        private async Task GetAstronomyEventsAsync()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressMessage = "Lining up the Sun, the Moon, and stars...";
            });
            await _eventService.GetPhotoDayEventsAsync();
            if (_session.EventsData.Count == _session.Dates.Length)
            {
                return;
            }
            return;
        }

        // Call Weather Service
        private async Task<bool> GetWeatherDataAsync()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressMessage = $"Calling the station in {_session.SelectedLocation.City} for the weather...";
            });

            try
            {
                await _gfs.BuildWeatherForecastsAsync(_session.SelectedLocation, _session.StartDate, _session.EndDate);
                if (_session.WeatherForecasts.Count > 0)
                {

                    Debug.WriteLine($"[SelectLocationVM] Weather data retrieved successfully");
                    IsWorking = false;
                    return true;
                }
                Debug.WriteLine($"[SelectLocationVM] GetWeatherAsync returned null");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLocationVM] Error getting weather data. Exception: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
                ProgressMessage = $"Error: {ex.Message}";

                IsWorking = false;
                return false;
            }
            return false;
        }

        public void Debounce(Func<Task> action, TimeSpan delay)
        {
            // Cancel any pending execution
            _cts?.Cancel();
            _cts?.Dispose();

            // Create a new CTS for the new debounce cycle
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // Run the delayed task
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, token); // Wait for inactivity
                    if (!token.IsCancellationRequested)
                    {
                        await action();
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected when a new event occurs before delay ends
                }
            }, token);
        }
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
