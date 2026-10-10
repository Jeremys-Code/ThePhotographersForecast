namespace NationWeatherService
{
    public interface INwsService
    {
        Task<(string, string, string)> GetNwsDataAsync(double latitude, double longitude);
    }
}