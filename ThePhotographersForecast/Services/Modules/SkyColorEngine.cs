using SkiaSharp;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.JsonConverters;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services.Modules
{
    public class SkyColorEngine : ISkyColorEngine
    {
        private readonly HttpClient _client;
        public Session SharedSession { get; set; }
        private IGetEventService _eventService;

        public SkyColorEngine(IHttpClientFactory client, Session sharedSession, IGetEventService eventService)
        {
            _client = client.CreateClient("skyClient");
            SharedSession = sharedSession;
            _eventService = eventService;
        }
        private async Task<T?> GetDataAsync<T>(HttpRequestMessage request)
        {
            try
            {
                // ensure response is disposed so connection returns to pool promptly
                using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"ERROR: {_client.BaseAddress} FAILED: Status Code {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    return default;
                }

                await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new NullNumberConverterFactory());
                var result = await JsonSerializer.DeserializeAsync<T>(stream, options).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is ObjectDisposedException || ex is SocketException)
            {
                Debug.WriteLine($"Transient error: {ex.GetType().Name} - {ex.Message}");
                return default;
            }
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
                //                var dailyBrush = ToLinearGradientBrush(gradient, new Point(0, 0), new Point(0, 1));
                daily[day] = gradient;



            }

            return daily;
        }
        private LinearGradientBrush ToLinearGradientBrush(SKColor[] colors,
                                                          Microsoft.Maui.Graphics.Point startPoint,
                                                            Microsoft.Maui.Graphics.Point endPoint)
        {
            if (colors == null || colors.Length == 0)
                throw new ArgumentException("Colors array cannot be null or empty.", nameof(colors));

            // Create gradient stops evenly spaced
            var gradientStops = new GradientStopCollection();
            for (int i = 0; i < colors.Length; i++)
            {
                double offset = (double)i / (colors.Length - 1); // 0 to 1
                gradientStops.Add(new GradientStop(
                    Color.FromRgba(colors[i].Red, colors[i].Green, colors[i].Blue, colors[i].Alpha),
                    (float)offset
                ));
            }

            return new LinearGradientBrush(gradientStops, startPoint, endPoint);
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
                    $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={SharedSession.SelectedLocation.Latitude}&longitude={SharedSession.SelectedLocation.Longitude}&hourly=pm10,pm2_5,nitrogen_dioxide,sulphur_dioxide,ozone,dust,uv_index,uv_index_clear_sky,aerosol_optical_depth,carbon_monoxide&timezone=auto&start_date={DateTime.Now.Date.ToString("yyyy-MM-dd")}&end_date={DateTime.Now.AddDays(3).Date.ToString("yyyy-MM-dd")}";
                var aqRequest = new HttpRequestMessage(HttpMethod.Get, aqUrl);
                aqRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "6.0"));
                var aqResult = await GetDataAsync<SkyCompositionData.Rootobject>(aqRequest);
                if (aqResult == null) return null;

                // WEATHER API
                string weatherUrl =
                    $"https://api.open-meteo.com/v1/forecast?latitude={SharedSession.SelectedLocation.Latitude}&longitude={SharedSession.SelectedLocation.Longitude}&hourly=surface_pressure,cloud_cover,direct_normal_irradiance,diffuse_radiation,shortwave_radiation&models=gfs_seamless&timezone=auto&start_date={DateTime.Now.Date.ToString("yyyy-MM-dd")}&end_date={DateTime.Now.AddDays(3).ToString("yyyy-MM-dd")}";
                var weatherRequest = new HttpRequestMessage(HttpMethod.Get, weatherUrl);
                weatherRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "6.0"));
                var airQ = await GetDataAsync<SkyColorAttributesData.Rootobject>(weatherRequest);
                if (airQ == null) return null;


                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(aqResult.timezone);
                int count = airQ.hourly.time.Length;
                var sky = new SkyColor[count];

                for (int i = 0; i < count; i++)
                {
                    var t = airQ.hourly.time[i];

                    var eventDay = await _eventService.GetSelectedDayEventModelAsync(t);
                    if (eventDay == null)
                        return null;

                    sky[i] = new SkyColor
                    {
                        Time = t,

                        // Air quality
                        Pm10 = aqResult.hourly.pm10[i],
                        Pm25 = aqResult.hourly.pm2_5[i],
                        CarbonMonoxide = aqResult.hourly.carbon_monoxide[i],
                        NitrogenDioxide = aqResult.hourly.nitrogen_dioxide[i],
                        SulphurDioxide = aqResult.hourly.sulphur_dioxide[i],
                        Ozone = aqResult.hourly.ozone[i],
                        Dust = aqResult.hourly.dust[i],
                        UvIndex = aqResult.hourly.uv_index[i],
                        UvIndexClearSky = aqResult.hourly.uv_index_clear_sky[i],

                        // Weather
                        SurfacePressure = airQ.hourly.surface_pressure[i],
                        CloudCoverPercent = airQ.hourly.cloud_cover[i],
                        DirectNormalIrradiance = airQ.hourly.direct_normal_irradiance[i],
                        DiffuseRadiation = airQ.hourly.diffuse_radiation[i],
                        ShortwaveRadiation = airQ.hourly.shortwave_radiation[i],

                        // Solar geometry
                        // SunAltitudeDeg = eventDay.AstronomicalData.SunAltitude,
                        //  SunAzimuthDeg = eventDay.AstronomicalData.SunAzimuth,
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