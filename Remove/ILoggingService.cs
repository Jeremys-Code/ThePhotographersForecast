namespace ThePhotographersForecast.Services
{
    public interface ILoggingService
    {
        Task<T?> ExecuteJsonOperation<T>(string operationName, Func<Task<T?>> func);
    }
}