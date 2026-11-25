using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class DailyForecastViewModel : ObservableObject
    {
        private ISession _session;

        [ObservableProperty]
        public partial CalendarDayModel[] RunDates { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel SelectedRunDate { get; set; }
        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast[] Dailies { get; set; }
        //TODO: Pull the Health and Safety data from the DailyForecastSummaries
        [ObservableProperty]
        public partial DailyForecast[]  GfsDailies { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast SelectedDaily { get; set; }
        [ObservableProperty]
        public partial DailyForecast SelectedGfsDaily { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlies { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedDailySelectedHourly { get; set; }
        [ObservableProperty]
        public partial HourlyForecast[] SelectedGfsDailyHourlies { get; set; }
        [ObservableProperty]
        public partial HourlyForecast SelectedGfsDailySelectedHourly { get; set; }
        [ObservableProperty]
        public partial EventModel[] EventModels { get; set; }
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
            if (_session.Dates == null || _session.WeatherForecasts == null || _session.NwsData == null || _session.EventsData == null) return;
            //Collections
            RunDates = _session.Dates.ToArray();
            Dailies = _session.NwsData.DailyForecasts.ToArray();
            GfsDailies = _session.WeatherForecasts.DailyForecasts.ToArray();
            EventModels = _session.EventsData.ToArray();

            SelectedRunDate = RunDates.First();
            SelectedDate = SelectedRunDate.Date;
            
            //Collections or Objects qualified by the SelectedDate
            SelectedDaily = Dailies.Where(d => d.StartTime.Date == SelectedDate.Date && d.IsDaytime==true).FirstOrDefault(new NwsDailyForecast());
            SelectedDailyHourlies = SelectedDaily.HourlyWeathers?.ToArray() ?? Array.Empty<NwsHourlyForecast>();
            SelectedDailySelectedHourly = SelectedDailyHourlies.FirstOrDefault(new NwsHourlyForecast());
            SelectedGfsDaily = GfsDailies.Where(x => x.Date == DateOnly.FromDateTime(SelectedDate)).FirstOrDefault(new DailyForecast());
            SelectedGfsDailyHourlies = SelectedGfsDaily.HourlyForecasts.ToArray();
            SelectedGfsDailySelectedHourly = SelectedGfsDailyHourlies.FirstOrDefault(new HourlyForecast());
            SelectedDailyEventModel = EventModels.Where(x => x.Date.Date == SelectedDate.Date).FirstOrDefault(new EventModel());
            SelectedDailyEvents = SelectedDailyEventModel.EventTimes.ToArray();
            SelectedDailyEventTime = SelectedDailyEvents.FirstOrDefault(new EventTime());
            SelectedDailyEventTimeForecast = SelectedDailyEventTime.Forecast ?? new NwsHourlyForecast();
        }

        partial void OnSelectedRunDateChanged(CalendarDayModel value)
        {
            SelectedDate = value.Date;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await SelectedDateUpdateAsync();
            });
        }

        private async Task SelectedDateUpdateAsync()
        {
            SelectedDaily = Dailies.Where(d => d.StartTime.Date == SelectedDate.Date && d.IsDaytime == true).FirstOrDefault(new NwsDailyForecast());
            SelectedDailyHourlies = SelectedDaily.HourlyWeathers?.ToArray() ?? Array.Empty<NwsHourlyForecast>();
            SelectedDailySelectedHourly = SelectedDailyHourlies.FirstOrDefault(new NwsHourlyForecast());
            SelectedGfsDaily = GfsDailies.Where(x => x.Date == DateOnly.FromDateTime(SelectedDate)).FirstOrDefault(new DailyForecast());
            SelectedGfsDailyHourlies = SelectedGfsDaily.HourlyForecasts.ToArray();
            SelectedGfsDailySelectedHourly = SelectedGfsDailyHourlies.FirstOrDefault(new HourlyForecast());
            SelectedDailyEventModel = EventModels.Where(x => x.Date.Date == SelectedDate.Date).FirstOrDefault(new EventModel());
            SelectedDailyEvents = SelectedDailyEventModel.EventTimes.ToArray();
            SelectedDailyEventTime = SelectedDailyEvents.FirstOrDefault(new EventTime());
            SelectedDailyEventTimeForecast = SelectedDailyEventTime.Forecast ?? new NwsHourlyForecast();
            return;
        }
    }
}
