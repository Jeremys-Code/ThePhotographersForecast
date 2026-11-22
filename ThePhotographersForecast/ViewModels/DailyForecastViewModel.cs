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
                field = _session.NwsData.DailyForecasts.ToArray();
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

        public DailyForecastViewModel(ISession session)
        {
            _session = session;
            RefreshFromSession();
        }

        public void RefreshFromSession()
        {
            if (_session?.NwsData?.DailyForecasts == null || _session.NwsData.DailyForecasts.Length == 0)
                return;

            Dailies = _session.NwsData.DailyForecasts.ToArray();
            if (_session.Dates != null && _session.Dates.Length > 0)
            {
                SelectedDate = _session.Dates[0].Date;
            }
            SelectedDaily = Dailies[0];
            SelectedGfsDaily = _session.WeatherForecasts?.DailyForecasts?.FirstOrDefault(d => d.Date == DateOnly.FromDateTime(SelectedDate)) ?? new DailyForecast();
            SelectedDailyEventModel = _session.EventsData?.FirstOrDefault(d => d.Date == SelectedDate.Date) ?? new EventModel();
            SelectedDailyHourlies = SelectedDaily.HourlyWeathers?.ToArray() ?? Array.Empty<NwsHourlyForecast>();
            SelectedDailyHour = SelectedDailyHourlies.FirstOrDefault() ?? new NwsHourlyForecast();
        }

    }
}
