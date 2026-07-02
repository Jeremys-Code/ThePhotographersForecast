using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.ViewModels
{
    partial class DayViewModel : ObservableObject
    {
        [ObservableProperty]
        private DailyForecast dailyWeather;
        [ObservableProperty]
        private string dailyName;
        [ObservableProperty]
        private string timePeriod;
        [ObservableProperty]
        private string displayName;
        [ObservableProperty]
        private DateOnly date;
        [ObservableProperty]
        private bool _isToday;
        [ObservableProperty]
        private string dailyForecast;
        [ObservableProperty]
        private ImageSource dailyIcon;
        [ObservableProperty]
        private EventsViewModel[] dailyEvents;
    }
}