using CommunityToolkit.Maui.Core.Extensions;
using ThePhotographersForecast.Models.ObserverCalc;
using ThePhotographersForecast.Models.ObserverCalc.Dtos;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services.Modules
{
    public interface ISkyColorCalculator
    {
        Color GetSkyColor(DateTime date, double latitude, double longitude);
    }

    public class SkyColorCalculator : ISkyColorCalculator
    {
        /// <summary>
        /// Calculates an approximate sky color based on sun elevation.
        /// </summary>
        /// <param name="sunElevationDegrees">Sun elevation angle in degrees (0 = horizon, 90 = directly overhead)</param>
        /// <returns>Estimated sky color as a System.Drawing.Color</returns>
        private ISession _session;
        public SkyColorCalculator(ISession session)
        {

        }
        public Color GetSkyColor(DateTime date, double latitude, double longitude)
        {
            // The suns elevation
            var sunElevationDegrees = SunCalc.GetPosition(date, latitude, longitude).Altitude;
            // Clamp input to valid range
            sunElevationDegrees = Math.Max(0, Math.Min(90, sunElevationDegrees));

            // Normalize elevation to 0..1
            double t = sunElevationDegrees / 90.0;

            // Rayleigh scattering approximation:
            // Higher sun = more blue, lower sun = more orange/red
            // We'll interpolate between horizon color and zenith color
            Color horizonColor = Color.Parse("#F7CD5D"); // warm orange
            Color zenithColor = Color.Parse("#87CEED"); // sky blue


            return LerpColor(horizonColor, zenithColor, t);
        }

        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        private Color LerpColor(Color horizonColor, Color zenithColor, double t)
        {
            byte hR = horizonColor.GetByteRed();
            byte hG = horizonColor.GetByteGreen();
            byte hB = horizonColor.GetByteBlue();
            byte zR = zenithColor.GetByteRed();
            byte zG = zenithColor.GetByteGreen();
            byte zB = zenithColor.GetByteBlue();
            byte r = (byte)(hR + (zR - hR) * t);
            byte g = (byte)(hG + (zG - hG) * t);
            byte b = (byte)(hB + (zB - hB) * t);
            return Color.FromRgb(r, g, b);
        }
    }
}
