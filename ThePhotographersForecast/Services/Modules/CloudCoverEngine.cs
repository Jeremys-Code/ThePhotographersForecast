using System;
using System.Collections.Generic;
using System.Text;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Modules
{
    public static class CloudCoverEngine
    {
        // Map mean cloud fraction (0–1) to professional categories
        public static string Category(double fraction)
        {
            if (fraction <= 0.10) return "clear";
            if (fraction <= 0.25) return "few clouds";
            if (fraction <= 0.50) return "scattered clouds";
            if (fraction <= 0.75) return "mostly cloudy";
            return "overcast";
        }

        // Basic stats on a bin
        public static double Mean(Bin bin) =>
            bin.Values.Any() ? bin.Values.Average() : 0;

        public static double Min(Bin bin) =>
            bin.Values.Any() ? bin.Values.Min() : 0;

        public static double Max(Bin bin) =>
            bin.Values.Any() ? bin.Values.Max() : 0;

        // Trend between two bins
        public static string Trend(double previousMean, double currentMean)
        {
            double delta = currentMean - previousMean;

            if (delta >= 0.30) return "rapidly increasing";
            if (delta >= 0.15) return "increasing";
            if (delta <= -0.30) return "rapidly decreasing";
            if (delta <= -0.15) return "decreasing";
            return "remaining fairly steady";
        }
    }

}
