namespace ThePhotographersForecast.Services
{
    public interface INavigationService
    {
        static abstract Task NavigateToAsync(string route, IDictionary<string, object>? parameters);
        Task GoBackAsync();
        void Initialize(IServiceProvider services);
        Task NavigateToForecastAsync();
        Task NavigateToSelectLocationAsync();
        Task NavigateToSettingsAsync();
        Task PopAsync();
    }
}