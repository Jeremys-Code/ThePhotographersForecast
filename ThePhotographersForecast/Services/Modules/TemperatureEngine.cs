using System.Text;

using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services.Modules
{
    public static class TemperatureEngine
    {

        public string TempCategory(double temp, double dewPoint, double relativeHumidity, double wbgt)
        {
            var sb = new StringBuilder();
            sb.Append($"The temperature will be {NumberGroups.TempsToString(temp)} with a dew point {NumberGroups.DewPointToString(dewPoint)}. Expect ");
            // --- Extreme heat danger (WBGT-based) ---
            if (wbgt >= 90)
            {
                sb.Append("extreme dangerous heat this hour.");
                return sb.ToString();
            }
            if (wbgt >= 82)
            {
                sb.Append("dangerous heat this hour.");
                return sb.ToString();
            }
            if (wbgt >= 78)
            {
                sb.Append("very hot and humid this hour.");
                return sb.ToString();
            }
            // --- RelativeHumidity / dew point discomfort ---
            bool oppressiveHumidity = dewPoint >= 75 || relativeHumidity >= 85;
            bool veryHumid = dewPoint >= 70 || relativeHumidity >= 75;
            bool humid = dewPoint >= 65 || relativeHumidity >= 60;

            // --- Cold stress ---
            if (temp <= 20)
            {
                sb.Append("frigid conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 32)
            {
                sb.Append("freezing conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 40)
            {
                sb.Append(humid ? "raw and cold conditions this hour." : "cold conditions this hour.");
                return sb.ToString();
            }
            // --- Cool to mild ---
            if (temp <= 50)
            {
                sb.Append(humid ? "chilly and damp conditions this hour." : "cool conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 60)
            {
                sb.Append(humid ? "cool and humid conditions this hour." : "mildly cool conditions this hour.");
                return sb.ToString();
            }
            // --- Mild to warm ---
            if (temp <= 70)
            {
                sb.Append(humid ? "mild but humid conditions this hour." : "mild conditions this hour.");
                return sb.ToString();
            }
            if (temp <= 80)
            {
                if (oppressiveHumidity) sb.Append("warm and oppressive conditions this hour.");
                if (veryHumid) sb.Append("warm and very humid conditions this hour.");
                if (humid) sb.Append("warm and humid conditions this hour.");
                sb.Append("warm conditions this hour.");
                return sb.ToString();
            }

            // --- Hot categories ---
            if (temp <= 90)
            {
                if (oppressiveHumidity) sb.Append("hot and oppressive conditions this hour.");
                if (veryHumid) sb.Append("hot and very humid conditions this hour.");
                if (humid) sb.Append("hot and humid conditions this hour.");
                sb.Append("hot conditions this hour.");
                return sb.ToString();
            }

            // --- Extreme heat (temp-based) ---
            if (temp > 90)
            {
                if (oppressiveHumidity) sb.Append("extremely hot and oppressive conditions this hour.");
                if (veryHumid) sb.Append("extremely hot and very humid conditions this hour.");
                sb.Append("extremely hot conditions this hour.");
                return sb.ToString();
            }

            sb.Append("unknown");
            return sb.ToString();
        }


        public static double Mean(Bin bin) =>
            bin.Values.Any() ? bin.Values.Average() : 0;

        public static double Min(Bin bin) =>
            bin.Values.Any() ? bin.Values.Min() : 0;
        public static double Max(Bin bin) =>
            bin.Values.Any() ? bin.Values.Max() : 0;

        public static double StandardDeviation(Bin bin)
        {
            var m = Mean(bin);
            var count = bin.Values.Count;
            List<double> o = new List<double>();
            foreach (var item in bin.Values) { o.Add(Math.Pow(item - m, 2)); }
            ;
            var variance = o.Average();
            var stDev = Math.Sqrt(variance);
            return stDev;
        }
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
