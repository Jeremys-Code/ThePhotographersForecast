using CommunityToolkit.Mvvm.Collections;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Shared
{
    public interface ISessionStore
    {
        MappedDaily[] DailyCollection { get; set; }
        ObservableCollection<DateTime> DailyStartTimes { get; set; }
        DateTime EndDateTime { get; set; }
        ObservableGroupedCollection<string, EventsViewModel> TimeOfDayGroupedEvents { get; set; }
        ObservableCollection<EventModel> EventsData { get; set; }
        GfsWeatherModel GfsData { get; set; }
        MappedHourly[] HourlyCollection { get; set; }
        DateTime[] Hours { get; set; }
        bool IsComplete { get; set; }
        PhotoLocation SelectedLocation { get; set; }
        DateTime StartDateTime { get; set; }
        ObservableCollection<WeatherForecast> WeatherForecasts { get; set; }
        //void ClearSession();
    }
}