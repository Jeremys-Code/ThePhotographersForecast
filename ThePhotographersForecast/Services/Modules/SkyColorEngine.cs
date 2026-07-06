using SkiaSharp;
using System.Diagnostics;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services.Modules
{
    public class SkyColorEngine
    {
        private readonly IHttpClientFactory _clientFactory;
        private HttpClient _client;
        private Session _session;
        private GetEventService _eventService;

        public SkyColorEngine(IHttpClientFactory clientFactory, Session session, GetEventService eventService)
        {
            _clientFactory = clientFactory;
            _client = _clientFactory.CreateClient("photographer");
            _session = session;
            _eventService = eventService;
        }

        // ------------------------------------------------------------
        // PUBLIC ENTRY POINT
        // ------------------------------------------------------------
        public async Task<Dictionary<DateOnly, SKColor[]>> GetDailySkyColorGradientsAsync()
        {
            var sky = await GetSkyCompositionAsync();
            if (sky == null)
                return new Dictionary<DateOnly, SKColor[]>();

            var daily = new Dictionary<DateOnly, SKColor[]>();

            var grouped = sky.GroupBy(s => DateOnly.FromDateTime(s.Time));

            foreach (var dayGroup in grouped)
            {
                var day = dayGroup.Key;
                var hourly = dayGroup.ToList();

                // Compute a representative color for the day
                var dailyColor = ComputeDailySkyColor(hourly);

                // Build a 4‑stop gradient using your signature palette
                var gradient = BuildGradientArray(dailyColor);

                daily[day] = gradient;
            }

            return daily;
        }

        // ------------------------------------------------------------
        // FETCH AIR QUALITY + WEATHER + SUN/MOON
        // ------------------------------------------------------------
        private async Task<SkyColor[]> GetSkyCompositionAsync()
        {
            try
            {
                // AIR QUALITY API
                string aqUrl =
                    $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={_session.SelectedLocation.Latitude}&longitude={_session.SelectedLocation.Longitude}" +
                    $"&hourly=pm10,pm2_5,nitrogen_dioxide,sulphur_dioxide,ozone,dust,uv_index,uv_index_clear_sky,aerosol_optical_depth,carbon_monoxide" +
                    $"&timezone=auto&start_date={_session.StartDate.ToString("yyyy-MM-dd")}&end_date={_session.EndDate.ToString("yyyy-MM-dd")}";

                var aqResponse = await _client.GetAsync(aqUrl);
                if (!aqResponse.IsSuccessStatusCode)
                    return null;

                var aqData = await aqResponse.Content.ReadFromJsonAsync<SkyCompositionData.Rootobject>();
                if (aqData == null)
                    return null;

                // WEATHER API
                string weatherUrl =
                    $"https://api.open-meteo.com/v1/forecast?latitude={_session.SelectedLocation.Latitude}&longitude={_session.SelectedLocation.Longitude}" +
                    $"&hourly=surface_pressure,cloud_cover,direct_normal_irradiance,diffuse_radiation,shortwave_radiation" +
                    $"&models=gfs_seamless&timezone=auto&start_date={_session.StartDate.ToString("yyyy-MM-dd")}&end_date={_session.EndDate.ToString("yyyy-MM-dd")}";

                var wxResponse = await _client.GetAsync(weatherUrl);
                if (!wxResponse.IsSuccessStatusCode)
                    return null;

                var wxData = await wxResponse.Content.ReadFromJsonAsync<SkyColorAttributesData.Rootobject>();
                if (wxData == null)
                    return null;

                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(wxData.timezone);
                int count = aqData.hourly.time.Length;
                var sky = new SkyColor[count];

                for (int i = 0; i < count; i++)
                {
                    var t = DateTime.Parse(aqData.hourly.time[i]);

                    var eventDay = _eventService.GetSelectedDayEventModel(DateOnly.FromDateTime(t));
                    if (eventDay == null)
                        return null;

                    sky[i] = new SkyColor
                    {
                        Time = t,

                        // Air quality
                        Pm10 = aqData.hourly.pm10[i],
                        Pm25 = aqData.hourly.pm2_5[i],
                        CarbonMonoxide = aqData.hourly.carbon_monoxide[i],
                        NitrogenDioxide = aqData.hourly.nitrogen_dioxide[i],
                        SulphurDioxide = aqData.hourly.sulphur_dioxide[i],
                        Ozone = aqData.hourly.ozone[i],
                        Dust = aqData.hourly.dust[i],
                        UvIndex = aqData.hourly.uv_index[i],
                        UvIndexClearSky = aqData.hourly.uv_index_clear_sky[i],

                        // Weather
                        SurfacePressure = wxData.hourly.surface_pressure[i],
                        CloudCoverPercent = wxData.hourly.cloud_cover[i],
                        DirectNormalIrradiance = wxData.hourly.direct_normal_irradiance[i],
                        DiffuseRadiation = wxData.hourly.diffuse_radiation[i],
                        ShortwaveRadiation = wxData.hourly.shortwave_radiation[i],

                        // Solar geometry
                        SunAltitudeDeg = eventDay.AstronomicalData.SunAltitude,
                        SunAzimuthDeg = eventDay.AstronomicalData.SunAzimuth,
                        Sunrise = eventDay.EventTimes.Where(s => s.EventName == "Sunrise").First().AstroTime,
                        Sunset = eventDay.EventTimes.Where(s => s.EventName == "Sunset").First().AstroTime,
                        SolarNoon = eventDay.EventTimes.Where(s => s.EventName == "Solar Noon").First().AstroTime
                    };
                }

                return sky;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sky composition error: {ex.Message}");
                return null;
            }
        }

        // ------------------------------------------------------------
        // COMPUTE DAILY SKY COLOR
        // ------------------------------------------------------------
        private SKColor ComputeDailySkyColor(List<SkyColor> hourly)
        {
            // Weighted average of hourly colors
            double r = 0, g = 0, b = 0;
            double totalWeight = 0;

            foreach (var h in hourly)
            {
                var c = EstimateSkyColor(
                    h.DirectNormalIrradiance,
                    h.Time,
                    h.CloudCoverPercent,
                    h.SunAltitudeDeg,
                    h.SunAzimuthDeg,
                    h.Sunrise,
                    h.Sunset,
                    h.SolarNoon);

                double w = Math.Max(0.1, h.DirectNormalIrradiance / 1000.0);

                r += c.Red * w;
                g += c.Green * w;
                b += c.Blue * w;

                totalWeight += w;
            }

            return new SKColor(
                (byte)(r / totalWeight),
                (byte)(g / totalWeight),
                (byte)(b / totalWeight));
        }

        // ------------------------------------------------------------
        // BUILD 4‑STOP GRADIENT FOR A DAY
        // ------------------------------------------------------------
        private SKColor[] BuildGradientArray(SKColor dailyColor)
        {
            return new SKColor[]
            {
                SKColor.Parse("#382365"),   // deep twilight purple
                SKColor.Parse("#002C5B"),   // deep blue
                dailyColor,                 // predicted sky color
                SKColor.Parse("#F2F27B")    // warm yellow highlight
            };
        }

        // ------------------------------------------------------------
        // SKY COLOR ESTIMATION (your physics model)
        // ------------------------------------------------------------
        public Color EstimateSkyColor(
            double irradianceWm2,
            DateTime time,
            double cloudCoverPercent,
            double sunAltitudeDeg,
            double sunAzimuthDeg,
            DateTime sunrise,
            DateTime sunset,
            DateTime solarNoon)
        {
            double irradiancePercent = Math.Clamp(irradianceWm2 / 1000.0, 0, 1);
            double altitudeFactor = Math.Clamp(sunAltitudeDeg / 90.0, 0, 1);
            double clarityFactor = 1.0 - (cloudCoverPercent / 100.0);

            double intensity = irradiancePercent * altitudeFactor * clarityFactor;

            double minutesFromSunrise = (time - sunrise).TotalMinutes;
            double minutesToSunset = (sunset - time).TotalMinutes;

            double transitionFactor = 0;
            if (minutesFromSunrise >= 0 && minutesFromSunrise <= 60)
                transitionFactor = 1 - (minutesFromSunrise / 60.0);
            else if (minutesToSunset >= 0 && minutesToSunset <= 60)
                transitionFactor = 1 - (minutesToSunset / 60.0);

            if (transitionFactor > 0)
            {
                return Color.FromRgba(
                    (int)(255 * transitionFactor + 135 * (1 - transitionFactor)),
                    (int)(69 * transitionFactor + 206 * (1 - transitionFactor)),
                    (int)(0 * transitionFactor + 235 * (1 - transitionFactor)),
                    255);
            }

            if (intensity > 0.8) return Color.FromRgb(135, 206, 235);
            if (intensity > 0.5) return Color.FromRgb(135, 206, 250);
            if (intensity > 0.2) return Color.FromRgb(176, 196, 222);

            return Color.FromRgb(0, 44, 91);
        }
    }
}