using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ApiClients
{
    public class GetApi : ObservableObject, IGetApi
    {
        private string appDataDirectory = FileSystem.Current.AppDataDirectory;
        private IHttpClientFactory _client = null;
        private IServiceProvider _service;
        [ObservableProperty]
        public partial Session SharedSession { get; set }
        public void Initialize(IServiceProvider service, Session sharedSession, IHttpClientFactory client)
        {
            _service = service;
            SharedSession = sharedsession;
            _client = client;
        }

        public async Task<T> RetrieveApiDataAsync<T>(HttpRequestMessage request)
        {

            var uri = request.RequestUri ?? default;

            string fileName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = uri.Host.Replace('.', '_');
            }

            fileName += $"_{SharedSession.StartDate.ToString("yyyy_MM_dd")}_{SharedSession.EndDate.ToString("yyyy-MM-dd")}.json";

            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            string filePath = Path.Combine(appDataDirectory, fileName);

            try
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                if (File.Exists(filePath))
                {
                    jsonOptions.GetConverter(typeof(NullNumberConverterFactory));
                    var cached = await File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<T>(cached, jsonOptions);
                }

                if (_client.CreateClient() is HttpClient client)
                {
                    client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue(@"ThePhotographersForecast/6.0"));
                    client.DefaultRequestHeaders.Accept.Clear();
                    if (client.DefaultRequestHeaders.Host.Contains("api.weather.gov", StringComparison.OrdinalIgnoreCase))
                    {
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"application/geo+json"));
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(@"application/json"));
                    }

                    var response = await client.SendAsync(request);

                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();

                    await File.WriteAllTextAsync(filePath, json);
                    return JsonSerializer.Deserialize<T>(json, jsonOptions);
                }
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlertAsync("Data file error", "An error occurred while downloading data files. Please try again.", "OK");

            }

            return default;
        }

        private async Task<List<T>> GetCachedJsonFiles<T>(string fileName)
        {
            var dataFiles = Directory.GetFiles(appDataDirectory, fileName).Order().ToList();
            List<T> jsonList = new List<T>();
            foreach (var file in dataFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var jsonObject = JsonSerializer.Deserialize<T>(content);
                if (jsonObject != null) jsonList.Add(jsonObject);
            }
            return jsonList;
        }
    }
}