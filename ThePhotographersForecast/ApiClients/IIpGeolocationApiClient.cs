namespace ThePhotographersForecast.ApiClients
{
    public interface IIpGeolocationApiClient
    {
        Task<string> GetIpGeolocationDataAsync<T>(HttpRequestMessage request);
    }
}