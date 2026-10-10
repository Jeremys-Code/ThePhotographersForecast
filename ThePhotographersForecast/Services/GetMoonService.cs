using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.JsonConverters;

namespace ThePhotographersForecast.Services
{
    public partial class GetMoonService : IGetMoonService
    {
        private HttpClient _client;

        public GetMoonService(IHttpClientFactory client)
        {
            _client = client.CreateClient("moonClient");
        }
        private async Task<T?> GetDataAsync<T>(HttpRequestMessage request)
        {
            try
            {
                // ensure response is disposed so connection returns to pool promptly
                using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"ERROR: {_client.BaseAddress} FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    return default;
                }

                await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new NullNumberConverterFactory());
                var result = await JsonSerializer.DeserializeAsync<T>(stream, options).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is ObjectDisposedException || ex is SocketException)
            {
                Debug.WriteLine($"Transient error: {ex.GetType().Name} - {ex.Message}");
                return default;
            }
        }

        public async Task<FutureMoonPhase[]> GetFutureMoonPhasesAsync(DateTime date, int selectedTimeZoneOffset)
        {
            string apiString = $"https://aa.usno.navy.mil/api/moon/phases/date?date={date.ToString("yyyy-MM-dd")}&nump=5";
            var request = new HttpRequestMessage(HttpMethod.Get, apiString);
            var json = await GetDataAsync<UsnoData.Rootobject>(request);

            if (json == null)
            {
                FutureMoonPhase moon = new FutureMoonPhase
                {
                    LocalDateTime = date,
                    MoonPhaseName = "No moon phases to report"
                };
                var moonArray = new FutureMoonPhase[1] { moon };
                return moonArray;
            }

            int phases = json.numphases;
            FutureMoonPhase[] moonPhases = new FutureMoonPhase[phases];
            for (int i = 0; i < json.phasedata.Length; i++)
            {
                var pd = json.phasedata[i];
                int year = pd.year;
                int month = pd.month;
                int day = pd.day;
                var utc = new DateTime(year, month, day, pd.time.Hour, pd.time.Minute, 0, DateTimeKind.Utc);
                moonPhases[i] = new FutureMoonPhase
                {
                    UtcDateTime = utc,
                    LocalDateTime = utc.AddSeconds(selectedTimeZoneOffset),
                    MoonPhaseName = pd.phase
                };
            }

            return moonPhases;
        }
    }
}
