namespace ThePhotographersForecast.Services.Modules
{
    // Normalized hourly snapshot used for comparison and segmentation
    public record HourlySnapshot(
        DateTime Time,
        double Temperature,
        double? HeatIndex,
        double SkyCover,
        double PrecipitationAmt,
        string PrecipitationType,
        double WindSpeed,
        double Gusts,
        bool IsDay,
        IReadOnlyList<string> WeatherKeywords
    );

    public class Thresholds
    {
        public double TempDelta { get; init; } = 3.0; // degrees
        public double WindDelta { get; init; } = 5.0; // same units as WindSpeed
        public double GustDelta { get; init; } = 5.0;
        public double SkyCoverDelta { get; init; } = 15.0; // percent
        public double PrecipAmtDelta { get; init; } = 0.1; // small amount threshold
    }

    public class Segment
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public HourlySnapshot Representative { get; set; }
        public int Count { get; private set; } = 1;

        public Segment(HourlySnapshot snap)
        {
            Start = snap.Time;
            End = snap.Time.AddHours(1);
            Representative = snap;
        }

        public void Extend(HourlySnapshot snap)
        {
            End = snap.Time.AddHours(1);
            Count++;
            // update representative by averaging numeric fields conservatively
            Representative = Representative with
            {
                Temperature = (Representative.Temperature * (Count - 1) + snap.Temperature) / Count,
                HeatIndex = AverageNullable(Representative.HeatIndex, snap.HeatIndex, Count),
                SkyCover = (Representative.SkyCover * (Count - 1) + snap.SkyCover) / Count,
                PrecipitationAmt = (Representative.PrecipitationAmt * (Count - 1) + snap.PrecipitationAmt) / Count,
                WindSpeed = (Representative.WindSpeed * (Count - 1) + snap.WindSpeed) / Count,
                Gusts = (Representative.Gusts * (Count - 1) + snap.Gusts) / Count,
                IsDay = Representative.IsDay || snap.IsDay,
                WeatherKeywords = Representative.WeatherKeywords.Union(snap.WeatherKeywords).ToArray()
            };
        }

        private static double? AverageNullable(double? a, double? b, int count)
        {
            if (a == null && b == null) return null;
            double sum = (a ?? 0) * (count - 1) + (b ?? 0);
            return sum / count;
        }
    }

    public static class ForecastSegmentation
    {
        // Map ValueEvolution to HourlySnapshot
        public static HourlySnapshot From(ValueEvolution v)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));

            // Temperature (use first available temperature reading)
            double temp = v.Temperature?.FirstOrDefault()?.Value ?? 0;
            double? heat = v.ApparentTemperature?.FirstOrDefault()?.Value;
            double sky = v.SkyCover?.FirstOrDefault()?.Value ?? 0;
            double precipAmt = v.PrecipitationAmt;
            string precipType = string.IsNullOrEmpty(v.PrecipitationType) ? "none" : v.PrecipitationType;

            double windSpeed = v.WindSpeed;
            //if (!string.IsNullOrWhiteSpace(v.WindSpeed))
            //{
            //    // attempt to parse numeric Value from string like "5 mph" or "5"
            //    var digits = new string(v.WindSpeed.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            //    if (double.TryParse(digits, out var w)) windSpeed = w

            //}

            double gusts = v.Gusts?.FirstOrDefault()?.Value ?? 0;

            var keywords = v.ShortForecast
                .Select(w => w?.Value ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            return new HourlySnapshot(v.Time, temp, heat, sky, precipAmt, precipType, windSpeed, gusts, v.IsDay, keywords);
        }

        public static bool AreEquivalent(HourlySnapshot a, HourlySnapshot b, Thresholds t)
        {
            if (a == null || b == null) return false;
            if (!string.Equals(a.PrecipitationType, b.PrecipitationType, StringComparison.OrdinalIgnoreCase))
                return false;
            if (Math.Abs(a.Temperature - b.Temperature) > t.TempDelta) return false;
            if (Math.Abs(a.WindSpeed - b.WindSpeed) > t.WindDelta) return false;
            if (Math.Abs(a.Gusts - b.Gusts) > t.GustDelta) return false;
            if (Math.Abs(a.SkyCover - b.SkyCover) > t.SkyCoverDelta) return false;
            if (Math.Abs(a.PrecipitationAmt - b.PrecipitationAmt) > t.PrecipAmtDelta) return false;
            // if weather keywords differ significantly, treat as change
            var aSet = new HashSet<string>(a.WeatherKeywords, StringComparer.OrdinalIgnoreCase);
            var bSet = new HashSet<string>(b.WeatherKeywords, StringComparer.OrdinalIgnoreCase);
            if (!aSet.SetEquals(bSet)) return false;
            return true;
        }

        // Build segments from ordered ValueEvolution sequence
        public static List<Segment> BuildSegments(IEnumerable<ValueEvolution> hours, Thresholds thresholds = null)
        {
            thresholds ??= new Thresholds();
            var ordered = hours.OrderBy(h => h.Time).ToArray();
            var segments = new List<Segment>();
            Segment current = null;

            foreach (var v in ordered)
            {
                var snap = From(v);
                if (current == null)
                {
                    current = new Segment(snap);
                    continue;
                }
                if (AreEquivalent(current.Representative, snap, thresholds))
                {
                    current.Extend(snap);
                }
                else
                {
                    segments.Add(current);
                    current = new Segment(snap);
                }
            }

            if (current != null) segments.Add(current);
            return segments;
        }

        // Merge very short segments into nearest neighbor when shorter than minHours
        public static List<Segment> SmoothSegments(List<Segment> segments, int minHours = 2)
        {
            if (segments == null) return new List<Segment>();
            var result = new List<Segment>();

            for (int i = 0; i < segments.Count; i++)
            {
                var s = segments[i];
                var durationHours = Math.Max(1, (int)Math.Round((s.End - s.Start).TotalHours));
                if (durationHours >= minHours)
                {
                    result.Add(s);
                    continue;
                }

                // merge with previous if exists and similar, otherwise with next
                if (result.Count > 0)
                {
                    var prev = result[^1];
                    prev.Extend(s.Representative);
                }
                else if (i + 1 < segments.Count)
                {
                    var next = segments[i + 1];
                    // extend next backwards by merging representative Values conservatively
                    next.Start = s.Start;
                    // merge numeric fields by a simple average weighted by counts
                    var total = next.Count + s.Count;
                    next.Representative = next.Representative with
                    {
                        Temperature = (next.Representative.Temperature * next.Count + s.Representative.Temperature * s.Count) / total,
                        SkyCover = (next.Representative.SkyCover * next.Count + s.Representative.SkyCover * s.Count) / total,
                        PrecipitationAmt = (next.Representative.PrecipitationAmt * next.Count + s.Representative.PrecipitationAmt * s.Count) / total,
                        WindSpeed = (next.Representative.WindSpeed * next.Count + s.Representative.WindSpeed * s.Count) / total,
                        Gusts = (next.Representative.Gusts * next.Count + s.Representative.Gusts * s.Count) / total,
                        WeatherKeywords = next.Representative.WeatherKeywords.Union(s.Representative.WeatherKeywords).ToArray()
                    };
                    // call Extend to increment count and adjust end-time once per source hour
                    for (int k = 0; k < s.Count; k++)
                        next.Extend(s.Representative);
                }
                else
                {
                    // last element and no previous: just add
                    result.Add(s);
                }
            }

            return result;
        }
    }
}
