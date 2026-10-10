using System.Diagnostics;
using System.Text.Json;

namespace ThePhotographersForecast.ApiClients
{
    public class NwsApiClient : IApiClient
    {
        private readonly HttpClient _http;

        public NwsApiClient(IHttpClientFactory http)
        {
            _http = http.CreateClient("nwsClient");
        }

        public async Task<T?> GetDataAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            // Make retries for transient socket/IO/HTTP issues. Only safe to retry when request has no content (GET/HEAD).
            const int maxAttempts = 3;
            var attempt = 0;
            var delay = TimeSpan.FromSeconds(1);
            var canRetry = request.Content == null && (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head);

            HttpRequestMessage CloneHeaders(HttpRequestMessage original)
            {
                var clone = new HttpRequestMessage(original.Method, original.RequestUri);
                foreach (var header in original.Headers)
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
                if (original.Version != null)
                    clone.Version = original.Version;
                return clone;
            }

            while (true)
            {
                attempt++;
                // Clone headers into a fresh HttpRequestMessage because instances are generally not safe to reuse.
                using var reqToSend = CloneHeaders(request);
                try
                {
                    using (var response = await _http.SendAsync(reqToSend, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            Debug.WriteLine($"ERROR: NWSCLIENT FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                            return default;
                        }

                        Debug.WriteLine($"SUCCESSFULLY RECEIVED JSON FROM NWSCLIENT: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken).ConfigureAwait(false);
                        return result;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Caller requested cancellation - propagate as completed without result
                    Debug.WriteLine("GetDataAsync cancelled by caller.");
                    return default;
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is System.IO.IOException || ex is ObjectDisposedException || ex is System.Net.Sockets.SocketException)
                {
                    Debug.WriteLine($"Transient error on attempt {attempt}: {ex.GetType().Name} - {ex.Message}");
                    if (!canRetry || attempt >= maxAttempts || cancellationToken.IsCancellationRequested)
                    {
                        Debug.WriteLine("Giving up after transient errors.");
                        return default;
                    }

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return default;
                    }

                    // exponential backoff
                    delay = TimeSpan.FromSeconds(Math.Min(30, delay.TotalSeconds * 2));
                    // retry loop
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Non-transient error fetching NWS data: {ex}");
                    return default;
                }
            }
        }
    }
}
