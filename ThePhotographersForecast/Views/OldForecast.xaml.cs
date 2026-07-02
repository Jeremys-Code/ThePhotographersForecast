using System.ComponentModel;
using ThePhotographersForecast.ViewModels;


namespace ThePhotographersForecast.Views;

public partial class Forecast : ContentPage
{
    //   private ForecastViewModel Vm => BindingContext as ForecastViewModel;

    public Forecast(ForecastViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        //if (Vm != null)
        //{
        //    Vm.PropertyChanged += Vm_PropertyChanged;
        //}
    }

    //private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(ForecastViewModel.SelectedDaily))
    //    {
    //        MainThread.BeginInvokeOnMainThread(() =>
    //        {
    //            var selected = Vm?.SelectedDaily;
    //            if (selected != null)
    //            {
    //                // Scroll the CollectionView so the selected day is visible and centered
    //                EventCarousel.ScrollTo(selected, position: ScrollToPosition.Center, animate: true);
    //            }
    //        });
    //    }
    //}

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    if (Vm != null)
    //        Vm.PropertyChanged -= Vm_PropertyChanged;
    //}

    private void ContentPage_SizeChanged(object sender, EventArgs e)
    {
        InvalidateMeasure();


    }
}
//private void InitMap()
//{
//    var map = new Mapsui.Map();

//    // 1. Add OSM tiles
//    map.Layers.Add(OsmTileFactory.CreateOsmLayer());

//    // 2. Add polygon overlay
//    var coords = new[]
//    {
//    (-71.011, 42.2989),
//    (-71.0058, 42.3203),
//    (-71.0348, 42.3241),
//    (-71.0399, 42.3027),
//    (-71.011, 42.2989)
//};

//    map.Layers.Add(PolygonLayerFactory.CreatePolygonLayer(coords));

//    // 3. Compute 2.5 km bounding box
//    double centerLat = 42.31;
//    double centerLon = -71.02;

//    var (minLat, minLon, maxLat, maxLon) = PolygonLayerFactory.GetBoundingBox(centerLat, centerLon, 2500);

//    var min = SphericalMercator.FromLonLat(minLon, minLat);
//    var max = SphericalMercator.FromLonLat(maxLon, maxLat);

//    var envelope = new MRect(min.x, min.y, max.x, max.y);

//    // 4. Assign map to MapView
//    //

//    // 5. Navigate to bounding box (replacement for map.Home)
//    map.Navigator.ZoomIn(5, Mapsui.Animations.Easing.CubicIn);
//}


//protected override void OnAppearing()
//{
//    base.OnAppearing();
//    var deviceOrientation = new Services.DeviceOrientationService();
//    if (deviceOrientation.GetOrientation() != Services.DeviceOrientationService.DeviceOrientation.Undefined)
//    {
//        // On Android, we want to force the layout to update when the page appears
//        // to ensure it starts with the correct layout based on the current orientation and fold state.
//        UpdateLayoutForOrientation(DeviceDisplay.MainDisplayInfo);
//    }
//}
//protected override void OnDisappearing()
//{
//    base.OnDisappearing();
//    var deviceOrientation = new Services.DeviceOrientationService();
//    if (deviceOrientation.GetOrientation() != Services.DeviceOrientationService.DeviceOrientation.Undefined)
//    {
//        DeviceDisplay.MainDisplayInfoChanged -= OnDisplayInfoChanged;
//    }
//}

//private void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
//{

//    UpdateLayoutForOrientation(e.DisplayInfo);
//}

//private void UpdateLayoutForOrientation(DisplayInfo info)
//{
//    try
//    {
//        double width = info.Width / info.Density;
//        double height = info.Height / info.Density;
//        bool isLandscape = width > height;
//        var innerView = Shell.Current.CurrentPage.FindByName("EventForecastCollection") as CollectionView;

//        if (isLandscape)
//        {
//            // Header on left taking 1/4 width, forecast on right taking 3/4
//            MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
//            MainGrid.ColumnDefinitions[1].Width = new GridLength(3, GridUnitType.Star);

//            // Move header to left column spanning both rows
//            Grid.SetRow(ForecastPageHeader, 0);
//            Grid.SetColumn(ForecastPageHeader, 0);
//            Grid.SetRowSpan(ForecastPageHeader, 3);
//            Grid.SetColumnSpan(ForecastPageHeader, 1);

//            // Place forecast border in right column and full height
//            Grid.SetRow(ForecastBorder, 0);
//            Grid.SetColumn(ForecastBorder, 1);
//            Grid.SetRowSpan(ForecastBorder, 3);
//            Grid.SetColumnSpan(ForecastBorder, 1);

//            // Switch collectionview to vertical layout
//            var gridLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical);
//            CarouselItems.ItemsLayout = gridLayout;
//            CarouselItems.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
//            CarouselItems.VerticalScrollBarVisibility = ScrollBarVisibility.Always;

//            // Make inner collection of events scroll vertically

//            innerView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
//            innerView.ItemsLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical);

//        }
//        else
//        {
//            // Portrait: header on top, forecast below with full width
//            MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
//            MainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

//            Grid.SetRow(ForecastPageHeader, 0);
//            Grid.SetColumn(ForecastPageHeader, 0);
//            Grid.SetRowSpan(ForecastPageHeader, 1);
//            Grid.SetColumnSpan(ForecastPageHeader, 2);

//            Grid.SetRow(ForecastBorder, 1);
//            Grid.SetColumn(ForecastBorder, 0);
//            Grid.SetRowSpan(ForecastBorder, 1);
//            Grid.SetColumnSpan(ForecastBorder, 2);

//            // Switch collectionview to horizontal carousel
//            var gridLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Horizontal);
//            CarouselItems.ItemsLayout = gridLayout;
//            CarouselItems.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
//            CarouselItems.VerticalScrollBarVisibility = ScrollBarVisibility.Never;

//            innerView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
//            innerView.ItemsLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical);
//        }