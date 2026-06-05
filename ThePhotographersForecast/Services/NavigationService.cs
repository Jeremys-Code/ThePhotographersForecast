using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;
using ThePhotographersForecast.Views;

namespace ThePhotographersForecast.Services;

public static class NavigationService
{
    private static IServiceProvider _services;

    public static void Initialize(IServiceProvider services)
    {
        _services = services;
    }

    public static async Task NavigateToSelectLocationAsync()
    {


        var vm = _services.GetRequiredService<ViewModels.SelectLocationViewModel>();
        var page = new SelectLocation(vm);
        page.BindingContext = vm;

        await NavigationHelper.SafePushAsync(page, 800);

    }
    public static async Task NavigateToForecastAsync()
    {

        var vm = _services.GetRequiredService<ViewModels.ForecastViewModel>();

        var page = new Forecast(vm);
        page.BindingContext = vm;
        await NavigationHelper.SafePushAsync(page, 800);
    }
    public static async Task NavigateToPhotoTimeTrackAsync()
    {

        var vm = _services.GetRequiredService<ViewModels.PhotoTimeTrackViewModel>();

        var page = new PhotoTimeTrack(vm);
        page.BindingContext = vm;
        await NavigationHelper.SafePushAsync(page, 800);
    }
    public static async Task NavigateToCalendarAsync()
    {

        var vm = _services.GetRequiredService<ViewModels.CalendarViewModel>();

        var page = new Calendar(vm);
        page.BindingContext = vm;

        await NavigationHelper.SafePushAsync(page, 800);

    }

    //public static async Task NavigateToSettingsPageAsync()
    //{
    //    var instance = new PhotographersDay.Views.SettingsPage();
    //    var page = _services.GetRequiredService<PhotographersDay.Views.SettingsPage>();
    //    await Shell.Current.CurrentPage.Navigation.PushAsync(instance);
    //}
    public static Task PopAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
    public static Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}
