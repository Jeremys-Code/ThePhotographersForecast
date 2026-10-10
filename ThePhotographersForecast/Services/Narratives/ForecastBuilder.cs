using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Narratives
{
    public static class ForecastBuilder
    {
        public static string BuildDailyNarrative(MeteorologisticAgent agent)
        {
            var bins = new[]
            {
            agent.EarlyHours,
            agent.EarlyMorning,
            agent.Morning,
            agent.Afternoon,
            agent.Evening,
            agent.Night
            };
            return string.Empty;
        }
    }
}
