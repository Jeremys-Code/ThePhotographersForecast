using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using ThePhotographersForecast.Data;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;
using Xunit;

namespace ThePhotographersForecast.Tests.Services;

public class GetNwsDataTests
{
    private static GetNwsData CreateSut(
        HttpMessageHandler handler,
        out Mock<ISession> sessionMock,
        out Mock<IForecastService> forecastServiceMock,
        out Mock<ISkyColorEngine> skyEngineMock)
    {
        var httpClient = new HttpClient(handler);

        sessionMock = new Mock<ISession>();
        sessionMock.SetupProperty(s => s.SelectedLocation, new PhotoLocation());
        sessionMock.SetupGet(s => s.StartDate).Returns(new DateTime(2026, 11, 13, 6, 0, 0));
        sessionMock.SetupGet(s => s.EndDate).Returns(new DateTime(2026, 11, 14, 18, 0, 0));

        forecastServiceMock = new Mock<IForecastService>();
        skyEngineMock = new Mock<ISkyColorEngine>();

        return new GetNwsData(
            httpClient,
            sessionMock.Object,
            forecastServiceMock.Object,
            skyEngineMock.Object);
    }

    #region GetWindDirection Tests

    [Theory]
    [InlineData("n", "north")]
    [InlineData("N", "north")]
    [InlineData("nne", "north-northeast")]
    [InlineData("ne", "Northeast")]
    [InlineData("NE", "Northeast")]
    [InlineData("ene", "east-northeast")]
    [InlineData("e", "east")]
    [InlineData("ese", "east-southeast")]
    [InlineData("s", "south")]
    [InlineData("ssw", "south-southwest")]
    [InlineData("sw", "southwest")]
    [InlineData("wsw", "west-southwest")]
    [InlineData("w", "west")]
    [InlineData("wnw", "west-northwest")]
    [InlineData("nw", "northwest")]
    [InlineData("nnw", "north-northwest")]
    [InlineData("sse", "south-southeast")]
    [InlineData("vrbl", "unknown")]
    [InlineData("", "unknown")]
    public void GetWindDirection_ReturnsExpectedDirection(string input, string expected)
    {
        string result = GetNwsData.GetWindDirection(input);

        Assert.Equal(expected, result);
    }

    #endregion

    #region CoverageStmt Tests

    [Theory]
    [InlineData("areas", "light", "rain_showers", "areas of  light rain showers")]
    [InlineData("slight_chance", "moderate", "snow_showers", "slight chance of  moderate snow showers")]
    [InlineData("likely", "heavy", "thunderstorms", "likely chance of  heavy thunderstorms")]
    [InlineData("", "light", "fog", " light fog")]
    [InlineData(null, "patchy", "freezing_rain", " patchy freezing rain")]
    [InlineData("unknown_coverage", "Light", "Drizzle", " light drizzle")]
    public void CoverageStmt_FormatsCoverageIntensityAndWeather(
        string? coverage,
        string intensity,
        string weather,
        string expected)
    {
        string result = GetNwsData.CoverageStmt(coverage!, intensity, weather);

        Assert.Equal(expected, result);
    }

    #endregion

    #region ParseNwsInterval Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseNwsInterval_NullOrWhitespace_ReturnsMinValueAndZeroDuration(string? interval)
    {
        var sut = CreateSut(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
            out _, out _, out _);

        var (time, duration) = sut.ParseNwsInterval(interval!);

        Assert.Equal(DateTime.MinValue, time);
        Assert.Equal(0, duration);
    }

    [Fact]
    public void ParseNwsInterval_ValidIsoIntervalWithHours_ReturnsLocalTimeAndDuration()
    {
        var sut = CreateSut(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
            out _, out _, out _);
        const string interval = "2026-11-13T12:00:00+00:00/PT6H";
        var expectedLocalTime = DateTimeOffset.Parse("2026-11-13T12:00:00+00:00").LocalDateTime;

        var (time, duration) = sut.ParseNwsInterval(interval);

        Assert.Equal(expectedLocalTime, time);
        Assert.Equal(6, duration);
    }

    [Fact]
    public void ParseNwsInterval_ValidIsoIntervalWithDays_ReturnsHoursDuration()
    {
        var sut = CreateSut(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
            out _, out _, out _);
        const string interval = "2026-11-13T00:00:00+00:00/P1D";

        var (_, duration) = sut.ParseNwsInterval(interval);

        Assert.Equal(24, duration);
    }

