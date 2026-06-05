using System.Net.Http.Json;

namespace ThePhotographersForecast.Services
{
    public static class JsonDataService
    {
        private static readonly HttpClient _http = new HttpClient();
        private static CancellationToken ct = new CancellationToken();
        public static async Task<T?> GetJsonDataAsync<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("The_Photographers_Forecast", "1.0"));
            await Task.Delay(800);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return default;

            var content = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            return content;
        }
    }
}
//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//    request.Method = "GET";
//    request.UserAgent = "PfApp, jeremy@bostonfromabove.com";
//    request.Accept = "application/geo+json";


//    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
//    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
//    {
//        string data = reader.ReadToEnd();
//        var item = JsonConvert.DeserializeObject<T>(data);
//        if (item != null)
//            return item;
//    }
//    return null;
//}
