using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace ThePhotographersForecast.Services.ML;

/// <summary>
/// Condition classes matching the 8 softmax outputs of photographers_forecast.onnx.
/// </summary>
public enum PhotoWeatherCondition
{
 //   ClearTwilights = 0,
 //    VibrantBlueHours = 1,
    ClearCrisp = 0,
    GoldenDramaticLight = 1,
    PartlyCloudy = 2,
    OvercastFlatLight = 3,
    WindyGusty = 4,
    MistyFogRisk = 5,
    RainShowers = 6,
    SevereStormy = 7
}

/// <summary>
/// Hourly input record populated from api.weather.gov gridpoint/hourly data
/// and your solar ephemeris engine.
/// </summary>
public sealed record HourlyNwsInput(
    DateTimeOffset LocalTime,
    float TemperatureC,
    float ApparentTemperatureC,
    float DewpointC,
    float RelativeHumidityPct,
    float WindSpeedKmh,
    float? WindGustKmh,
    float WindDirectionDeg,
    float SkyCoverPct,
    float PrecipProbPct,
    float SolarElevationDeg,
    float CeilingHeightM = 10000f,
    float CloudCoverLowPct = 0f,
    float CloudCoverMidPct = 0f,
    float CloudCoverHighPct = 0f,
    float VisibilityKm = 16f,
    float WindSpeed120mKmh = 0f,
    float PrecipitationMm = 0f,
    float MoonElevationDeg = 0f,
    float MoonIllumination = 0f);

public sealed record HourlyForecastPrediction(
    DateTimeOffset Timestamp,
    float DroneFlightScore,
    float BlueHourScore,
    float SunriseScore,
    float SunsetScore,
    float LunarEventScore,
    float GalaxyScore,
    float OverallScore,
    PhotoWeatherCondition Condition,
    float ConditionConfidence,
    string HumanReadableForecast)
{
    public DateTimeOffset LocalTime => Timestamp;
    public float PhotoScore => OverallScore;
    public float DroneSafetyScore => DroneFlightScore;
}

public sealed record DailyForecastSummary(
    DateOnly Date,
    float DailyPhotoScore,
    float AverageDroneSafetyScore,
    PhotoWeatherCondition DominantCondition,
    IReadOnlyList<HourlyForecastPrediction> BestHours,
    string HumanReadableDailySummary,
    IReadOnlyList<HourlyForecastPrediction> HourlyPredictions);

/// <summary>
/// Thread-safe singleton ONNX inference service for The Photographer's Forecast.
/// Register in MauiProgram.cs via: builder.Services.AddSingleton&lt;PhotographersForecastOnnxService&gt;();
/// </summary>
public sealed class PhotographersForecastOnnxService : IDisposable
{
    private const string ModelAssetName = "photographers_forecast.onnx";
    private const int FeatureCount = 21;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private InferenceSession? _session;
    private bool _disposed;

