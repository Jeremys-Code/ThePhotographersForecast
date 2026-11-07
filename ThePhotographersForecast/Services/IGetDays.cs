using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetDays
    {
        Task<ObservableCollection<DayViewModel>> GetDaysAsync();
    }
}
