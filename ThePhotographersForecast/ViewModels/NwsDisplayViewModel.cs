using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class DailyForecastViewViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel SelectedCalendarDate { get; set; }
        [ObservableProperty]
        public partial NwsWeatherModel Data { get; set; }
        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast[] AllDailies { get; set; }

        [ObservableProperty]
        public partial NwsDailyForecast SelectedDaily { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlies { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedDailyHourly { get; set; }
        private ISession _session;
        public DailyForecastViewViewModel(ISession session)
        {
            _session = session;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Dates = [.. _session.Dates];
                AllDailies = [.. _session.NwsData.DailyForecasts];
            });
            SelectedCalendarDate = Dates[0];
            SelectedDate = SelectedCalendarDate.Date;
            //SelectedDailies = AllDailies.Where(d => d.StartTime.Time == SelectedDate.Time || d.EndTime.Time == SelectedDate.Time).OrderBy(o => o.StartTime).ToArray();
            SelectedDaily = AllDailies[0];
            SelectedDailyHourlies = [.. SelectedDaily.HourlyWeathers];
        }

        [RelayCommand]
        private void SelectedDailyForecastChanged(CalendarDayModel calendarDate)
        {
            SelectedDate = calendarDate.Date;
            SelectedDaily = AllDailies.Where(d => d.StartTime.Date == SelectedDate.Date).First();
            SelectedDailyHourlies = [.. SelectedDaily.HourlyWeathers];
        }
    }
}
