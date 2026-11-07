using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ApiClients
{
    public interface IGetApi
    {
        void Initialize(IServiceProvider service, ISession session);
        Task<T> RetrieveApiDataAsync<T>(HttpRequestMessage request);
    }
}