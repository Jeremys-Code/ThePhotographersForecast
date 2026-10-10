using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastDetailsViewModel : ObservableObject
    {
        public string LocationName => SharedSession.SelectedLocation.DisplayName;
        [ObservableProperty]
        public partial NwsDailyForecast[] DailyForecasts { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast SelectedDailyForecast { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDailyHourlyForecasts { get; set; }


        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        [ObservableProperty]
        public partial Session SharedSession { get; set; }

        public ForecastDetailsViewModel(Session sharedSession)
        {

            SharedSession = sharedSession;
            // Use the Session collection directly so the ViewModel observes updates when the Session is populated
            if (SharedSession.NwsData.DailyForecasts == null || SharedSession.NwsData.DailyForecasts.Length == 0) return;
            MainThread.BeginInvokeOnMainThread(() => DailyForecasts = [.. SharedSession.NwsData.DailyForecasts]);
            SelectedDailyForecast = DailyForecasts[0];
            SelectedDailyHourlyForecasts = SelectedDailyForecast.HourlyWeathers;


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
