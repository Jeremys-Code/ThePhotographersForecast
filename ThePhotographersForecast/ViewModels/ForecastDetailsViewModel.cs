using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.ML;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastDetailsViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        public partial string LocationName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial NwsDailyForecast[] DailyForecasts { get; set; } = Array.Empty<NwsDailyForecast>();

        [ObservableProperty]
        public partial NwsDailyForecast SelectedDailyForecast { get; set; } = new NwsDailyForecast();

        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlyForecasts { get; set; } = Array.Empty<NwsHourlyForecast>();

        [ObservableProperty]
        public partial NwsHourlyForecast? SelectedHourlyForecast { get; set; }

        [ObservableProperty]
        public partial EventModel[] EventModels { get; set; } = Array.Empty<EventModel>();

        [ObservableProperty]
        public partial EventModel SelectedEventModel { get; set; } = new EventModel();

        [ObservableProperty]
        public partial FutureMoonPhase[] MoonPhases { get; set; } = Array.Empty<FutureMoonPhase>();

        [ObservableProperty]
        public partial FutureMoonPhase? SelectedMoonPhase { get; set; }

        [ObservableProperty]
        public partial int SelectedDailyIndex { get; set; }

        // EventsAndPredictions panel properties
        [ObservableProperty]
        public partial string SelectedHourHeader { get; set; } = "Select an hourly forecast to see why it's rated & its astronomical events.";

        [ObservableProperty]
        public partial string SelectedHourPredictionReason { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<EventTime> SelectedHourEvents { get; set; } = new ObservableCollection<EventTime>();

        [ObservableProperty]
        public partial bool HasSelectedHourEvents { get; set; }

        // Event to notify the View (code-behind) to scroll to the target index
        public event Action<int>? ScrollToIndexRequested;

        private readonly ISession _session;

        public ForecastDetailsViewModel(ISession session)
        {
            _session = session;
            RefreshFromSession();
        }

        public void RefreshFromSession()
        {
            var dailyArray = _session.NwsData?.DailyForecasts?.ToArray() ?? Array.Empty<NwsDailyForecast>();
            EventModels = _session.EventsData?.ToArray() ?? Array.Empty<EventModel>();
            LocationName = _session.NwsData?.SelectedLocation?.DisplayName ?? string.Empty;

            if (dailyArray.Length == 0)
                return;

            if (DailyForecasts == null || DailyForecasts.Length != dailyArray.Length || !ReferenceEquals(DailyForecasts[0], dailyArray[0]))
            {
                LoadFullForecast(0);
            }
            else
            {
                AnnotateBestHours(SelectedDailyHourlyForecasts);
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("dailyIndex", out var rawValue) &&
                int.TryParse(rawValue?.ToString(), out int index))
            {
                LoadFullForecast(index);
            }
        }

        private void LoadFullForecast(int index)
        {
            NwsDailyForecast[] dailyArray = _session.NwsData?.DailyForecasts?.ToArray() ?? Array.Empty<NwsDailyForecast>();
            if (dailyArray.Length == 0)
                return;

            if (DailyForecasts == null || DailyForecasts.Length != dailyArray.Length || !ReferenceEquals(DailyForecasts[0], dailyArray[0]))
            {
                DailyForecasts = dailyArray;
            }

            EventModels = _session.EventsData?.ToArray() ?? Array.Empty<EventModel>();
            LocationName = _session.NwsData?.SelectedLocation?.DisplayName ?? string.Empty;

            // Clamp index safely, update SelectedDailyForecast, and notify the UI to jump to it
            SelectedDailyIndex = Math.Clamp(index, 0, DailyForecasts.Length - 1);
            SelectedDailyForecast = DailyForecasts[SelectedDailyIndex];

            var hourlies = SelectedDailyForecast.HourlyWeathers?.ToArray() ?? Array.Empty<NwsHourlyForecast>();
            AnnotateBestHours(hourlies);
            SelectedDailyHourlyForecasts = hourlies;

            SelectedEventModel = EventModels.FirstOrDefault(x => x.Date.Date == SelectedDailyForecast.StartTime.Date) ?? new EventModel();

            var firstBest = hourlies.FirstOrDefault(h => h.IsBestPhotoOrDroneHour) ?? hourlies.FirstOrDefault();
            if (firstBest != null)
            {
                SelectHourlyForecast(firstBest);
            }

            ScrollToIndexRequested?.Invoke(SelectedDailyIndex);
        }

        partial void OnSelectedDailyForecastChanged(NwsDailyForecast value)
        {
            if (value == null)
            {
                SelectedDailyHourlyForecasts = Array.Empty<NwsHourlyForecast>();
                return;
            }

            var hourlies = value.HourlyWeathers?.ToArray() ?? Array.Empty<NwsHourlyForecast>();
            AnnotateBestHours(hourlies);
            SelectedDailyHourlyForecasts = hourlies;

            var matchingEvents = EventModels?.FirstOrDefault(x => x.Date.Date == value.StartTime.Date);
            if (matchingEvents != null)
            {
                SelectedEventModel = matchingEvents;
            }
        }

        [RelayCommand]
        public void SelectHourlyForecast(NwsHourlyForecast? hour)
        {
            if (hour == null) return;
            SelectedHourlyForecast = hour;

            SelectedHourHeader = $"{hour.StartTime:dddd, MMM d @ h:mm tt}" +
                (string.IsNullOrEmpty(hour.BestHourBadgeText) ? string.Empty : $"  ({hour.BestHourBadgeText})");

            SelectedHourPredictionReason = !string.IsNullOrWhiteSpace(hour.BestHourReason)
                ? hour.BestHourReason
                : "No AI prediction details available for this hour.";

            var dayEventModel = EventModels?.FirstOrDefault(x => x.Date.Date == hour.StartTime.Date);
            var matchingHourEvents = dayEventModel?.EventTimes?
                .Where(ev => ev.WeatherHour == hour.StartTime ||
                             (ev.AstroTime.Date == hour.StartTime.Date && ev.AstroTime.Hour == hour.StartTime.Hour) ||
                             (ev.OtherTime.HasValue && ev.AstroTime <= hour.EndTime && ev.OtherTime.Value >= hour.StartTime))
                .ToList() ?? new List<EventTime>();

            SelectedHourEvents = new ObservableCollection<EventTime>(matchingHourEvents);
            HasSelectedHourEvents = SelectedHourEvents.Count > 0;
        }

        partial void OnSelectedHourlyForecastChanged(NwsHourlyForecast? value)
        {
            if (value != null && SelectedHourHeader != $"{value.StartTime:dddd, MMM d @ h:mm tt}" +
                (string.IsNullOrEmpty(value.BestHourBadgeText) ? string.Empty : $"  ({value.BestHourBadgeText})"))
            {
                SelectHourlyForecast(value);
            }
        }

        private void AnnotateBestHours(NwsHourlyForecast[] hours)
        {
            if (hours == null || hours.Length == 0) return;

            var summaries = _session.ScoreSummaries ?? _session.DailyPhotoScoringSummary;
            var allPreds = summaries?
                .SelectMany(s => s.HourlyPredictions ?? Array.Empty<HourlyForecastPrediction>())
                .ToList() ?? new List<HourlyForecastPrediction>();

            var bestPhotoTimes = new HashSet<DateTime>();
            var bestDroneTimes = new HashSet<DateTime>();

            if (summaries != null)
            {
                foreach (var daySum in summaries)
                {
                    if (daySum.BestHours != null)
                    {
                        foreach (var bh in daySum.BestHours)
                            bestPhotoTimes.Add(TruncateToHour(bh.Timestamp.UtcDateTime));
                    }

                    if (daySum.HourlyPredictions != null && daySum.HourlyPredictions.Count > 0)
                    {
                        foreach (var dh in daySum.HourlyPredictions
                            .OrderByDescending(p => p.DroneFlightScore)
                            .Take(3)
                            .Where(p => p.DroneFlightScore >= 70f))
                        {
                            bestDroneTimes.Add(TruncateToHour(dh.Timestamp.UtcDateTime));
                        }
                    }
                }
            }

            foreach (var hour in hours)
            {
                var hourKey = TruncateToHour(hour.StartTime.UtcDateTime);
                bool isBestPhoto = bestPhotoTimes.Contains(hourKey);
                bool isBestDrone = bestDroneTimes.Contains(hourKey);

                hour.IsBestPhotoOrDroneHour = isBestPhoto || isBestDrone;
                hour.BestHourBadgeText = (isBestPhoto, isBestDrone) switch
                {
                    (true, true) => "★ Best Photo & Drone",
                    (true, false) => "★ Best Photo Hour",
                    (false, true) => "★ Best Drone Hour",
                    _ => string.Empty
                };

                var pred = allPreds.FirstOrDefault(p => TruncateToHour(p.Timestamp.UtcDateTime) == hourKey);
                if (pred != null)
                {
                    var reasons = new List<string>();
                    if (isBestPhoto) reasons.Add("Top-rated photo lighting/sky score for the day");
                    if (isBestDrone) reasons.Add("Optimal low-wind & safe visibility window for drone flight");
                    if (pred.BlueHourScore >= 60f) reasons.Add($"Strong Blue Hour lighting ({pred.BlueHourScore:F0}/100)");
                    if (pred.SunriseScore >= 60f) reasons.Add($"Prime Sunrise conditions ({pred.SunriseScore:F0}/100)");
                    if (pred.SunsetScore >= 60f) reasons.Add($"Prime Sunset conditions ({pred.SunsetScore:F0}/100)");
                    if (pred.LunarEventScore >= 60f) reasons.Add($"Favorable Lunar visibility ({pred.LunarEventScore:F0}/100)");
                    if (pred.GalaxyScore >= 60f) reasons.Add($"Clear dark sky for Galaxy/Astro ({pred.GalaxyScore:F0}/100)");

                    string whyPrefix = reasons.Count > 0
                        ? $"Why: {string.Join(" • ", reasons)}.\n"
                        : string.Empty;

                    hour.BestHourReason =
                        $"{whyPrefix}" +
                        $"Scores — Overall: {pred.OverallScore:F0}/100 | Drone: {pred.DroneFlightScore:F0}/100 | " +
                        $"Blue Hr: {pred.BlueHourScore:F0} | Sunrise: {pred.SunriseScore:F0} | Sunset: {pred.SunsetScore:F0} | " +
                        $"Lunar: {pred.LunarEventScore:F0} | Galaxy: {pred.GalaxyScore:F0}\n" +
                        $"{pred.HumanReadableForecast}";
                }
                else
                {
                    hour.BestHourReason = hour.DetailedForecast ?? hour.ShortForecast ?? string.Empty;
                }
            }
        }

        private static DateTime TruncateToHour(DateTime dt) =>
            new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
    }
}
