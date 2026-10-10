using System.Text;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.Modules;

namespace ThePhotographersForecast.Services.Narratives
{
    public static class DailyForecastSummary
    {
        public static string BuildDailyTimeOfDayForecasts(MeteorologisticAgent agent)
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

            // Compute mean cloud cover for each bin
            var means = bins.Select(b => CloudCoverEngine.Mean(b)).ToList();
            var categories = means.Select(m => CloudCoverEngine.Category(m)).ToList();

            // Identify key features
            int maxIndex = means.IndexOf(means.Max());
            int minIndex = means.IndexOf(means.Min());

            string peakPeriod = bins[maxIndex].Label;
            string peakCategory = categories[maxIndex];

            string clearPeriod = bins[minIndex].Label;
            string clearCategory = categories[minIndex];

            // Determine overall trend
            double overallDelta = means.Last() - means.First();
            string overallTrend = CloudCoverEngine.Trend(means.First(), means.Last());

            var sb = new StringBuilder();

            // Sentence 1 — overall trend
            sb.Append($"clouds are {overallTrend} through the day, ");

            // Sentence 2 — peak cloudiness
            sb.Append($"with {peakCategory} conditions in the {peakPeriod.ToLower()}. ");

            // Sentence 3 — clearing window
            sb.Append($"The clearest skies occur during the {clearPeriod.ToLower()}, ");

            //// Sentence 4 — sky experience
            //if (means[minIndex] <= 0.10)
            //    sb.Append("providing excellent Visibility.");
            //else if (means[minIndex] <= 0.25)
            //    sb.Append("offering good Visibility for sunrise and sunset.");
            //else
            //    sb.Append("with occasional breaks in the clouds.");

            return sb.ToString();
        }
    }
}
