using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models
{
    public class Inference
    {
        public DateOnly Date { get; set; }
        public string DateString { get; set; } = string.Empty;
        public string DailySummary { get; set; } = string.Empty;
        public string DominantCondition { get; set; } = string.Empty;
        public string BlueHourScore { get; set; } = string.Empty;
        public string SurnriseScore { get; set; } = string.Empty;
        public string SunsetScore { get; set; } = string.Empty;
        public string MoonScore { get; set; } = string.Empty;
        public string GalaxyScore { get; set; } = string.Empty;
        public string DailyScore { get; set; } = string.Empty;
        public string BestHoursText { get; set; } = string.Empty;
        public ObservableCollection<(DateTime, string)> BestHours { get; set; } = new ObservableCollection<(DateTime, string)>();
        public string AvgDroneSafetyScore { get; set; } = string.Empty;
    }
}
