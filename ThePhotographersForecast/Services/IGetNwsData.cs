using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public interface IGetNwsData
    {
        public string DailyUrl { get; set; }
        public string HourlyUrl { get; set; }
        public string DetailUrl { get; set; }


        Task<NwsWeatherModel> GetWeatherAsync(double latitude, double longitude);

    }
}