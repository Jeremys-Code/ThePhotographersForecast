using ThePhotographersForecast.Views;

namespace ThePhotographersForecast.Services;

public static class NavigationService
{
    //    private static IServiceProvider? _service;


    private static IServiceProvider Service;// => _service ?? throw new InvalidOperationException("NavigationService not initialized. Call Initialize(IServiceProvider) first.");

    public static void Initialize(IServiceProvider services)
    {
        Service = services;
    }

    public static async Task NavigateToLaunchPage()
    {
        if (Preferences.Default.ContainsKey("FirstRun"))
        {
            var results = Preferences.Default.Get("FirstRun", true);
            if (results == true)
            {
                Preferences.Default.Set("FirstRun", false);
                await NavigationService.NavigateToSettingsAsync();
            }
            else
            {
                await NavigationService.NavigateToSelectLocationAsync();
            }
        }
        else
        {
            await NavigationService.NavigateToSettingsAsync();
            Preferences.Default.Set("FirstRun", false);
        }
    }

    public static async Task NavigateToSelectLocationAsync()
    {
        var vm = Service.GetService<ViewModels.SelectLocationViewModel>();
        var page = new SelectLocation(vm);
        await Shell.Current.Navigation.PushAsync(page);
        return;
    }
    public static async Task NavigateToForecastAsync()
    {
        var vm = Service.GetService<ViewModels.ForecastViewModel>();
        var page = new Forecast(vm);
        await Shell.Current.Navigation.PushAsync(page);
        return;
    }
    public static async Task NavigateToSettingsAsync()
    {
        var vm = Service.GetService<ViewModels.SettingsViewModel>();
        var page = new Settings(vm);
        await Shell.Current.Navigation.PushAsync(page);
        return;
    }
    public static async Task PopAsync()
    {
        await Shell.Current.GoToAsync("..");
        return;
    }
    public static async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
        return;
    }

    public static async Task NavigateToAsync(string route, IDictionary<string, object>? parameters)
    {
        if (parameters != null)
            await Shell.Current.GoToAsync(route, parameters);
        else
            await Shell.Current.GoToAsync(route);
    }


}
