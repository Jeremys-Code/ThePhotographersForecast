using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface ILottieParadeService
    {
        Task<LottieAnimation[]> BuildLottieParadeAsync();
    }
}