using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models
{
    public class EventModel
    {
        public ObservableCollection<EventTime> EventTimes
        {
            get;
            set;
        } = new ObservableCollection<EventTime>();
    }
    public class EventTime
    {
        public DateOnly Date { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = default;
        public DateTime EndTime { get; set; } = default;
        public DateTime WeatherHour { get; set; } = default;
        public DateTime OrderTime => StartTime != default ? StartTime : EndTime;
    }
}