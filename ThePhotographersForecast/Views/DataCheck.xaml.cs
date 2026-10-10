using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class DataCheck : ContentPage
{
    private DataCheckViewModel _vm;
    private IBackgroundMapService _mapService;
    public DataCheck(DataCheckViewModel vm, IBackgroundMapService mapService)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        _mapService = mapService;

        // Create the map from the service and assign it to the MapView declared in XAML
        var createdMap = _mapService.CreateMap();
        if (createdMap != null)
        { // copy MapElements (polylines/polygons)
            foreach (var elem in createdMap.MapElements)
                BackgroundMapView.MapElements.Add(elem);
            // copy Pins
            foreach (var pin in createdMap.Pins)
                BackgroundMapView.Pins.Add(pin);

            // If the created map already moved to a region (via MoveToRegion), try to use its VisibleRegion.
            if (createdMap.VisibleRegion != null)
            {
                BackgroundMapView.MoveToRegion(createdMap.VisibleRegion);
            }
            else if (BackgroundMapView.Pins.Count > 0)
            {
                // center on first pin with a small radius if no VisibleRegion
                var center = BackgroundMapView.Pins[0].Location;
                BackgroundMapView.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(center, Microsoft.Maui.Maps.Distance.FromKilometers(1.5)));
            }
            // Keep the map non-interactive so it acts as a fixed background
            BackgroundMapView.InputTransparent = true;
            // Ensure the map view remains enabled so tiles can render (do not set IsEnabled = false)
            BackgroundMapView.IsEnabled = true;
        }

    }
}
