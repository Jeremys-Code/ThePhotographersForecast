using PhotographersDay.Models;

namespace PhotographersDay.Services
{
    public interface INavigationService
    {
        Task NavigateToSelectLocationAsync();
        Task NavigateToForecastAsync();
    }
}
