using Microsoft.Maui.Controls.Shapes;
using System.Net.Http.Json;
using System.Net.Sockets;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class SelectLocation : ContentPage
{

    private SelectLocationViewModel _vm;
    private CancellationTokenSource? _cts;
    private HttpClient _client;
    const double a = 300; // ellipse radius X
    const double b = 200;  // ellipse radius Y
    const int duration = 3000; // ms
    public SelectLocation(SelectLocationViewModel vm, IHttpClientFactory client)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;
        _client = client.CreateClient("photographer");

        InputLocation.IsEnabled = false;

        // Guarded subscription: ensure the named control exists in XAML



        Welcome.Loaded += async (s, e) => { await Welcome.FadeToAsync(1, 200, Easing.SpringIn); };
        Photographers.Loaded += async (s, e) => { await Photographers.FadeToAsync(1, 1000, Easing.SpringIn); };

        _vm.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(_vm.Progress))
            {
                await DataProgressBar.ProgressTo(_vm.Progress, 500, Easing.BounceIn);

            }
        };
    }

    private void _vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        PercentComplete.Text = $"Completed {_vm.Progress}%";
    }

    private async void InputLocation_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_vm.IsWorking == true)
        {
            InputLocation.Text = _vm.DisplayName;
            InputLocation.IsReadOnly = true;
        }
        if (_vm.StopQuerying)
        {
            InputLocation.TextChanged -= InputLocation_TextChanged;
            return;
        }
        if (e.NewTextValue.Length < 3)
        {
            return;
        }
        // Cancel & dispose previous pending request
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _vm.Debounce(async () =>
        {
            try
            {
                _vm.Suggestions = new List<Models.PhotoLocation>();
                _vm.ShowSuggestions = false;

                string input = e.NewTextValue;
                string escaped = Uri.EscapeDataString(input);
                string apiString = $@"https://nominatim.openstreetmap.org/search?q={escaped}&format=geocodejson&addressdetails=1&countrycodes=us";
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(apiString));
                var results = new GeocodeResult.Rootobject();
                try
                {
                    _client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "6.0"));
                    _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"application/json"));
                    results = await _client.GetFromJsonAsync<GeocodeResult.Rootobject>(apiString, cancellationToken: token);
                }
                catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.OperationAborted) { }
                if (results == null || token.IsCancellationRequested)
                    return;

                _vm.Suggestions.Clear();
                _vm.ShowSuggestions = false;
                if (results != null)
                {
                    for (int i = 0; i < results.features.Length; i++)
                    {
                        var result = results.features[i];
                        _vm.Suggestions.Add(new Models.PhotoLocation
                        {
                            Longitude = Double.Parse(result.geometry.coordinates[0].ToString()),
                            Latitude = Double.Parse(result.geometry.coordinates[1].ToString()),
                            City = result.properties.geocoding.name ?? result.properties.geocoding.city,
                            County = result.properties.geocoding.county,
                            State = result.properties.geocoding.state,
                            Country = result.properties.geocoding.country,
                            GeocodeDisplayName = result.properties.geocoding.label
                        });
                    }
                }

                _vm.ShowSuggestions = _vm.Suggestions.Count > 0;

            }
            catch (OperationCanceledException)
            {
                // cancelled -ignore
            }
            catch (Exception)
            {
                // cancelled - ignore
            }
        }, TimeSpan.FromMilliseconds(900));
    }

    private async void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Window == null) return;
        double Width = Window.Width;
        double Height = Window.Height;
        // double parentWidth = TitleBorder.Width;
        // double parentHeight = TitleBorder.Height;
        if (Width == 0) return;

        double scale = .6;
        double newFormulaSize = Math.Max(20, Math.Min((Width * scale), 36));
        Welcome.FontSize = newFormulaSize;
        Photographers.FontSize = newFormulaSize;
    }

    private async void OnGpsButtonPressed(object sender, TappedEventArgs e)
    {
        if (sender is Shape gpsButton)
        {
            await Task.WhenAll(
                gpsButton.ScaleYToAsync(0.85, 90, Easing.CubicOut),
                gpsButton.TranslateToAsync(0, 4, 90, Easing.CubicOut)
                );
            //optional: small delay too simulate a physical press
            //Release back to normal
            await Task.WhenAll(
                gpsButton.ScaleYToAsync(1.0, 120, Easing.CubicIn),
                gpsButton.TranslateToAsync(0, 0, 120, Easing.CubicIn));
            gpsButton.Focus();

        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (InputLocation.Width > 0)
            if (InputLocation.IsEnabled == false)
            {
                InputLocation.IsEnabled = true;
                InputLocation.TextChanged += InputLocation_TextChanged;
                InputLocation.Focus();
            }

    }
}