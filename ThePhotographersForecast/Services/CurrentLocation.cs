using System.Diagnostics;

namespace ThePhotographersForecast.Services
{
    public static class CurrentLocation
    {
        public static double Latitude { private set; get; }
        public static double Longitude { private set; get; }

        private static CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        //public static async Task<(double)> GetCurrentLocation()
        public static async Task<(double lat, double lon)?> GetCurrentLocation()
        {
            try
            {

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

                _cancelTokenSource = new CancellationTokenSource();

                var location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
                //var place = await Geocoding.GetPlacemarksAsync(location);
                if (location != null)
                {
                    //Sample Latitudes and Longitudes to test different locations around the world:
                    //            Berlin, 52.520007, 13.404954
                    //            London, 51.507351, -0.127758
                    //            Moscow, 55.755826, 37.6173
                    //            Mountain Vi ..., 37.386052, -122.083851
                    //            Mumbai, 19.075984, 72.877656
                    //            San Francisco,37.774929, -122.419416
                    //            Shanghai, 31.230416, , 121.473701
                    //            São Paulo, -23.55052,-46.633309
                    //            Tokyo, 35.689487, 139.691706
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;
                    //string apiString = "https://nominatim.openstreetmap.org/reverse?lat={location.Latitude}&lon={location.Longitude}&format=json";
                    //using (var client = new HttpClient())
                    //{
                    //    client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
                    //    var response = await client.GetAsync(new Uri(apiString));
                    //    if (response.IsSuccessStatusCode)
                    //    {
                    //        var results = await response.Content.ReadFromJsonAsync<OpenStreetMapsReverseGeocoderData.Rootobject>();
                    //        var displayName = results.display_name.Split(',').ToArray();
                    //        for (int x = displayName.Length - 1; 0 <= x; x--)
                    //        {
                    //            if (displayName[x].Contains("County"))
                    //            {
                    //                x = 1;
                    //            }
                    //            if (x == displayName.Length - 1)
                    //                SelectedLocation.Country = displayName[x].Trim();
                    //            if (x == displayName.Length - 2)
                    //                SelectedLocation.State = displayName[x].Trim();
                    //            if (x == displayName.Length - 3)
                    //                _ = displayName[x].Trim();
                    //            if (x == displayName.Length - 4)
                    //                SelectedLocation.City = displayName[x].Trim();
                    //        }
                    //    }
                    //}

                    return (Latitude, Longitude);

                }
                if (location == null)
                {
                    Debug.WriteLine("Unable to access the devices current location.");
                }
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                //Unable to get coordinates from device GPS. Inform the user and suggest they enable location services or enter their location manually.
                await Shell.Current.DisplayAlertAsync("PhotoLocation Access", "Unable to get current coordinates. Please ensure location services are enabled and try again, or enter your location manually below.", "OK");
                Debug.WriteLine($"Get Current PhotoLocation: Unable to get current coordinates");
                Debug.WriteLine(ex);
            }
            return null;
        }




        public static async Task<(double lat, double lon)?> VerifyPermissionAsync()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
            {
                var deviceLocation = await GetCurrentLocation();

                if (deviceLocation != null)
                {
                    return (deviceLocation.Value.lat, deviceLocation.Value.lon); ;
                }
            }

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                await Shell.Current.DisplayAlertAsync("The Photographer's Forecast Permission Denied", "Please enable location services in settings to allow the app to access your time and location.", "OK");

            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // Prompt the user with additional information as to why the permission is needed
                await Shell.Current.DisplayAlertAsync("The Photographer's Forecast Permission", "In order to show you local weather and times of the day please allow location access.", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
            {
                var deviceLocation = await GetCurrentLocation();
                if (deviceLocation != null)
                {
                    return (deviceLocation.Value.lat, deviceLocation.Value.lon);
                }
            }
            else
            {

                await Shell.Current.DisplayAlertAsync("The Photographer's Forecast Permission Denied", "You have denied location access. You can enter your location manually on the Select Location Page.", "OK");
                return null;

            }

            return null;

        }
    }
}