    [Fact]
    public void ParseNwsInterval_MissingDurationPart_ReturnsZeroDuration()
    {
        var sut = CreateSut(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
            out _, out _, out _);
        const string interval = "2026-11-13T12:00:00+00:00";
        var expectedLocalTime = DateTimeOffset.Parse("2026-11-13T12:00:00+00:00").LocalDateTime;

        var (time, duration) = sut.ParseNwsInterval(interval);

        Assert.Equal(expectedLocalTime, time);
        Assert.Equal(0, duration);
    }

    #endregion

    #region ObservableProperty Tests

    [Fact]
    public void ObservableProperties_RaisePropertyChanged()
    {
        var sut = CreateSut(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
            out _, out _, out _);
        var changedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        sut.DailyUrl = "https://api.weather.gov/gridpoints/BOX/71,90/forecast";
        sut.HourlyUrl = "https://api.weather.gov/gridpoints/BOX/71,90/forecast/hourly";
        sut.DetailUrl = "https://api.weather.gov/gridpoints/BOX/71,90";
        sut.NwsData = new NwsWeatherModel();

        Assert.Contains(nameof(GetNwsData.DailyUrl), changedProperties);
        Assert.Contains(nameof(GetNwsData.HourlyUrl), changedProperties);
        Assert.Contains(nameof(GetNwsData.DetailUrl), changedProperties);
        Assert.Contains(nameof(GetNwsData.NwsData), changedProperties);
    }

    #endregion

    #region GetWeatherAsync Tests

    [Fact]
    public async Task GetWeatherAsync_WhenPointsEndpointReturnsErrorStatus_ReturnsEmptyNwsWeatherModel()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var sut = CreateSut(handler, out _, out _, out _);

