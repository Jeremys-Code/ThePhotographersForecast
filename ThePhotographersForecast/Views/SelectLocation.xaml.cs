using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.ViewModels;
using static ThePhotographersForecast.Models.PhotoLocation;

namespace ThePhotographersForecast.Views;

public partial class SelectLocation : ContentPage
{
    private HttpClient _client;
    private SelectLocationViewModel _vm;
    private CancellationTokenSource? _cts;
    public SelectLocation(SelectLocationViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        _client = new HttpClient();
        InputLocation.TextChanged += InputLocation_TextChanged;

    }

    private async void InputLocation_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length < 3)
        {
            return;
        }
        Debounce(async () =>
        {
            string input = e.NewTextValue;
            string apiString = $@"https://nominatim.openstreetmap.org/search?q={input}&format=geocodejson&addressdetails=1&countrycodes=us";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(apiString));
            _client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return;

            var results = await response.Content.ReadFromJsonAsync<GeocodeResult.Rootobject>();
            _vm.Suggestions.Clear();
            _vm.ShowSuggestions = false;
            if (results != null)
            {
                for (int i = 0; i < results.Property1.Length; i++)
                {
                    var result = results.Property1[i];
                    var bbox = result.boundingbox;

                    var coords = new LocationCoordinates
                    {
                        SouthWest = (Latitude: bbox[0], Longitude: bbox[2]),
                        NorthEast = (Latitude: bbox[1], Longitude: bbox[3])
                    };
                    _vm.Suggestions.Add(new Models.PhotoLocation
                    {
                        Latitude = Double.Parse(result.lat),
                        Longitude = Double.Parse(result.lon),
                        Village = result.address.village ?? string.Empty,
                        Town = result.address.town ?? string.Empty,
                        Suburb = result.address.suburb ?? string.Empty,
                        Neighborhood = result.address.neighbourhood ?? string.Empty,
                        City = result.address.city ?? string.Empty,
                        State = result.address.state,
                        Country = result.address.country,
                        SpatialCoordinates = coords
                    });
                }

                _vm.ShowSuggestions = true;

                return;
            }
            return;
        }, TimeSpan.FromMilliseconds(800));
    }
    /// <summary>
    /// Debounces an action so it only runs after the specified delay without new calls.
    /// </summary>
    private void Debounce(Func<Task> action, TimeSpan delay)
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