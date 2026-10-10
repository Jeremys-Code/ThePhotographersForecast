using SkiaSharp;

namespace ThePhotographersForecast.Services.Modules
{
    public interface ISkyColorEngine
    {
        Color EstimateSkyColor(double irradianceWm2, DateTime time, double cloudCoverPercent, double sunAltitudeDeg, double sunAzimuthDeg, DateTime sunrise, DateTime sunset, DateTime solarNoon);
        Task<Dictionary<DateOnly, SKColor[]>> GetDailySkyColorGradientsAsync();
    }
}