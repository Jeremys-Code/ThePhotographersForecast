using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    public class AzureMapsLocationService
    {
        [PasswordPropertyText]
        private string mapKey;
        internal async Task SetMapKeyAsync()
        {
            string maps = await SecureStorage.GetAsync("Maps") ?? string.Empty;
            if (maps == string.Empty)
            {
                var lines = File.ReadAllLines("commonweathmall.jpg");
                if (lines[158].Contains("jtPhotoId"))
                {
                    maps = lines[158].Split(':').ToArray()[1].Trim();
                }
                else
                {
                    string line = lines.Where(x => x.Contains("jtPhotoId:")).First();
                    maps = line.Split(':').ToArray()[1].Trim();
                }
                await SecureStorage.SetAsync("Maps", maps);
            }
            mapKey = maps;
            return;
        }
        private readonly HttpClient _client;

        public AzureMapsLocationService(IHttpClientFactory client)
        {
            _client = client.CreateClient("photographer");
        }

        public async Task<List<PhotoLocation>> GetSuggestionsAsync(string query)
        {
            var url = $"https://atlas.microsoft.com/search/fuzzy/json" +
                      $"?api-version=1.0" +
                      $"&subscription-key={mapKey}" +
                      $"&query={Uri.EscapeDataString(query)}" +
                      $"&limit=5" +
                      $"&typeahead=true" +
                      $"&countrySet=US";

            var response = await _client.GetFromJsonAsync<AzureMapsUserEntryResponse.Rootobject>(url);

            return response?.results?
                .Select(r => new PhotoLocation
                {
                    //DisplayName = string.Concat((r.address.neighbourhood != null ? $"{r.address.neighbourhood}, " : string.Empty), (r.address.municipalitySubdivision != null ? $"{r.address.municipalitySubdivision}, " : string.Empty), r.address.municipality, ", ", r.address.countrySubdivision),
                    Latitude = r.position.lat,
                    Longitude = r.position.lon
                })
                .ToList() ?? new();
        }


    }
}


