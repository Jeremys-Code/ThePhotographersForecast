using SkiaSharp;
using System.Net.Http.Json;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Modules
{
    public class SkyColorEngine
    {
        private HttpClient _client;
        private SKColor[] startColors;
        private SKColor[] endColors;
        private float[] startPositions;
        private float[] endPositions;
        private float progress;
        private double irradianceWm2;
        private DateTime time;
        private double cloudCoverPercent;
        private double sunAltitudeDeg;
        private double sunAzimuthDeg;
        private DateTime sunrise;
        private DateTime sunset;
        private DateTime solarNoon;

        public void ForecastSkyColor(IHttpClientFactory client, double latitude, double longitude, DateTime startDate, DateTime endDate)
        {
            _client = client.CreateClient("photographer");
            string apiString = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latitude}&longitude={longitude}&hourly=pm10,pm2_5,nitrogen_dioxide,sulphur_dioxide,ozone,dust,uv_index,uv_index_clear_sky,aerosol_optical_depth,carbon_monoxide&start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}";
            try
            {
                // Early call to pull all Hourly Forecasts at once
                var allHourlyForecasts = GetHourly(item);
                // this.startColors = new SKColor[] { SKColor.Parse("#FF250032"), SKColor.Parse("#FF330018"), SKColor.Parse("#FF140158"), SKColor.Parse("#FF002C5B") };
                //var sky = GetColor();
                //this.endColors = new SKColor[] { InterpolateColor(this.startColors[0], sky), InterpolateColor(this.startColors[1], sky), InterpolateColor(this.startColors[2], sky), InterpolateColor(this.startColors[3], sky) };
                //Random random = new Random();
                //random.NextDouble();
                //float[] positionOne = new float[4] { 0.0f, 0.33f, 0.66f, 1.0f };
                //float[] positionTwo = new float[4] { 0.0f, 0.25f, 0.75f, 1.0f };
                //for (int i = 0; i < 4; i++)
                //{
                //    if (i == 0) { random.NextSingle()}
                //    positionOne[i] = (float)random.NextDouble();
                //    positionTwo[i] = (float)random.NextDouble();
                //};
                //this.startPositions = positionOne;
                //this.endPositions = positionTwo;
                // }

            }}

        private async Task<GfsDataService.Rootobject> GetSkyCompositionAsync(DateTime startDate, DateTime endDate, double latitude, double longitude)
        {
            string apiString = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latitude}&longitude={longitude}&hourly=pm10,pm2_5,nitrogen_dioxide,sulphur_dioxide,ozone,dust,uv_index,uv_index_clear_sky,aerosol_optical_depth,carbon_monoxide,ammonia&start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}";
            try
            {
                CancellationToken ct = new CancellationToken();
                var request = new HttpRequestMessage(HttpMethod.Get, apiString);
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!response.IsSuccessStatusCode)
                    return null;
                var item = await response.Content.ReadFromJsonAsync<SkyCompositionData.Rootobject>(cancellationToken: ct);
                int nbrOfDays = item.hourly.time.Length;
                var skyColors = new SkyColor[nbrOfDays];
                for (int s = 0; s < nbrOfDays; s++)
                {
                    skyColors[s] = new SkyColor
                    {
                        Time = item.hourly.time[s],
                        Pm10 = item.hourly.pm10[s],
                        Pm25 = item.hourly.pm2_5[s],
                        CarbonMonoxide = item.hourly.carbon_monoxide[s],
                        NitrogenDioxide = item.hourly.nitrogen_dioxide[s],
                        SulphurDioxide = item.hourly.sulphur_dioxide[s],
                        Ozone = item.hourly.ozone[s],
                        Dust = item.hourly.dust[s],
                        UvIndex = item.hourly.uv_index[s],
                        UvIndexClearSky = item.hourly.uv_index_clear_sky[s],
                        Irradiance = item.hourly.irradiance[s],
                        CloudCoverPercent = item.hourly.cloud_cover_percent[s],
                        SunAltitudeDeg = item.hourly.sun_altitude_deg[s],
                        SunAzimuthDeg = item.hourly.sun_azimuth_deg[s],
                        Sunrise = item.hourly.sunrise[s],
                        Sunset = item.hourly.sunset[s],
                        SolarNoon = item.hourly.solar_noon[s]
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching sky composition data: {ex.Message}");
                return null;
            }
        }

        public void DrawGradient(SKCanvas canvas)
        {
            var interpolatedColors = new SKColor[4];
            var interpolatedPositions = new float[4];

            for (int i = 0; i < 4; i++)
            {

                interpolatedColors[i] = InterpolateColor(startColors[i], endColors[i]);
                interpolatedPositions[i] = InterpolatePosition(startPositions[i], endPositions[i]);
            }

            if (DeviceDisplay.MainDisplayInfo.Width != 0 && DeviceDisplay.MainDisplayInfo.Height != 0)
            {
                using var paint = new SKPaint
                {

                    Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint((float)Shell.Current.Window.Width / 2, (float)Shell.Current.Window.Height),
                        interpolatedColors,
                        interpolatedPositions,
                        SKShaderTileMode.Clamp)
                };
                canvas.DrawRect(canvas.LocalClipBounds, paint);
            }
        }
        //Morphes the colors
        public SKColor InterpolateColor(SKColor c1, SKColor c2)
        {
            SKColor newInteropColor = new SKColor(
                (byte)(c1.Red + (c2.Red - c1.Red) * progress),
                (byte)(c1.Green + (c2.Green - c1.Green) * progress),
                (byte)(c1.Blue + (c2.Blue - c1.Blue) * progress),
                (byte)(c1.Alpha + (c2.Alpha - c1.Alpha) * progress));
            return newInteropColor;
        }

        //Moving the transition point of the color gradient
        private float InterpolatePosition(float p1, float p2)
        {
            float position = p1 + (p2 - p1) * progress;
            return position;
        }


        // Creates the predicted color of the sky
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
            // Normalize irradiance (original formula)
            double irradiancePercent = Math.Clamp(irradianceWm2 / 1000.0, 0, 1);

            // Normalize sun altitude (0° = horizon, 90° = zenith)
            double altitudeFactor = Math.Clamp(sunAltitudeDeg / 90.0, 0, 1);

            // Time-based transition factor (sunrise/sunset glow)
            double minutesFromSunrise = (time - sunrise).TotalMinutes;
            double minutesToSunset = (sunset - time).TotalMinutes;
            double transitionFactor = 0;

            if (minutesFromSunrise >= 0 && minutesFromSunrise <= 60)
                transitionFactor = 1 - (minutesFromSunrise / 60.0); // sunrise glow
            else if (minutesToSunset >= 0 && minutesToSunset <= 60)
                transitionFactor = 1 - (minutesToSunset / 60.0); // sunset glow

            // Cloud cover dampening
            double clarityFactor = 1.0 - (cloudCoverPercent / 100.0);

            // Composite intensity
            double intensity = irradiancePercent * altitudeFactor * clarityFactor;

            // Apply warm tint during transition periods
            if (transitionFactor > 0)
            {
                // Blend toward warm hues (orange/red)
                return Color.FromRgba(
                    (int)(255 * transitionFactor + 135 * (1 - transitionFactor)), // R
                    (int)(69 * transitionFactor + 206 * (1 - transitionFactor)),  // G
                    (int)(0 * transitionFactor + 235 * (1 - transitionFactor)),    // B
                    255
                );
            }

            // Map intensity to daytime gradient
            if (intensity > 0.8)
                return Color.FromRgb(135, 206, 235); // SkyBlue RGB
            else if (intensity > 0.5)
                return Color.FromRgb(135, 206, 250); // LightSkyBlue RGB
            else if (intensity > 0.2)
                return Color.FromRgb(176, 196, 222); // LightSteelBlue RGB
            else
                return Color.FromRgb(0, 44, 91); // DarkSlateGray RGB
        }

        //Creates the color array
        //public Color GetColor()
        //{

        //    DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
        //    List<Color> colors = new List<Color>();
        //    List<DateTime> dateTimes = new List<DateTime>();


        //    double irradianceWm2 = (double)_db.ForecastHour.Where(a => DateOnly.FromDateTime(a.Time) == currentDate && a.Time.Hour == DateTime.Now.Hour).Select(x => x.DirectNormalIrradiance).FirstOrDefault();
        //    double cloudCoverPercent = (double)_db.ForecastHour.Where(a => DateOnly.FromDateTime(a.Time) == currentDate && a.Time.Hour == DateTime.Now.Hour).Select(x => x.CloudCover).FirstOrDefault();
        //    double sunAltitudeDeg = _db.Astronomies.Where(a => a.ForecastTime == currentDate).Select(x => x.SunAltitude).FirstOrDefault();
        //    double sunAzimuthDeg = _db.Astronomies.Where(a => a.ForecastTime == currentDate).Select(x => x.SunAzimuth).FirstOrDefault();
        //    DateTime sunrise = _db.Astronomies.Where(a => a.ForecastTime == currentDate).Select(x => x.Morning.Sunrise).FirstOrDefault();
        //    DateTime sunset = _db.Astronomies.Where(a => a.ForecastTime == currentDate).Select(x => x.Evening.Sunset).FirstOrDefault();
        //    DateTime solarNoon = _db.Astronomies.Where(a => a.ForecastTime == currentDate).Select(x => x.Morning.SolarNoon).FirstOrDefault();


        //    var color = EstimateSkyColor(irradianceWm2, DateTime.Now, cloudCoverPercent, sunAltitudeDeg, sunAzimuthDeg, sunrise, sunset, solarNoon);

        //    return color;

        //Debug.WriteLine(" colors recorded.");


        //SKColor skColor = new SKColor(color.GetByteRed(), color.GetByteGreen(), color.GetByteBlue(), color.GetByteAlpha());
        //return skColor;

        //var kColors = new SKColor[4] { SKColor.Parse("#382365"), SKColor.Parse("#002C5B"), skColor, SKColor.Parse("#F2F27B") }; //Light Yellow 

        //return kColors;

        //        }

        // Creates a gradient
        //public SKShader ColorMorphing(float width, float height)
        //{
        //    var sky = GetColor();

        //    // Define two gradients to morph between
        //    //SKColor[] gradient2 = { SKColor.Parse("#FFB78BAF"), SKColor.Parse("#FF0068CC"), SKColor.Parse("#FFFFD075"), SKColor.Parse("#FFFFFFFF") };
        //    //SKColor[] gradient3 = { SKColor.Parse("#FFFFF4AF"), SKColor.Parse("#FFE3D092"), SKColor.Parse("#FF4388B1"), SKColor.Parse("#FF0068CC") };
        //    SKColor[] gradient2 = sky.ToArray();
        //    float[] positions = { 0.0f, 0.3f, 0.7f, 1.0f }; // Gradient positions



        //    // Interpolate colors
        //    SKColor[] morphedGradient = new SKColor[startColors.Length];
        //    for (int i = 0; i < startColors.Length; i++)
        //    {
        //        morphedGradient[i] = InterpolateColor(startColors[i], endColors[i]);
        //    }

        //    // Create the gradient shader
        //    using var shader = SKShader.CreateLinearGradient(
        //        new SKPoint(width / 2, height),
        //        new SKPoint(0, 0),
        //        morphedGradient,
        //        positions,
        //        SKShaderTileMode.Clamp
        //    );

        //    return shader;

        //    // Draw the gradient
        //    //            using var paint = new SKPaint { Shader = shader };
        //    //          canvas.DrawRect(new SKRect(0, 0, width, height), paint);

        //    // Save the result
        //    //using var image = surface.Snapshot();
        //    //using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        //    // System.IO.File.WriteAllBytes("GradientMorph.png", data);

        //    //Console.WriteLine("Gradient morphing image saved as 'GradientMorph.png'.");
        //}
    }
}



}
}
