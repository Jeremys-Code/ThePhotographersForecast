using System.Collections.ObjectModel;

namespace PhotographersDay.Models
{
    public class ForecastEventModel
    {
        public DateOnly EventDate { get; set; }
        public GfsDailyForecast[] DailyForecast { get; set; } = Array.Empty<GfsDailyForecast>();

        public ObservableCollection<EventsModel> EventTimes { get; set; } = new ObservableCollection<EventsModel>();

    }
    public string GetTimesOfDay(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 3):
                    return "Night";
                case int h when (3 <= h && h < 5):
                    return "Early Morning";
                case int h when (5 <= h && h < 12):
                    return "Morning";
                case int h when (12 <= h && h < 17):
                    return "Afternoon";
                case int h when (17 <= h && h < 22):
                    return "Evening";
                case int h when (22 <= h && h < 24):
                    return "Night";
                default:
                    return string.Empty;
            }
        }
    }