using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ThePhotographersForecast.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public async Task<T?> ExecuteJsonOperation<T>(
            string operationName,
            Func<Task<T?>> func)
        {
            var correlationId = Guid.NewGuid().ToString();
            var sw = Stopwatch.StartNew();

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Operation"] = operationName
            }))
            {
                try
                {
                    _logger.LogInformation("Starting JSON operation");

                    var result = await func();

                    sw.Stop();

                    if (result == null)
                    {
                        _logger.LogWarning("JSON operation returned null");
                    }
                    else
                    {
                        _logger.LogInformation("JSON operation succeeded in {Elapsed}ms", sw.ElapsedMilliseconds);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "JSON operation failed after {Elapsed}ms", sw.ElapsedMilliseconds);
                    throw;
                }
            }
        }
    }
}
