using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Views;

namespace ThePhotographersForecast.Models
{
    public partial class PhotoDayModel : ObservableObject
    {
        [ObservableProperty]
        private PhotoDay[] photoDays;
    }

    public partial class PhotoDay : ObservableObject
    {
        [ObservableProperty]
        private int day;
        [ObservableProperty]
        private DateTime date;

        [ObservableProperty]
        private bool _isToday;
        [ObservableProperty]
        private bool _isEmpty;
        [ObservableProperty]
        private bool _isEventDay;
        [ObservableProperty]
        private bool isEventWindow;
        [ObservableProperty]
        private bool _isFlipped;
        [ObservableProperty]
        private GfsDailyForecast[] dailyForecasts;
        [ObservableProperty]
        private GfsHourlyForecast[] hourlyForecasts;

        [ObservableProperty]
        private PhotoHour[] photoHours;
    }

    public partial class PhotoHour : ObservableObject
    {
        [ObservableProperty]
        private DateTime hour;
        [ObservableProperty]
        private string timeOfDay;
        [ObservableProperty]
        private Hourly hourly;
        [ObservableProperty]
        private ObservableCollection<EventTime> events = new ObservableCollection<EventTime>();
    }
}

