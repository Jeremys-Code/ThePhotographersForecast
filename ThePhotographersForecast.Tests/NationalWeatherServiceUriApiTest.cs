using Microsoft.Extensions.Logging;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Tests
{
    public class NationalWeatherServiceUriApiTest
    {
        public static double Latitude = 42.32;
        public static double Longitude = -71.093;
        private readonly IHttpClientFactory client;
        public Session session = new Session();

        [Fact]
        public void TestNationWeatherServiceUrlAPI()
        {
            var logger = new -internal sealed class Logger : ILogger
+public sealed class Logger : ILogger
{
    ...
}
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ThePhotographersForecast.Tests")]
<ItemGroup>
  <InternalsVisibleTo Include="ThePhotographersForecast.Tests" />
</ItemGroup>();
            var logging = new LoggingService(logger);
            var imageCache = new ImageCacheService(client);
            var weatherReporter = new WeatherReporter(session, imageCache);
            var eventService = new GetEventService(session, client);
            var weatherService = new GetWeatherService(client, weatherReporter, eventService, session, imageCache);
            var forecastService = new ForecastService(weatherReporter, session, weatherService);
            var nwsService = new GetNwsData(client, session, logging, forecastService);
        }
    }
}
