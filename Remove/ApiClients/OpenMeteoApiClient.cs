using System.Diagnostics;

namespace ThePhotographersForecast.ApiClients
{
    public class OpenMeteoApiClient : IOpenMeteoApiClient
    {
        private readonly HttpClient _http;

        public OpenMeteoApiClient(IHttpClientFactory http)
        {
            _http = http.CreateClient("photographer");
        }

        public async Task<string> GetOpenMeteoDataAsync<T>(HttpRequestMessage request)
        {

            var content = string.Empty;
            request.Headers.Add("Accept", "application/json");
            try
            {
                using (var response = await _http.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead))
                {

                    if (!response.IsSuccessStatusCode)
                    {

                        throw new ApplicationException($"ERROR: OPENMETEOAPICLIENT FAILED AT GETOPENMETEODATAASYNC: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    }
                    Debug.WriteLine($"SUCCESSFULLY RECEIVED JSON FROM GETOPENMETEODATAASYNC: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    content = await response.Content.ReadAsStringAsync();
                }
            }
            catch (ApplicationException ex)
            {
                // Log the exception or handle it as needed
                Debug.WriteLine(ex);
            }
            return content;
        }
    }
}
