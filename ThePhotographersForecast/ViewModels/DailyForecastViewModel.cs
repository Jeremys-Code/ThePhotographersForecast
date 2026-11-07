using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class DailyForecastViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ISession _session { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel[] RunDates { get; set; }
        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; }

        public NwsDailyForecast[] Dailies
        {
            get => field;
            set
            {
                field = [.. _session.NwsData.DailyForecasts];
                OnPropertyChanged();
            }
        }
        //TODO: Pull the Health and Safety data from the DailyForecastSummaries
        [ObservableProperty]
        public partial NwsDailyForecast SelectedDaily { get; set; }
        [ObservableProperty]
        public partial DailyForecast SelectedGfsDaily { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlies { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedDailyHour { get; set; }
        [ObservableProperty]
        public partial HourlyForecast[] SelectedGfsDailyHourlies { get; set; }
        [ObservableProperty]
        public partial HourlyForecast SelectedGfsDailyHour { get; set; }
        [ObservableProperty]
        public partial EventModel SelectedDailyEventModel { get; set; }
        [ObservableProperty]
        public partial EventTime[] SelectedDailyEvents { get; set; }
        [ObservableProperty]
        public partial EventTime SelectedDailyEventTime { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedDailyEventTimeForecast { get; set; }

        public DailyForecastViewModel(ISession _session)
        {
            _session = _session;
            Initialize();
            if (Dailies != null || Dailies?.Length != _session.NwsData.DailyForecasts.Length) return;
            SelectedDate = _session.Dates[0].Date;
            SelectedDaily = Dailies[0];
            SelectedGfsDaily = _session.WeatherForecasts.DailyForecasts.Where(d => d.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();
            SelectedDailyEventModel = _session.EventsData.Where(d => d.Date == SelectedDate.Date).SingleOrDefault();
            SelectedDailyHourlies = [.. SelectedDaily.HourlyWeathers];
            SelectedDailyHour = SelectedDailyHourlies[0];

        }

        private async void Initialize()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                Dailies = _session.NwsData.DailyForecasts.ToArray();
            });

        }

    }
}
