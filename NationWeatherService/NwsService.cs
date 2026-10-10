namespace NationWeatherService
{
    public class NwsService : INwsService
    {
        private readonly HttpClient _client;

        public NwsService(HttpClient client)
        {
            _client = client;
        }
        //    public NwsService(HttpClient client)
        //  {
        //    var handler = new System.Net.Http.SocketsHttpHandler
        //    {
        //        ConnectCallback = async (context, cancellationToken) =>
        //        {
        //            var host = context.DnsEndPoint.Host;
        //            var port = context.DnsEndPoint.Port;
        //            var addresses = await System.Net.Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
        //            System.Net.IPAddress ipv4 = null;
        //            for (int i = 0; i < addresses.Length; i++)
        //            {
        //                if (addresses[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //                {
        //                    ipv4 = addresses[i];
        //                    break;
        //                }
        //            }
        //            if (ipv4 == null)
        //            {
        //                ipv4 = addresses.Length > 0 ? addresses[0] : System.Net.IPAddress.Loopback;
        //            }
        //            var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        //            await socket.ConnectAsync(new System.Net.IPEndPoint(ipv4, port)).ConfigureAwait(false);
        //            return new NetworkStream(socket, ownsSocket: true);
        //        }
        //    };
        //    _client = new System.Net.Http.HttpClient(handler);
        //    _client.BaseAddress = new Uri("https://api.weather.gov/");
        //    _client.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0 (airmail@bostonfromabove.com)");
        //    _client.DefaultRequestHeaders.Add("Accept", "application/geo+json");
        //}
        private async Task GetDataAsync<T>(HttpRequestMessage request)
        {
            var content = await _client.GetStringAsync("https://api.weather.gov/points/42.32,-71.059");
            if (content is string result)
            {
                File.WriteAllText("GetContent.txt", result);
            }
            return;
            //var response = await _client.SendAsync(request);
            //if (response.IsSuccessStatusCode)
            //{
            //    var json = await response.Content.ReadAsStringAsync();
            //    return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            //}
            //else
            //{
            //    throw new Exception($"Request failed with status code: {response.StatusCode}");
            //}
        }

        public async Task<(string, string, string)> GetNwsDataAsync(double latitude, double longitude)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://api.weather.gov/points/{latitude},{longitude}");

            await GetDataAsync<NwsUrlData.Rootobject>(request);
            //if (nwsUrlData != null)
            //{
            //    var forecastUrl = nwsUrlData.properties.forecast;
            //    var forecastHourlyUrl = nwsUrlData.properties.forecastHourly;
            //    var forecastGridDataUrl = nwsUrlData.properties.forecastGridData;
            //    return (forecastUrl, forecastHourlyUrl, forecastGridDataUrl);
            //    // Now you can use these URLs to make further requests to get the actual forecast data.
            //}
            return ("S", "O", "L");
        }
    }
}
