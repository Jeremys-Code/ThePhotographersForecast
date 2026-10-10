using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class ForecastDetails : ContentPage
{
    private IBackgroundMapService _backgroundMapService;
    private readonly ForecastDetailsViewModel _vm;
    //private NetTopologySuite.Geometries.Envelope? _initialEnvelope;
    //private bool _navigated = false;
    public ForecastDetails(ForecastDetailsViewModel vm, IBackgroundMapService backgroundMapService)
    {

        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        //_backgroundMapService = backgroundMapService;

        //// Create the map from the service and assign it to the MapView declared in XAML
        //var createdMap = _backgroundMapService.CreateMap();
        //if (createdMap != null)
        //{ // copy MapElements (polylines/polygons)
        //    foreach (var elem in createdMap.MapElements)
        //        BackgroundMapView.MapElements.Add(elem);
        //    // copy Pins
        //    foreach (var pin in createdMap.Pins)
        //        BackgroundMapView.Pins.Add(pin);

        //    // If the created map already moved to a region (via MoveToRegion), try to use its VisibleRegion.
        //    if (createdMap.VisibleRegion != null)
        //    {
        //        BackgroundMapView.MoveToRegion(createdMap.VisibleRegion);
        //    }
        //    else if (BackgroundMapView.Pins.Count > 0)
        //    {
        //        // center on first pin with a small radius if no VisibleRegion
        //        var center = BackgroundMapView.Pins[0].Location;
        //        BackgroundMapView.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(center, Microsoft.Maui.Maps.Distance.FromKilometers(1.5)));
        //    }
        //    // Keep the map non-interactive so it acts as a fixed background
        //    BackgroundMapView.InputTransparent = true;
        //    // Ensure the map view remains enabled so tiles can render (do not set IsEnabled = false)
        //    BackgroundMapView.IsEnabled = true;
        //    }
        //// If session polygon coordinates exist, compute a Spherical Mercator envelope and navigate the map's navigator to it
        //var ring = _vm.SharedSession?.Coordinates;
        //if (ring != null)
        //{
        //    var mercCoords = ring.Coordinates.Select(c => Mapsui.Projections.SphericalMercator.FromLonLat(c.X, c.Y)).ToArray();
        //    var minX = mercCoords.Min(p => p.x);
        //    var maxX = mercCoords.Max(p => p.x);
        //    var minY = mercCoords.Min(p => p.y);
        //    var maxY = mercCoords.Max(p => p.y);
        //    var padX = (maxX - minX) * 0.1;
        //    var padY = (maxY - minY) * 0.1;

        //    // Store envelope to navigate after the view has laid out
        //    _initialEnvelope = new NetTopologySuite.Geometries.Envelope(minX - padX, maxX + padX, minY - padY, maxY + padY);
        //}

        // The MapView is declared in XAML (BackgroundMapView); no need to replace Content
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //if (BackgroundMapView == null)
        //    return;

        //// If the MapView already has a size, navigate immediately. Otherwise wait for SizeChanged.
        //if (BackgroundMapView.Width > 0 && BackgroundMapView.Height > 0)
        //{
        //    TryNavigateToInitialEnvelope();
        //}
        //else
        //{
        //    BackgroundMapView.SizeChanged += BackgroundMapView_SizeChanged;
        //}
    }

    //private void BackgroundMapView_SizeChanged(object? sender, EventArgs e)
    //{
    //    if (BackgroundMapView.Width <= 0 || BackgroundMapView.Height <= 0)
    //        return;

    //    BackgroundMapView.SizeChanged -= BackgroundMapView_SizeChanged;
    //    TryNavigateToInitialEnvelope();
    //}

    //private void TryNavigateToInitialEnvelope()
    //{
    //    if (BackgroundMapView == null)
    //        return;

    //    try
    //    {
    //        var createdMap = _backgroundMapService.CreateMap();
    //        if (createdMap != null)
    //        { // copy MapElements (polylines/polygons)
    //            foreach (var elem in createdMap.MapElements)
    //                BackgroundMapView.MapElements.Add(elem);
    //            // copy Pins
    //            foreach (var pin in createdMap.Pins)
    //                BackgroundMapView.Pins.Add(pin);

    //            if (createdMap.VisibleRegion != null)
    //            {
    //                BackgroundMapView.MoveToRegion(createdMap.VisibleRegion);
    //            }
    //            else if (BackgroundMapView.Pins.Count > 0)
    //            {
    //                // center on first pin with a small radius if no VisibleRegion
    //                var center = BackgroundMapView.Pins[0].Location;
    //                BackgroundMapView.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(center, Microsoft.Maui.Maps.Distance.FromKilometers(1.5)));
    //            }
    //        }
    //    }
    //    catch
    //    {
    //        // best-effort: do not throw if API differs at runtime
    //    }
    //}
}