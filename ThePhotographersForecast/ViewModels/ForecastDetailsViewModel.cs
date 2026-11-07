using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string LocationName { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast[] DailyForecasts { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast SelectedDailyForecast { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlyForecasts { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase[] MoonPhases { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase SelectedMoonPhase { get; set; }


        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ISession _session;

        public ForecastDetailsViewModel(ISession session)
        {

            _session = session;
            DailyForecasts = _session.NwsData.DailyForecasts.ToArray();
            //MoonPhases = _session.MoonPhases.ToArray();
            SelectedDailyForecast = DailyForecasts.FirstOrDefault();
            SelectedDailyHourlyForecasts = SelectedDailyForecast.HourlyWeathers.ToArray();
            //SelectedMoonPhase = MoonPhases.FirstOrDefault();
            LocationName = _session.NwsData.SelectedLocation.DisplayName;


        }

        // Automatically called by CommunityToolkit.Mvvm when SelectedDailyForecast changes
        partial void OnSelectedDailyForecastChanged(NwsDailyForecast Value)
        {
            if (Value?.HourlyWeathers != null)
            {
                SelectedDailyHourlyForecasts = Value.HourlyWeathers;
            }
            else
            {
                SelectedDailyHourlyForecasts = [];
            }
        }


    }
}
