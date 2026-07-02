using System;
using System.Collections.Generic;
using System.Text;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.Modules;

namespace ThePhotographersForecast.Services.Narratives
{
    public static class CloudCoverNarrative
    {
        public static string BuildDailyNarrative(MeteorologisticAgent agent)
        {
            var bins = new[]
            {
            agent.Overnight,
            agent.EarlyMorning,
            agent.Morning,
            agent.Afternoon,
            agent.Evening,
            agent.Night
        };

            var sb = new StringBuilder();
            sb.AppendLine($"Cloud cover evolution for {agent.BinDate:dddd, MMMM dd}:");

            for (int i = 0; i < bins.Length; i++)
            {
                var bin = bins[i];
                if (bin == null || !bin.Values.Any())
                    continue;

                double mean = CloudCoverEngine.Mean(bin);
                double min = CloudCoverEngine.Min(bin);
                double max = CloudCoverEngine.Max(bin);
                string category = CloudCoverEngine.Category(mean);

                // Base description for this period
                sb.Append($"{bin.Label}: skies are {category} on average ");

                sb.Append($"(cloud cover ranging from {min:P0} to {max:P0}).");

                // Trend vs previous period
                if (i > 0)
                {
                    var prev = bins[i - 1];
                    if (prev != null && prev.Values.Any())
                    {
                        double prevMean = CloudCoverEngine.Mean(prev);
                        string trend = CloudCoverEngine.Trend(prevMean, mean);

                        sb.Append($" Relative to {prev.Label.ToLower()}, cloud cover is {trend}.");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}
