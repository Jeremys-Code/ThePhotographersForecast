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
        public async Task<string> GetCachedFileAsync(string url)
        {
            var hash = url.GetHashCode().ToString();
            var cache = FileSystem.CacheDirectory;
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

                    var response = _client.SendAsync(request).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                        return "unknown.png";

                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes == null || bytes.Length == 0)
                    {
                        return "unknown.png";
                    }
                    else
                    {
                        // Return an ImageSource created from an in-memory stream so it survives after disposal
                        var content = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(imagePath, content);
                        return imagePath;
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
