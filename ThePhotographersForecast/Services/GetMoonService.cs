using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    public partial class GetMoonService : ObservableObject, IGetMoonService
    {

        private ISession _session;
        private readonly HttpClient _client;
        public GetMoonService(HttpClient client, ISession session)
        {
            _client = client;
            _session = session;
        }
        private async Task<T?> GetDataAsync<T>(HttpRequestMessage request, string name, CancellationToken cancellationToken = default)
        {
            try
            {
                // ensure response is disposed so connection returns to pool promptly
                using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"ERROR: {_client.BaseAddress} FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    return default;
                }

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new NullNumberConverterFactory());

                // pass the cancellation token to DeserializeAsync so the read can be cancelled cleanly
                var result = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
                string jsonFileContent = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true // optional: for readable output
                });

                File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, $"moon_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}_{name}.json"), jsonFileContent);
                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Request cancelled.");
                return default;
            }
            catch (Exception ex) //handle transient IO/HTTP/Socket/ObjectDisposed etc.
            {
                Debug.WriteLine($"Transient error: {ex.GetType().Name} - {ex.Message}");
                return default;
            }
        }

        public async Task<FutureMoonPhase[]> GetFutureMoonPhasesAsync(DateTime date, int selectedTimeZoneOffset)
        {
            string apiString = $"https://aa.usno.navy.mil/api/moon/phases/date?date={date.ToString("yyyy-MM-dd")}&nump=5";
            var request = new HttpRequestMessage(HttpMethod.Get, apiString);
            var json = await GetDataAsync<UsnoData.Rootobject>(request, "phases");

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
                TimeOnly time = TimeOnly.TryParse(pd.time, out var moonTime) ? moonTime : default;
                int year = pd.year;
                int month = pd.month;
                int day = pd.day;
                var utc = new DateTime(year, month, day, time.Hour, time.Minute, 0, DateTimeKind.Utc);
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
