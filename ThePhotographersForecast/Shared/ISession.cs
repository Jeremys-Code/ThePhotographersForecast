using CommunityToolkit.Mvvm.Collections;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Shared
{
    public interface ISession
    {
        DateTime[] Dates { get; set; }
        DateTime EndDate { get; set; }
        Dictionary<TimeOfDay, List<EventTime>> EventsByPeriod { get; set; }
        ObservableCollection<EventModel> EventsData { get; set; }
        ObservableCollection<EventTime> EventTimes { get; set; }
        bool IsComplete { get; set; }
        PhotoLocation SelectedLocation { get; set; }
        ObservableCollection<SkyColor> SkyColors { get; set; }
        LinearGradientBrush SkyGradient { get; set; }
        DateTime StartDate { get; set; }
        ObservableCollection<WeatherForecast> WeatherForecasts { get; set; }
    }
}