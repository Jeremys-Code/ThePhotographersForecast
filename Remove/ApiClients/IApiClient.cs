namespace ThePhotographersForecast.ApiClients
{
    public interface IApiClient
    {
        Task<T?> GetDataAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