        var result = await sut.GetWeatherAsync(42.3601, -71.0589);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, sut.DailyUrl);
        Assert.Equal(string.Empty, sut.HourlyUrl);
        Assert.Equal(string.Empty, sut.DetailUrl);
    }

    [Fact]
    public async Task GetWeatherAsync_WhenHttpRequestThrows_ReturnsEmptyNwsWeatherModel()
    {
        var handler = new StubHttpMessageHandler(_ =>
            throw new HttpRequestException("Network unavailable"));
        var sut = CreateSut(handler, out _, out _, out _);

        var result = await sut.GetWeatherAsync(42.3601, -71.0589);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, sut.DailyUrl);
    }

    [Fact]
    public async Task GetWeatherAsync_RoundsCoordinatesToFiveDecimalPlacesInPointsRequest()
    {
        Uri? capturedUri = null;
        var handler = new StubHttpMessageHandler(req =>
        {
            capturedUri = req.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        var sut = CreateSut(handler, out _, out _, out _);

        await sut.GetWeatherAsync(42.36012345, -71.05891234);

        Assert.NotNull(capturedUri);
        Assert.Equal("https://api.weather.gov/points/42.36012,-71.05891", capturedUri!.ToString());
    }

    /// <summary>
    /// End-to-end orchestration test for <see cref="GetNwsData.GetWeatherAsync"/>.
    /// Note: <c>GetDataAsync</c> writes debug JSON snapshots using <c>FileSystem.Current.CacheDirectory</c>.
    /// When executed outside an initialized MAUI host, <c>FileSystem.Current.CacheDirectory</c> may throw
    /// <c>NotImplementedException</c>, which <c>GetDataAsync</c> catches and returns <c>null</c>.
    /// Run this test in a MAUI-initialized test host or after injecting an <c>IFileSystem</c> abstraction.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_ValidResponses_PopulatesNwsWeatherModel()
    {
        const string dailyUrl = "https://api.weather.gov/gridpoints/BOX/71,90/forecast";
        const string hourlyUrl = "https://api.weather.gov/gridpoints/BOX/71,90/forecast/hourly";
        const string detailUrl = "https://api.weather.gov/gridpoints/BOX/71,90";

        var pointsJson = JsonSerializer.Serialize(new
        {
            properties = new
            {
                forecast = dailyUrl,
                forecastHourly = hourlyUrl,
                forecastGridData = detailUrl,
                timeZone = "America/New_York",
                relativeLocation = new
                {
                    properties = new
                    {
                        city = "Boston",
                        state = "MA"
                    }
                }
            }
        });

        var dailyJson = JsonSerializer.Serialize(new
        {
            properties = new
            {
                elevation = new { value = 10.0 },
                periods = new[]
                {
                    new
                    {
                        number = 1,
                        name = "Today",
                        startTime = "2026-11-13T06:00:00-05:00",
                        endTime = "2026-11-13T18:00:00-05:00",
                        isDaytime = true,
                        temperature = new { value = 52.0 },
                        probabilityOfPrecipitation = new { value = 10.0, unitCode = "wmoUnit:percent" },
                        windSpeed = new { value = 16.09344, minValue = 8.04672, maxValue = 16.09344 },
                        windDirection = "NW",
                        windGust = new { value = 24.14016 },
                        icon = "https://api.weather.gov/icons/land/day/sct?size=medium",
                        shortForecast = "Mostly Sunny",
                        detailedForecast = "Mostly sunny, with a high near 52."
                    }
                }
            }
        });

        var hourlyJson = JsonSerializer.Serialize(new
        {
            properties = new
            {
                periods = new[]
                {
                    new
                    {
                        number = 1,
                        startTime = "2026-11-13T09:00:00-05:00",
                        endTime = "2026-11-13T10:00:00-05:00",
                        isDaytime = true,
                        temperature = new { value = 48.0 },
                        probabilityOfPrecipitation = new { value = 5.0 },
                        dewpoint = new { value = 32.0 },
                        relativeHumidity = new { value = 55.0 },
                        windSpeed = new { value = 10.0 },
                        windDirection = "NW",
                        windGust = new { value = 15.0 },
                        icon = "https://api.weather.gov/icons/land/day/sct?size=medium",
                        shortForecast = "Mostly Sunny"
                    },
                    new
                    {
                        number = 2,
                        startTime = "2026-11-13T21:00:00-05:00",
                        endTime = "2026-11-13T22:00:00-05:00",
                        isDaytime = false,
                        temperature = new { value = 38.0 },
                        probabilityOfPrecipitation = new { value = 0.0 },
                        dewpoint = new { value = 28.0 },
                        relativeHumidity = new { value = 65.0 },
                        windSpeed = new { value = 6.0 },
                        windDirection = "N",
                        windGust = (object?)null,
                        icon = "https://api.weather.gov/icons/land/night/few?size=medium",
                        shortForecast = "Mostly Clear"
                    }
                }
            }
        });

        var detailJson = JsonSerializer.Serialize(new
        {
            properties = new
            {
                validTimes = "2026-11-13T06:00:00+00:00/PT12H",
                weather = new { values = Array.Empty<object>() },
                apparentTemperature = new
                {
                    uom = "wmoUnit:degC",
                    values = new[]
                    {
                        new { validTime = "2026-11-13T14:00:00+00:00/PT1H", value = 8.0 }
                    }
                },
                skyCover = new
                {
                    uom = "wmoUnit:percent",
                    values = new[]
                    {
                        new { validTime = "2026-11-13T14:00:00+00:00/PT1H", value = 25.0 }
                    }
                }
            }
        });

        var handler = new StubHttpMessageHandler(req =>
        {
            string url = req.RequestUri!.ToString();
            string payload = url switch
            {
                var u when u.StartsWith("https://api.weather.gov/points/") => pointsJson,
                var u when u.StartsWith(dailyUrl) => dailyJson,
                var u when u.StartsWith(hourlyUrl) => hourlyJson,
                var u when u == detailUrl => detailJson,
                _ => throw new InvalidOperationException($"Unexpected URL: {url}")
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        });

        var sut = CreateSut(handler, out var sessionMock, out _, out _);

        var result = await sut.GetWeatherAsync(42.3601, -71.0589);

        Assert.NotNull(result);
        Assert.Equal(dailyUrl, sut.DailyUrl);
        Assert.Equal(hourlyUrl, sut.HourlyUrl);
        Assert.Equal(detailUrl, sut.DetailUrl);
        Assert.Equal("Boston", result.SelectedLocation.City);
        Assert.Equal("MA", result.SelectedLocation.State);
        Assert.Single(result.DailyForecasts);
        Assert.Equal(2, result.HourlyForecasts.Length);
        Assert.NotEmpty(result.HourlySupplementals);
        Assert.True(sessionMock.Object.SelectedLocation.Elevation.Elevation > 0);
    }

    #endregion

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_responder(request));
        }
    }
}