using System.Globalization;

namespace ThePhotographersForecast.Services
{
    public class HttpClientConverter : IValueConverter
    {
        private static readonly HttpClient _client = new HttpClient();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is UriImageSource url)
            {
                return ImageSource.FromStream(async (ct) =>
                {
                    // Add headers required by weather.gov
                    var request = new HttpRequestMessage(HttpMethod.Get, url.Uri);
                    request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
                    var response = await _client.SendAsync(request, ct);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStreamAsync(ct);
                });
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
