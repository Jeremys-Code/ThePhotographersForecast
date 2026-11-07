using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Models.DayData;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class DataCheckViewModel : ObservableObject
    {
        private ISession _session;
        private readonly IDailyForecastEventsView _hourlyServices;

        [ObservableProperty]
        public partial MappedHourly[] HourlyCollection { get; set; }
        [ObservableProperty]
        public partial MappedHourly CurrentHourly { get; set; }
        [ObservableProperty]
        public partial DateTime[] HoursCollection { get; set; }
        [ObservableProperty]
        public partial DateTime CurrentHour { get; set; }

        public DataCheckViewModel(ISession session, IDailyForecastEventsView hourlyServices)
        {
            _session = session;
            _hourlyServices = hourlyServices;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                HourlyCollection = await _hourlyServices.BuildMappedHourlyAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                CurrentHourly = HourlyCollection.FirstOrDefault() ?? new MappedHourly();
            });
            int nbrOfHours = _session.EndDate.Subtract(_session.StartDate).Hours + 1;
            var currentDate = _session.StartDate;
            HoursCollection = new DateTime[nbrOfHours];
            for (int i = 0; i < nbrOfHours; i++)
            {
                currentDate = currentDate;
                HoursCollection[i] = currentDate;
                currentDate = currentDate.AddHours(1);
            }
            CurrentHour = HoursCollection[0];
        }

        partial void OnCurrentHourChanged(DateTime value)
        {

        }

    }
}