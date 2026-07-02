using CommunityToolkit.Maui.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Services
{
    public class ImageCacheService
    {
        private readonly HttpClient _client;

        public ImageCacheService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("photographer") ?? new HttpClient();
        }
        public async Task<ImageSource> GetCachedFileAsync(string url)
        {
            var hash = url.GetHashCode().ToString();
            var cache = FileSystem.Current.CacheDirectory;
            var cacheResults = Directory.EnumerateFiles(cache, hash, SearchOption.TopDirectoryOnly);
            if (cacheResults.Any())
            {
                return cacheResults.First();
            }
            else
            {
                try
                {
                    //Local file path
                    var imagePath = Path.Combine(cache, hash);
                    // Build request with required headers for weather.gov
                    var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                    // Use a descriptive User-Agent per weather.gov policy
                    _client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
                    var response = await _client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        ImageSource imageSource = ImageSource.FromFile("unknown.png");
                        return imageSource;
                    }
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes == null || bytes.Length == 0)
                    {
                        ImageSource imageSource = ImageSource.FromFile("unknown.png");
                        return imageSource;
                    }
                    else
                    {
                        // Return an ImageSource created from an in-memory stream so it survives after disposal
                        var content = await response.Content.ReadAsByteArrayAsync();
                        ImageSource imgSource = ImageSource.FromStream(() => new MemoryStream(content));
                        return imgSource;
                    }
                }
                catch (Exception)
                {
                    return "unknown.png";
                }
            }

        }

    }
}
