using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class DataCheckViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Location { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast[] Dailies { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast[] SelectedDailies { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast SelectedDaily { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlies { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedDailyHourly { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast CurrentForecast { get; set; }
        [ObservableProperty]
        public partial NwsHourlySupplemental SelectedDailyHourlySupplemental { get; set; }
        [ObservableProperty]
        public partial DailyForecast[] MeteoDailies { get; set; }
        [ObservableProperty]
        public partial DailyForecast SelectedMeteoDaily { get; set; }
        [ObservableProperty]
        public partial HourlyForecast[] SelectedMeteoDailyHourlies { get; set; }
        [ObservableProperty]
        public partial HourlyForecast SelectedMeteoDailyHourly { get; set; }
        [ObservableProperty]
        public partial EventModel[] EventModels { get; set; }
        [ObservableProperty]
        public partial EventModel SelectedDailyEventModel { get; set; }
        [ObservableProperty]
        public partial EventTime[] SelectedDailyEventTimes { get; set; }
        [ObservableProperty]
        public partial DayViewModel[] Days { get; set; }
        [ObservableProperty]
        public partial DayViewModel SelectedDay { get; set; }

        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; }
        [ObservableProperty]
        public partial string StartDate { get; set; }
        [ObservableProperty]
        public partial string EndDate { get; set; }
        [ObservableProperty]
        public partial DateItem SelectedDateItem { get; set; }
        [ObservableProperty]
        public partial string TimePeriod { get; set; }
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; }
        public Session SharedSession { get; set; }
        private readonly IForecastService _forecastService;
        public DataCheckViewModel(Session sharedSession, IForecastService forecastService)
        {
            SharedSession = sharedSession;
            _forecastService = forecastService;
            MainThread.BeginInvokeOnMainThread(() =>
            {

                SelectedLocation = new PhotoLocation(SharedSession.SelectedLocation);
                Dates = SharedSession.Dates.ToArray();
                SelectedDate = Dates[0].Date;
                Dailies = SharedSession.NwsData.DailyForecasts.ToArray();
                SelectedDaily = Dailies[0];
                SelectedDailyHourlies = SelectedDaily.HourlyWeathers;
                SelectedDailyHourly = SelectedDailyHourlies[0];
                Days = SharedSession.Days.ToArray();
                MeteoDailies = SharedSession.WeatherForecast.DailyForecasts;
                EventModels = SharedSession.EventsData.ToArray();
                StartDate = SharedSession.StartDate.ToString("F");
                EndDate = SharedSession.EndDate.ToString("F");
                CurrentForecast = SharedSession.NwsData.HourlyForecasts[0];
                //if (Dailies != null && MeteoDailies != null)
                //{
                //    for (int e = 0; e < Dailies.Length; e++)
                //    {
                //        var meteoDaily = MeteoDailies.Where(x => DateOnly.FromDateTime(Dailies[e].StartTime) == x.Time).SingleOrDefault();
                //        var enrichedForecast = await _forecastService.BuildDailyForecastAsync(Dailies[e], meteoDaily);
                //        Dailies[e].EnrichedForecast = String.Join(" ", enrichedForecast);

                //    }
                //    ;
                //}
            });

            if (Dailies.Length == 0 || Dailies == null)
            {
                Dailies = Array.Empty<NwsDailyForecast>();
            }
            //SelectedDateItem = Dates[0];
            //SelectedDate = SelectedDateItem.Time;

            SelectedDailies = [.. Dailies.Where(w => w.StartTime.Date == SelectedDate || w.EndTime.Date == SelectedDate)];

            SelectedDailyHourlies = [.. SelectedDaily.HourlyWeathers];
            SelectedDailyHourly = SelectedDailyHourlies[0];
            if (SelectedDailyHourly.Supplemental != null)
                SelectedDailyHourlySupplemental = SelectedDailyHourly.Supplemental;


            SelectedMeteoDaily = MeteoDailies.Where(w => w.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();
            SelectedMeteoDailyHourlies = SelectedMeteoDaily.HourlyForecasts;
            SelectedMeteoDailyHourly = SelectedMeteoDailyHourlies[0];
            TimePeriod = SelectedMeteoDailyHourly.TimeOfDay;

            SelectedDailyEventModel = EventModels.Where(e => e.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();
            SelectedDailyEventTimes = SelectedDailyEventModel.EventTimes;
        }

        partial void OnSelectedDateItemChanged(DateItem Value)
        {
            SelectedDate = Value.Date;
            UpdateData(SelectedDate);
        }

        partial void OnSelectedDailyChanged(NwsDailyForecast nwsSelectedForecast)
        {
            SelectedDate = nwsSelectedForecast.StartTime.Date;
            UpdateData(SelectedDate);
        }

        partial void OnSelectedMeteoDailyChanged(DailyForecast selectedForecast)
        {
            SelectedDate = new DateTime(selectedForecast.Date.Year, selectedForecast.Date.Month, selectedForecast.Date.Day, 0, 0, 0);
            UpdateData(SelectedDate);
        }


        private async Task UpdateData(DateTime selectedDate)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                SelectedDate = selectedDate;
                SelectedDaily = Dailies.FirstOrDefault(x => x.StartTime.Date == SelectedDate);
                SelectedDailyHourlies = [.. SelectedDaily.HourlyWeathers];
                SelectedDailyHourly = SelectedDailyHourlies[0];
                SelectedDailyEventModel = EventModels.Where(e => e.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();
                SelectedDailyEventTimes = SelectedDailyEventModel.EventTimes;
                if (SelectedDailyHourly.Supplemental != null)
                    SelectedDailyHourlySupplemental = SelectedDailyHourly.Supplemental;
                SelectedDay = Days.Where(d => d.Date == selectedDate).SingleOrDefault();
                SelectedMeteoDaily = MeteoDailies.Where(w => w.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();
                SelectedMeteoDailyHourlies = SelectedMeteoDaily.HourlyForecasts;
                SelectedMeteoDailyHourly = SelectedMeteoDailyHourlies[0];
                TimePeriod = SelectedMeteoDailyHourly.TimeOfDay;

            });
        }
    }
}
