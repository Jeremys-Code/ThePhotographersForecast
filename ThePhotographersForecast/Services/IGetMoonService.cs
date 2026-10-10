using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetMoonService
    {
        Task<FutureMoonPhase[]> GetFutureMoonPhasesAsync(DateTime date, int utcOffset);
    }
}