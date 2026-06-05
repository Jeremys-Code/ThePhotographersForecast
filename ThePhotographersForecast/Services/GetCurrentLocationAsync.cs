using System.Diagnostics;

namespace ThePhotographersForecast.Services;

public class LocationServices
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;
    public async Task GetCurrentLocationAsync()
    {
        try
        {
            _isCheckingLocation = true;
            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10)
            );
            _cancelTokenSource = new CancellationTokenSource();
            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
            if (location != null)
            {

                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }
        }
        catch (FeatureNotSupportedException) { /* Not supported */ }
        catch (FeatureNotEnabledException) { /* Location disabled */ }
        catch (PermissionException) { /* Permission denied */ }
        catch (Exception) { /* Other errors */ }
        finally
        {
            _isCheckingLocation = false;
        }
    }
    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource?.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
}
