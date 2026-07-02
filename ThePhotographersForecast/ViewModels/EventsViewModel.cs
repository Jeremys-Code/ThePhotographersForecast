using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.ViewModels
{
    public partial class EventsViewModel : ObservableObject
    {
        [ObservableProperty]
        private EventTime getEvent;
        public string timeOfDay => this.GetEvent.TimeOfDay;
        public string EventName => this.GetEvent.EventName;
        public DateTime EventTime => this.GetEvent.AstroTime;
        public DateTime? EndTime => this.GetEvent.OtherTime;

        [ObservableProperty]
        private HourlyForecast currentHourly;
        public string HourlyName => this.CurrentHourly.Name;
        public DateTime HourlyStartTime => this.CurrentHourly.Time;
        public string HourlyForecast => this.CurrentHourly.WeatherSummary;
        public ImageSource HourlyIcon => this.CurrentHourly.WeatherImage;
    }
}
