using ThePhotographersForecast.Views;

namespace ThePhotographersForecast.Services;

public static class NavigationService
{
    private static IServiceProvider _service;

    public static void Initialize(IServiceProvider service)
    {
        _service = service;
    }

    public static async Task InitializeAsync()
    {
        var firstPage = (Preferences.Default.ContainsKey("FirstRun") ? "SelectLocation" : "Settings");
        await Shell.Current.GoToAsync(firstPage);
        return;
    }
    public static async Task NavigateToSelectLocationAsync()
    {
        await Shell.Current.Navigation.PushAsync(ServiceHelper.GetService<SelectLocation>(), true);
        return;

    }
    public static async Task NavigateToForecastAsync()
    {

        await Shell.Current.Navigation.PushAsync(ServiceHelper.GetService<Forecast>(), true);
        return;

    }
    public static async Task NavigateToSettingsAsync()
    {
        await Shell.Current.Navigation.PushAsync(ServiceHelper.GetService<Settings>(), true);
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

    public static async Task NavigateToAsync(Page route, IDictionary<string, object>? parameters)
    {
        if (parameters != null)
            await Shell.Current.Navigation.PushAsync(route, true);
        else
            await Shell.Current.Navigation.PushAsync(route);
    }


}
