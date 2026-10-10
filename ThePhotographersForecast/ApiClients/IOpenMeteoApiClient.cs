namespace ThePhotographersForecast.ApiClients
{
    public interface IOpenMeteoApiClient
    {
        Task<string> GetOpenMeteoDataAsync<T>(HttpRequestMessage request);
    }
}