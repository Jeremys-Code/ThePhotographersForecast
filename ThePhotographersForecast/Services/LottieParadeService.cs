using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;


namespace ThePhotographersForecast.Services
{
    public partial class LottieParadeService : ILottieParadeService
    {
        private ISession _session;
        public LottieParadeService(ISession session)
        {

            _session = session;
        }

        //Service Public entry point
        public async Task<LottieAnimation[]> BuildLottieParadeAsync()
        {
            var lottieParade = new LottieAnimation[_session.NwsData.HourlyForecasts.Length];

            for (int i = 0; i < _session.NwsData.HourlyForecasts.Length; i++)
            {
                var h = _session.NwsData.HourlyForecasts[i];
                var lFloat = new LottieAnimation
                {
                    LottieWeather = h.SkHourlyAnimation,
                    Time = h.StartTime,
                    Forecast = h.ShortForecast,
                    Temperature = h.TempAndUnit,
                    SkyTransition = h.TimeOfDayBrush
                };
                lottieParade[i] = lFloat;
            }

            return lottieParade;
        }
    }
}
