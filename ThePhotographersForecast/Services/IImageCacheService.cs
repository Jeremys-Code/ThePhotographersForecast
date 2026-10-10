namespace ThePhotographersForecast.Services
{
    public interface IImageCacheService
    {
        Task<ImageSource> GetCachedFileAsync(string url);
    }
}