    /// <summary>
    /// Lazily loads photographers_forecast.onnx from MAUI Resources/Raw (MauiAsset) once.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null)
            return;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_session is not null)
                return;

            await using Stream assetStream = await FileSystem.OpenAppPackageFileAsync(ModelAssetName)
                .ConfigureAwait(false);
            using var ms = new MemoryStream();
            await assetStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);

            var options = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            };

            _session = new InferenceSession(ms.ToArray(), options);
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Runs batched ONNX inference across all NWS forecast hours and returns
    /// daily photography scores, best hours, and human-readable summaries.
    /// </summary>
    public async Task<IReadOnlyList<DailyForecastSummary>> PredictForecastAsync(
        IReadOnlyList<HourlyNwsInput> hours,
        int topHoursPerDay = 5,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (hours is null || hours.Count == 0)
            return Array.Empty<DailyForecastSummary>();

        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        InferenceSession session = _session ?? throw new InvalidOperationException("ONNX session failed to initialize.");

        int n = hours.Count;
        var inputData = new float[n * FeatureCount];

        for (int i = 0; i < n; i++)
        {
            HourlyNwsInput h = hours[i];
            int offset = i * FeatureCount;

            float gustKmh = h.WindGustKmh ?? (h.WindSpeedKmh * 1.25f);
            float windRad = h.WindDirectionDeg * (MathF.PI / 180f);
            float daylightFlag = h.SolarElevationDeg >= 0f
                ? 1.0f
                : (h.SolarElevationDeg >= -6.0f ? 0.5f : 0.0f);

            inputData[offset + 0] = h.TemperatureC;
            inputData[offset + 1] = h.ApparentTemperatureC;
            inputData[offset + 2] = h.DewpointC;
            inputData[offset + 3] = h.RelativeHumidityPct;
            inputData[offset + 4] = h.WindSpeedKmh;
            inputData[offset + 5] = gustKmh;
            inputData[offset + 6] = MathF.Sin(windRad);
            inputData[offset + 7] = MathF.Cos(windRad);
            inputData[offset + 8] = h.SkyCoverPct;
            inputData[offset + 9] = h.PrecipProbPct;
            inputData[offset + 10] = h.SolarElevationDeg;
            inputData[offset + 11] = daylightFlag;
            inputData[offset + 12] = h.CeilingHeightM;
            inputData[offset + 13] = h.CloudCoverLowPct;
            inputData[offset + 14] = h.CloudCoverMidPct;
            inputData[offset + 15] = h.CloudCoverHighPct;
            inputData[offset + 16] = h.VisibilityKm;
            inputData[offset + 17] = h.WindSpeed120mKmh > 0f ? h.WindSpeed120mKmh : h.WindSpeedKmh * 1.25f;
            inputData[offset + 18] = h.PrecipitationMm;
            inputData[offset + 19] = h.MoonElevationDeg;
            inputData[offset + 20] = h.MoonIllumination;
        }

        var inputTensor = new DenseTensor<float>(inputData, new[] { n, FeatureCount });
        var inputs = new List<NamedOnnxValue>(1)
        {
            NamedOnnxValue.CreateFromTensor("nws_hourly_features", inputTensor)
        };

        // InferenceSession.Run is thread-safe for concurrent reads in ONNX Runtime.
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

        Tensor<float> droneScores = results.First(r => r.Name == "hourly_drone_flight_score").AsTensor<float>();
        Tensor<float> blueHourScores = results.First(r => r.Name == "hourly_blue_hour_score").AsTensor<float>();
        Tensor<float> sunriseScores = results.First(r => r.Name == "hourly_sunrise_score").AsTensor<float>();
        Tensor<float> sunsetScores = results.First(r => r.Name == "hourly_sunset_score").AsTensor<float>();
        Tensor<float> lunarScores = results.First(r => r.Name == "hourly_lunar_event_score").AsTensor<float>();
        Tensor<float> galaxyScores = results.First(r => r.Name == "hourly_galaxy_score").AsTensor<float>();
        Tensor<float> overallScores = results.First(r => r.Name == "hourly_overall_score").AsTensor<float>();
        Tensor<float> condProbs = results.First(r => r.Name == "hourly_condition_probs").AsTensor<float>();

        var hourlyPredictions = new List<HourlyForecastPrediction>(n);
        for (int i = 0; i < n; i++)
        {
            float dScore = Math.Clamp(droneScores[i, 0], 0f, 100f);
            float blueScore = Math.Clamp(blueHourScores[i, 0], 0f, 100f);
            float sunriseScore = Math.Clamp(sunriseScores[i, 0], 0f, 100f);
            float sunsetScore = Math.Clamp(sunsetScores[i, 0], 0f, 100f);
            float lunarScore = Math.Clamp(lunarScores[i, 0], 0f, 100f);
            float galaxyScore = Math.Clamp(galaxyScores[i, 0], 0f, 100f);
            float overallScore = Math.Clamp(overallScores[i, 0], 0f, 100f);

            int bestClass = 0;
            float bestProb = condProbs[i, 0];
            for (int c = 1; c < 8; c++)
            {
                float p = condProbs[i, c];
                if (p > bestProb)
                {
                    bestProb = p;
                    bestClass = c;
                }
            }

            var condition = (PhotoWeatherCondition)bestClass;
            string hourlyText = BuildHourlyForecastText(hours[i], overallScore, dScore, condition);

            hourlyPredictions.Add(new HourlyForecastPrediction(
                hours[i].LocalTime,
                dScore,
                blueScore,
                sunriseScore,
                sunsetScore,
                lunarScore,
                galaxyScore,
                overallScore,
                condition,
                bestProb,
                hourlyText));
        }

        return AggregateByDay(hours, hourlyPredictions, topHoursPerDay);
    }

    private static IReadOnlyList<DailyForecastSummary> AggregateByDay(
        IReadOnlyList<HourlyNwsInput> inputs,
        IReadOnlyList<HourlyForecastPrediction> predictions,
        int topHoursPerDay)
    {
        var paired = inputs.Zip(predictions, (inp, pred) => (inp, pred))
            .GroupBy(x => DateOnly.FromDateTime(x.pred.LocalTime.LocalDateTime))
            .OrderBy(g => g.Key);

        var dailySummaries = new List<DailyForecastSummary>();

        foreach (var dayGroup in paired)
        {
            var items = dayGroup.ToList();

            // Weight daytime & twilight hours higher than deep night so night hours
            // do not artificially drag down a day's outdoor photography score.
            double weightedSum = 0;
            double totalWeight = 0;
            foreach (var (inp, pred) in items)
            {
                double w = inp.SolarElevationDeg >= -6f ? 1.0 : 0.2;
                weightedSum += pred.PhotoScore * w;
                totalWeight += w;
            }
            float dailyPhotoScore = (float)(weightedSum / Math.Max(totalWeight, 1e-5));

            float avgDrone = items.Average(x => x.pred.DroneSafetyScore);

            // Dominant daytime condition (fallback to all hours if only night hours remain for today).
            var daytimeItems = items.Where(x => x.inp.SolarElevationDeg >= -6f).ToList();
            var condPool = daytimeItems.Count > 0 ? daytimeItems : items;
            PhotoWeatherCondition dominantCond = condPool
                .GroupBy(x => x.pred.Condition)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var bestHours = items
                .OrderByDescending(x => x.pred.PhotoScore)
                .Take(Math.Max(1, topHoursPerDay))
                .Select(x => x.pred)
                .OrderBy(p => p.LocalTime)
                .ToList();

            string dailyText = BuildDailySummaryText(dayGroup.Key, dailyPhotoScore, avgDrone, dominantCond, bestHours);

            dailySummaries.Add(new DailyForecastSummary(
                dayGroup.Key,
                dailyPhotoScore,
                avgDrone,
                dominantCond,
                bestHours,
                dailyText,
                items.Select(x => x.pred).ToList()));
        }

        return dailySummaries;
    }

    private static string BuildHourlyForecastText(
        HourlyNwsInput h,
        float photoScore,
        float droneScore,
        PhotoWeatherCondition condition)
    {
        string desc = condition switch
        {
            PhotoWeatherCondition.ClearCrisp => "Clear skies with crisp light",
            PhotoWeatherCondition.GoldenDramaticLight => "Prime golden-hour light and textured sky",
            PhotoWeatherCondition.PartlyCloudy => "Partly cloudy with balanced contrast",
            PhotoWeatherCondition.OvercastFlatLight => "Overcast sky with soft, flat diffused light",
            PhotoWeatherCondition.WindyGusty => "Breezy to gusty winds; use caution for drone flight",
            PhotoWeatherCondition.MistyFogRisk => "High humidity and narrow dew-point spread; mist or fog likely",
            PhotoWeatherCondition.RainShowers => "Likely rain or showers; protect camera gear",
            PhotoWeatherCondition.SevereStormy => "Stormy and gusty conditions; unsafe for outdoor/drone shoots",
            _ => "Variable conditions"
        };

        string droneTag = droneScore >= 75f ? "Drone OK" : (droneScore >= 45f ? "Drone Caution" : "No-Fly Wind/Wx");
        return $"{h.LocalTime:h:mm tt}: {desc}. {h.TemperatureC:F0}°C, {h.SkyCoverPct:F0}% clouds, wind {h.WindSpeedKmh:F0} km/h. Photo {photoScore:F0}/100 ({droneTag}).";
    }

    private static string BuildDailySummaryText(
        DateOnly date,
        float dailyScore,
        float avgDrone,
        PhotoWeatherCondition dominant,
        IReadOnlyList<HourlyForecastPrediction> bestHours)
    {
        string rating = dailyScore switch
        {
            >= 80f => "Excellent",
            >= 65f => "Good",
            >= 45f => "Fair",
            _ => "Poor"
        };

        string bestWindow = string.Join(", ", bestHours.Select(h => $"{h.LocalTime:h tt} ({h.PhotoScore:F0})"));
        string droneNote = avgDrone >= 75f ? "favorable for drone flight" : (avgDrone >= 45f ? "moderate wind/weather caution for drones" : "poor for drone flight");

        return $"{date:ddd, MMM d}: {rating} outdoor photography day ({dailyScore:F0}/100), {droneNote}. Dominant condition: {FormatCondition(dominant)}. Best hours: {bestWindow}.";
    }

    private static string FormatCondition(PhotoWeatherCondition c) => c switch
    {
        PhotoWeatherCondition.ClearCrisp => "Clear & Crisp",
        PhotoWeatherCondition.GoldenDramaticLight => "Golden / Dramatic Light",
        PhotoWeatherCondition.PartlyCloudy => "Partly Cloudy",
        PhotoWeatherCondition.OvercastFlatLight => "Overcast / Flat Light",
        PhotoWeatherCondition.WindyGusty => "Windy & Gusty",
        PhotoWeatherCondition.MistyFogRisk => "Misty / Fog Risk",
        PhotoWeatherCondition.RainShowers => "Rain / Showers",
        PhotoWeatherCondition.SevereStormy => "Severe / Stormy",
        _ => "Variable"
    };

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _session?.Dispose();
        _initLock.Dispose();
    }
}