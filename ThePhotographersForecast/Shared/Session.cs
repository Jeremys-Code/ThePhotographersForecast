using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Shared
{
    public partial class Session : ObservableObject, ISession
    {
        //private IServiceProvider _service;


        //Gfs weather model from open-meteo. Contains DailyForecasts, SelectedDailyHourlyForecasts, Grouped Event TImes, and moon phases.
        // private ObservableCollection<WeatherForecasts> _weatherForecasts = new();
        [ObservableProperty]
        public partial WeatherForecast WeatherForecasts { get; set; }
        [ObservableProperty]
        public partial EventModel[] EventsData { get; set; }
        [ObservableProperty]
        public partial EventTime[] EventTimes { get; set; }
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; }
        [ObservableProperty]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        public partial DateTime EndDate { get; set; }
        [ObservableProperty]
        public partial TimeZoneInfo SelectedTimeZone { get; set; }
        [ObservableProperty]
        public partial int SelectedTimeZoneOffset { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; }
        [ObservableProperty]
        public partial bool IsComplete { get; set; } = false;
        [ObservableProperty]
        public partial Dictionary<DateOnly, LinearGradientBrush> SkyColors { get; set; }
        [ObservableProperty]
        public partial Dictionary<string, ObservableCollection<EventTime>> EventsByPeriod { get; set; }
        [ObservableProperty]
        public partial LinearGradientBrush SkyGradient { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DayPeriod> Periods { get; set; }
        [ObservableProperty]
        public partial Microsoft.Maui.Controls.Maps.Polygon Coordinates { get; set; }
        [ObservableProperty]
        public partial NwsWeatherModel NwsData { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase[] MoonPhases { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DayViewModel> Days { get; set; }
        private void SetProperty<Type>(ref Type backingStore, Type Value)
        {
            if (Equals(backingStore, Value))
                return;

            backingStore = Value;
            OnPropertyChanged(nameof(Type));
        }
        partial void OnSelectedTimeZoneChanged(TimeZoneInfo Value)
        {
            if (Value == null || Value == default) return;

            this.SelectedLocation.TimeZoneName = Value.DisplayName;
        }
        partial void OnSelectedTimeZoneOffsetChanged(int Value)
        {
            if (Value == null || Value == default) return;

            var time = new TimeSpan(0, 0, Value);
            var sign = Value >= 0 ? "+" : "-";
            this.SelectedLocation.TimeZoneOffset = sign + time.Hours.ToString("HH:mm");
        }

        // Initialize the properties of the session state
        public Session()
        {
            //_service = service;
            WeatherForecasts = new WeatherForecast();
            EventsData = Array.Empty<EventModel>();
            EventTimes = Array.Empty<EventTime>();
            SelectedLocation = new PhotoLocation();
            StartDate = DateTime.UtcNow.Date;
            EndDate = DateTime.UtcNow.AddDays(6);
            Dates = Array.Empty<CalendarDayModel>();
            SkyColors = new Dictionary<DateOnly, LinearGradientBrush>();
            EventsByPeriod = new Dictionary<string, ObservableCollection<EventTime>>();
            NwsData = new NwsWeatherModel();
            Days = new ObservableCollection<DayViewModel>();
        }
        public async Task ResetSessionAsync()
        {

            return;
        }
    }
}
