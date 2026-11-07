using System.Diagnostics;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ApiClients
{
    public class IpGeolocationApiClient : IIpGeolocationApiClient
    {
        private readonly HttpClient _http;
        private Session _session;
        public IpGeolocationApiClient(IHttpClientFactory http, Session session)
        {
            _http = http.CreateClient("photographer");
            _session = session;
        }

        public async Task<string> GetIpGeolocationDataAsync<T>(HttpRequestMessage request)
        {
            string content = string.Empty;
            string apiKey = "b3c99c6a0b9e44169fa96b6918495732"; // Replace with
            request.Headers.Add("Accept", "application/json");
            _http.BaseAddress = new Uri($"https://api.ipgeolocation.io/v3/astronomy/timeSeries?apiKey={apiKey}&dateStart={_session.StartDate.ToString("yyyy-MM-dd")}&dateEnd={_session.EndDate.ToString("yyyy-MM-dd")}&lat={_session.SelectedLocation.Latitude}&long={_session.SelectedLocation.Longitude}");
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0");
            try
            {
                using (var response = await _http.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead))
                {

                    if (!response.IsSuccessStatusCode)
                    {

                        throw new ApplicationException($"ERROR: IPGEOLOCATIONAPICLIENT FAILED AT GETIPGEOLOCATIONDATAASYNC: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    }
                    Debug.WriteLine($"SUCCESSFULLY RECEIVED JSON FROM GETIPGEOLOCATIONDATAASYNC: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
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
