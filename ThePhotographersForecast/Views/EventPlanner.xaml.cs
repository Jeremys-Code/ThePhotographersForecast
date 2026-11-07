using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class EventPlanner : ContentPage
{
    private EventPlannerViewModel _vm;
    //   private IBackgroundMapService _mapService
    private double Pane2GridHeight { get; set; }
    public ISession _session { get; set; }
    public EventPlanner(EventPlannerViewModel vm, ISession session)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        _session = session;



        //    _mapService = mapService;asy += (s, e) =>

        //       this.SizeChanged += ContentPage_SizeChanged;

        //  HighTempAnimation.Source = new SKFileLottieImageSource { File = "thermometer_warmer.json" };
        //   LowTempAnimation.Source = new SKFileLottieImageSource { File = "thermometer_colder.json" };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var _timer = new System.Timers.Timer(1500);
        _timer.Elapsed += NextFloat;
    }

    private void NextFloat(object? sender, ElapsedEventArgs e)
    {
        //   this.Lott LottieParadeView.

    }
    //private void ContentPage_SizeChanged(object? sender, EventArgs e)
    //{

    //    if (this.PaneContentGrid.Height == 0 || PaneContentGrid.Width == 0) return;
    //    double h = Window.Height - 30;
    //    EventForecastCard.HeightRequest = h;


    //}
    private async Task OnCardTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // The sender is the Border that has the gesture recognizer in the DataTemplate
            if (sender is VisualElement ve)
            {
                // Attempt to animate a subtle lift (scale + translate) to simulate elevation
                var card = ve as VisualElement;

                // If the element is a Border, we can try to tweak its Shadow opacity for extra effect
                Border? border = ve as Border;
                double originalShadowOpacity = -1;
                if (border?.Shadow != null)
                {
                    originalShadowOpacity = border.Shadow.Opacity;
                    try { border.Shadow.Opacity = Math.Min(1.0f, ((float)originalShadowOpacity <= 0f ? 0.6f : (float)originalShadowOpacity + 0.15f)); } catch { }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.03, 120, Easing.CubicOut),
                        card.TranslateToAsync(0, -6, 120, Easing.CubicOut)
                    );
                }

                var front = ve.FindByName<Grid>("FrontView");
                var back = ve.FindByName<Grid>("BackView");
                if (front != null && back != null)
                {
                    if (front.IsVisible)
                    {
                        await front.RotateYToAsync(180, 180, Easing.CubicIn);
                        front.IsVisible = false;
                        back.RotationY = -1800;
                        back.IsVisible = true;
                        await back.RotateYToAsync(0, 220, Easing.CubicOut);
                    }
                    else
                    {
                        await back.RotateYToAsync(-180, 180, Easing.CubicIn);
                        back.IsVisible = false;
                        front.RotationY = -180;
                        front.IsVisible = true;
                        await front.RotateYToAsync(0, 220, Easing.CubicOut);
                    }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.0, 120, Easing.CubicIn),
                        card.TranslateToAsync(0, 0, 120, Easing.CubicIn)
                    );
                }

                if (border?.Shadow != null && originalShadowOpacity >= 0)
                {
                    try { border.Shadow.Opacity = (float)originalShadowOpacity; } catch { }
                }
            }

            if (sender is BindableObject bo && bo.BindingContext is DayViewModel day)
            {
                _vm.SelectedDay = day;
            }
        }
        catch (Exception)
        {
            if (sender is BindableObject bo && bo.BindingContext is DayViewModel daily)
                _vm.SelectedDay = daily;
        }
        return;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            // The sender is the Border that has the gesture recognizer in the DataTemplate
            if (sender is VisualElement ve)
            {
                // Attempt to animate a subtle lift (scale + translate) to simulate elevation
                var card = ve as VisualElement;

                // If the element is a Border, we can try to tweak its Shadow opacity for extra effect
                Border? border = ve as Border;
                double originalShadowOpacity = -1;
                if (border?.Shadow != null)
                {
                    originalShadowOpacity = border.Shadow.Opacity;
                    try { border.Shadow.Opacity = Math.Min(1.0f, ((float)originalShadowOpacity <= 0f ? 0.6f : (float)originalShadowOpacity + 0.15f)); } catch { }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.03, 120, Easing.CubicOut),
                        card.TranslateToAsync(0, -6, 120, Easing.CubicOut)
                    );
                }

                var front = ve.FindByName<Grid>("FrontView");
                var back = ve.FindByName<Grid>("BackView");
                if (front != null && back != null)
                {
                    if (front.IsVisible)
                    {
                        await front.RotateYToAsync(180, 180, Easing.CubicIn);
                        front.IsVisible = false;
                        back.RotationY = -1800;
                        back.IsVisible = true;
                        await back.RotateYToAsync(0, 220, Easing.CubicOut);
                    }
                    else
                    {
                        await back.RotateYToAsync(-180, 180, Easing.CubicIn);
                        back.IsVisible = false;
                        front.RotationY = -180;
                        front.IsVisible = true;
                        await front.RotateYToAsync(0, 220, Easing.CubicOut);
                    }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.0, 120, Easing.CubicIn),
                        card.TranslateToAsync(0, 0, 120, Easing.CubicIn)
                    );
                }

                if (border?.Shadow != null && originalShadowOpacity >= 0)
                {
                    try { border.Shadow.Opacity = (float)originalShadowOpacity; } catch { }
                }
            }

            if (sender is BindableObject bo && bo.BindingContext is DayViewModel day)
            {
                _vm.SelectedDay = day;
            }
        }
        catch (Exception)
        {
            if (sender is BindableObject bo && bo.BindingContext is DayViewModel daily)
                _vm.SelectedDay = daily;
        }
        return;
    }

    //private async void LottieParadeView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    //{

    //    if (e.CurrentItem is SKLottieView sK)
    //    {
    //        sK.Parent.GetVisualElementWindow().
    //    }

    //}

    //private async Task OnCardTapped(object sender, TappedEventArgs e)
    //{

    //    if (sender is VisualElement ve)
    //    {

    //    }

    //}
    // Create the map from the service and assign it to the MapView declared in XAML
    //    var createdMap = _mapService.CreateMap();
    //    if (createdMap != null)
    //    { // copy MapElements (polylines/polygons)
    //        foreach (var elem in createdMap.MapElements)
    //            BackgroundMapView.MapElements.Add(elem);
    //        // copy Pins
    //        foreach (var pin in createdMap.Pins)
    //            BackgroundMapView.Pins.Add(pin);

    //        // If the created map already moved to a region (via MoveToRegion), try to use its VisibleRegion.
    //        if (createdMap.VisibleRegion != null)
    //        {
    //            BackgroundMapView.MoveToRegion(createdMap.VisibleRegion);
    //        }
    //        else if (BackgroundMapView.Pins.Count > 0)
    //        {
    //            // center on first pin with a small radius if no VisibleRegion
    //            var center = BackgroundMapView.Pins[0].Location;
    //            BackgroundMapView.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(center, Microsoft.Maui.Maps.Distance.FromKilometers(1.5)));
    //        }
    //        // Keep the map non-interactive so it acts as a fixed background
    //        BackgroundMapView.InputTransparent = true;
    //        // Ensure the map view remains enabled so tiles can render (do not set IsEnabled = false)
    //        BackgroundMapView.IsEnabled = true;
    //    }
    //}
}