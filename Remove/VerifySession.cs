using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Shared
{
    public class VerifySession
    {
        private readonly ISession _session;
        public VerifySession(ISession session)
        {
            _session = session;
            _session.WeatherForecasts;
            -DailyForecast
            - HourlyForecast


            _session.NwsData
                - NwsWeatherModel
                - NwsDailyForecast
                - NwsHourlyForecast
                - NwsHourlySupplemental
            _session.EventsData
                - Astronomical
                - EventTime
            _session.Dates
                - DateItem
            _session.MoonPhases
                - FutureMoonPhase
            _session.Days

            // Iterate through sessions and write in the debug log as items are added to each collection.
              _session.WeatherForecasts =

            // Use the session collection directly so the ViewModel observes updates when the session is populated
            IEnumerable < T > = IEnumerable < T > ?? new ObservableCollection<T>();
            // When the session collection is updated later, refresh the selected date data on the UI thread
            IEnumerable<T>().CollectionChanged += (s, e) => MainThread.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine($"{sender.GetType().Name}.CollectionChanged invoked on thread {Environment.CurrentManagedThreadId}; count={WeatherForecasts.Count}");
                UpdateForDate(SelectedDate);
            });
        }
    }
}
}
