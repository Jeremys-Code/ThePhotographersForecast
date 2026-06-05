using Microsoft.Maui.Controls;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ThePhotographersForecast.Services
{
    public class HttpClientConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Convert a single URL or a list of URLs into ImageSource(s)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is List<string> urls && urls.Any())
                {
                    var list = urls.Select(url => CreateImageSourceFromUrl(url)).Where(x => x != null).ToList();
                    return list;
                }

                if (value is string url && !string.IsNullOrWhiteSpace(url))
                {
                    return CreateImageSourceFromUrl(url);
                }
            }
            catch (Exception)
            {
                // Swallow and return null so binding doesn't throw
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private ImageSource CreateImageSourceFromUrl(string url)
        {
            try
            {
                // Build request with required headers for weather.gov
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Use a descriptive User-Agent per weather.gov policy
                request.Headers.UserAgent.ParseAdd("PhotographersDay/3.0 (+https://example.com)");

                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                    return null;

                var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                if (bytes == null || bytes.Length == 0)
                    return null;

                // Return an ImageSource created from an in-memory stream so it survives after disposal
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
