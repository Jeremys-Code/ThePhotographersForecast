using Microsoft.Maui.Controls.Shapes;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class SelectLocation : ContentPage
{

    private SelectLocationViewModel _vm;
    private bool _isFlipping = false;
    const double a = 300; // ellipse radius X
    const double b = 200;  // ellipse radius Y
    const int duration = 3000; // ms

    public SelectLocation(SelectLocationViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;

        // Guarded subscription: ensure the named control exists in XAML
        if (InputLocation.IsLoaded)
        {
            _vm.IsSearchReadOnly = true;
        }

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
            _vm.IsSearchReadOnly = true;
            await Task.WhenAll(
                gpsButton.ScaleYToAsync(0.85, 180, Easing.CubicOut),
                gpsButton.TranslateToAsync(0, 4, 180, Easing.CubicOut)
                );
            //optional: small delay too simulate a physical press
            //Release back to normal
            await Task.WhenAll(
                gpsButton.ScaleYToAsync(1.0, 200, Easing.CubicIn),
                gpsButton.TranslateToAsync(0, 0, 200, Easing.CubicIn));
            gpsButton.Focus();

        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _vm.EnableLocationSearchCommand.Execute(null);
        if (InputLocation.Width > 0 && !_vm.IsSearchReadOnly)
        {
            InputLocation.Focus();
        }
    }
    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (!Daily.IsVisible || !Daily.IsEnabled) return;
        if (_isFlipping) return; // prevent overlapping flips
        _isFlipping = true;
        try
        {
            // The sender is the Border that has the gesture recognizer in the DataTemplate
            if (sender is VisualElement ve)
            {
                // Attempt to animate a subtle lift (scale + translate) to simulate elevation
                var card = ve as VisualElement;
                var border = ve.FindByName<Border>("CardBorder") as Border;

                double originalShadowOpacity = -1;
                if (border?.Shadow != null)
                {
                    originalShadowOpacity = border.Shadow.Opacity;
                    try { border.Shadow.Opacity = Math.Min(1.0f, ((float)originalShadowOpacity <= 0f ? 0.6f : (float)originalShadowOpacity + 0.15f)); } catch { }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.03, 300, Easing.CubicOut),
                        card.TranslateToAsync(0, -6, 300, Easing.CubicOut)
                    );
                }

                var front = ve.FindByName<Grid>("FrontView");
                var back = ve.FindByName<Grid>("BackView");
                if (front != null && back != null)
                {
                    // local helper to normalize rotation to 0..360
                    static double NormalizeRotation(double r) => ((r % 360) + 360) % 360;

                    // normalize stored rotation values so animations behave predictably
                    front.RotationY = NormalizeRotation(front.RotationY);
                    back.RotationY = NormalizeRotation(back.RotationY);

                    if (front.IsVisible)
                    {
                        // animate front to halfway then swap to back
                        await front.RotateYToAsync(90, 300, Easing.CubicIn);

                        // Swap visibility and animate back into view
                        front.IsVisible = false;
                        back.RotationY = -90; // start slightly turned away
                        back.IsVisible = true;
                        await back.RotateYToAsync(0, 300, Easing.CubicOut);
                        back.RotationY = 0;

                        // Ensure VisualState reflects the flipped card (this will keep XAML setters in sync)
                        VisualStateManager.GoToState(ve, "Flipped");
                    }
                    else
                    {
                        // animate back to halfway then swap to front
                        await back.RotateYToAsync(-90, 300, Easing.CubicIn);
                        back.IsVisible = false;
                        front.RotationY = 90;
                        front.IsVisible = true;
                        await front.RotateYToAsync(0, 300, Easing.CubicOut);
                        front.RotationY = 0;

                        VisualStateManager.GoToState(ve, "Normal");
                    }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.0, 300, Easing.CubicIn),
                        card.TranslateToAsync(0, 0, 300, Easing.CubicIn)
                    );
                }

                if (border?.Shadow != null && originalShadowOpacity >= 0)
                {
                    try { border.Shadow.Opacity = (float)originalShadowOpacity; } catch { }
                }
            }

            //   if (sender is BindableObject bo && bo.BindingContext is DayViewModel daily)
            if (sender is BindableObject bo && bo.BindingContext is NwsTimeOfDayForecast daily)
            {
                if (bo == Daily)
                {
                    Daily.SelectedItem = daily;
                }

            }
        }
        catch (Exception)
        {
            //   if (sender is BindableObject bo && bo.BindingContext is DayViewModel daily)
            if (sender is BindableObject bo && bo.BindingContext is NwsTimeOfDayForecast daily)
                if (bo == Daily)
                {
                    Daily.SelectedItem = daily;
                }
        }
        finally
        {
            _isFlipping = false;
        }
    }

    private async Task GoToDetailsTapped(object sender, TappedEventArgs e)
    {
        var index = _vm.Days.IndexOf(_vm.SelectedDay);
        await Shell.Current.GoToAsync(nameof(ForecastDetails));
    }
}