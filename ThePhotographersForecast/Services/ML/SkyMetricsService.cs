using System.Diagnostics;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services.ML
{
    public class SkyMetricsService : ISkyMetricsService
    {
        private ISession _session;
        private HttpClient _client;

        public SkyMetricsService(ISession session, HttpClient client)
        {
            _session = session;
            _client = client;
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

                File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, $"skydata_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}_{name}.json"), jsonFileContent);
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

        public async Task<SkyHourData[]> GetSkyModelDataAsync()
        {
            var item = new SkyData.Rootobject();
            string api = $@"https://api.open-meteo.com/v1/forecast?latitude={_session.SelectedLocation.Latitude}&longitude={_session.SelectedLocation.Longitude}&hourly=temperature_2m,cloud_cover,cloud_cover_low,cloud_cover_mid,cloud_cover_high,visibility,relative_humidity_2m,wind_direction_120m,wind_speed_120m,wind_gusts_10m,precipitation_probability,precipitation,apparent_temperature&timezone=auto&models=ncep_gfs_seamless&start_date={_session.StartDate:yyyy-MM-dd}&end_date={_session.EndDate:yyyy-MM-dd}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(api));
                item = await GetDataAsync<SkyData.Rootobject>(request, "skydata");
                if (item == null) return default;

                var skyData = new SkyHourData[item.hourly.time.Length];

                for (int s = 0; s < item.hourly.time.Length; s++)
                {
                    skyData[s] = new SkyHourData
                    {
                        Time = item.hourly.time[s],
                        ApparentTemperature = new SkyLayerFloat { Value = item.hourly.apparent_temperature[s], UnitCode = item.hourly_units.apparent_temperature },
                        CloudCover = new SkyLayerInt { Value = item.hourly.cloud_cover[s], UnitCode = item.hourly_units.cloud_cover },
                        CloudCoverHigh = new SkyLayerInt { Value = item.hourly.cloud_cover_high[s], UnitCode = item.hourly_units.cloud_cover_high },
                        CloudCoverLow = new SkyLayerInt { Value = item.hourly.cloud_cover_low[s], UnitCode = item.hourly_units.cloud_cover_low },
                        CloudCoverMid = new SkyLayerInt { Value = item.hourly.cloud_cover_mid[s], UnitCode = item.hourly_units.cloud_cover_mid },
                        Precipitation = new SkyLayerFloat { Value = item.hourly.precipitation[s], UnitCode = item.hourly_units.precipitation },
                        PrecipitationProbability = new SkyLayerInt { Value = item.hourly.precipitation_probability[s], UnitCode = item.hourly_units.precipitation_probability },
                        RelativeHumidity2m = new SkyLayerInt { Value = item.hourly.relative_humidity_2m[s], UnitCode = item.hourly_units.relative_humidity_2m },
                        Temperature2m = new SkyLayerFloat { Value = item.hourly.temperature_2m[s], UnitCode = item.hourly_units.temperature_2m },
                        Visibility = new SkyLayerFloat { Value = item.hourly.visibility[s], UnitCode = item.hourly_units.visibility },
                        WindDirection120m = new SkyLayerInt { Value = item.hourly.wind_direction_120m[s], UnitCode = item.hourly_units.wind_direction_120m },
                        WindSpeed120m = new SkyLayerFloat { Value = item.hourly.wind_speed_120m[s], UnitCode = item.hourly_units.wind_speed_120m },
                        WindGusts10m = new SkyLayerFloat { Value = item.hourly.wind_gusts_10m[s], UnitCode = item.hourly_units.wind_gusts_10m }
                    };
                }

                return skyData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Capturing Additional Sky Attributes for Inference {ex}");
                return default;
            }
        }
    }
}
