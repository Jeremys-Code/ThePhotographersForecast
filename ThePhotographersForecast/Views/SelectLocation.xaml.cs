using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class SelectLocation : ContentPage
{
    private readonly SelectLocationViewModel _vm;
    public SelectLocation(SelectLocationViewModel vm)
    {
        InitializeComponent();
        _vm = vm;

        BindingContext = _vm;
        InputLocation.TextChanged += InputLocation_TextChanged;
        if (_vm.IsWorking == true)
        {
            SelectedLocation();
        }
    }

    private async void InputLocation_TextChanged(object sender, TextChangedEventArgs e)
    {

        string input = e.NewTextValue;
        _vm.Suggestions.Clear();


        if (input.Length < 3 || string.IsNullOrEmpty(input))
        {

            return;
        }

        if (input.Length >= 3)
        {

            string apiString = String.Concat($@"https://geocoding-api.open-meteo.com/v1/search?name=", input, @"&count=10&language=en&format=json&countryCode=US");
            var response = await Services.JsonDataService.GetJsonDataAsync<GeocodeResult.Rootobject>(apiString);

            _vm.Suggestions.Clear();
            _vm.ShowSuggestions = false;
            if (response != null)
            {
                for (int i = 0; i < response.results.Length; i++)
                {
                    var result = response.results[i];
                    _vm.Suggestions.Add(new Models.PhotoLocation
                    {
                        Latitude = result.latitude,
                        Longitude = result.longitude,
                        City = !string.IsNullOrEmpty(result.admin3?.Trim()) ? result.admin3 : result.name,
                        State = result.admin1,
                        Country = result.country,
                        Elevation = result.elevation.HasValue ? (int)Math.Round(result.elevation.Value, 0) : 0,
                        TimeZone = result.timezone,
                        Population = result.population.HasValue ? (int)result.population.Value : 0
                    });
                }
                _vm.ShowSuggestions = true;
                return;
            }
        }
    }
    private void SelectedLocation()
    {
        InputLocation.Text = ServiceHelper.GetService<PhotoLocation>().DisplayName;
    }
}