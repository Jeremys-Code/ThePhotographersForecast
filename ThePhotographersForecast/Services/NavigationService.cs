using ThePhotographersForecast.Views;

namespace ThePhotographersForecast.Services;

public static class NavigationService
{
    private static IServiceProvider _services;

    private static IServiceProvider Services => _services ?? throw new InvalidOperationException("NavigationService not initialized. Call Initialize(IServiceProvider) first.");

    public static void Initialize(IServiceProvider services)
    {
        _services = services;
    }

    public static async Task NavigateToSelectLocationAsync()
    {
        var vm = _services.GetRequiredService<ViewModels.SelectLocationViewModel>();
        var page = new SelectLocation(vm);
        page.BindingContext = vm;
        await Shell.Current.Navigation.PushAsync(page);
        return;
    }
    public static async Task NavigateToForecastAsync()
    {
        var vm = _services.GetRequiredService<ViewModels.ForecastViewModel>();
        var page = new Forecast(vm);
        page.BindingContext = vm;
        await Shell.Current.Navigation.PushAsync(page);
        return;
    }
    public static async Task NavigateToPhotoTimeTrackAsync()
    {
        var vm = _services.GetRequiredService<ViewModels.PhotoTimeTrackViewModel>();
        var page = new PhotoTimeTrack(vm);
        page.BindingContext = vm;
        await Shell.Current.Navigation.PushAsync(page);
        return;
    }

    public static Task PopAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
    public static Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}