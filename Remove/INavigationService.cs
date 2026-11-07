using ThePhotographersForecast.Views;

namespace ThePhotographersForecast.Services
{
    public interface INavigationService
    {
        Task GoBackAsync();
        void InitializeAsync();
        Task NavigateToAsync(string route, IDictionary<string, object>? parameters);
        void NavigateToForecastAsync();
        void NavigateToSelectLocationAsync(SelectLocation selectLocation);
        void NavigateToSettingsAsync();
        Task PopAsync();
    }
}