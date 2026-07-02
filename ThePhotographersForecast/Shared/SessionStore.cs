using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Shared
{
    public partial class SessionStore : ObservableObject, ISessionStore
    {
        [ObservableProperty]
        public partial GfsWeatherModel GfsData { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> WeatherForecasts { get; set; } = new ObservableCollection<WeatherForecast>();
        [ObservableProperty]
        public partial ObservableCollection<EventModel> EventsData { get; set; } = new ObservableCollection<EventModel>();
        [ObservableProperty]
        public partial ObservableGroupedCollection<string, EventsViewModel> TimeOfDayGroupedEvents { get; set; } = new ObservableGroupedCollection<string, EventsViewModel>();
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();
        [ObservableProperty]
        public partial DateTime StartDateTime { get; set; }
        [ObservableProperty]
        public partial DateTime EndDateTime { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DateTime> DailyStartTimes { get; set; } = new ObservableCollection<DateTime>();
        // An array of hours that is used to organize the data by hour
        [ObservableProperty]
        public partial DateTime[] Hours { get; set; }

        [ObservableProperty]
        public partial MappedDaily[] DailyCollection { get; set; }
        [ObservableProperty]
        public partial MappedHourly[] HourlyCollection { get; set; }
        [ObservableProperty]
        public partial bool IsComplete { get; set; } = false;
        //public void ClearSession()
        //{

        //    this.DailyStartTimes = Array.Empty<DateTime>();

        //    this.EndDateTime = DateTime.MinValue;

        //    this.EventsData = new EventModel();
        //    this.GfsData = new GfsWeatherModel();

        //    this.Hours = Array.Empty<DateTime>();
        //    this.SelectedLocation = new PhotoLocation();
        //    this.StartDateTime = DateTime.MinValue;
        //    this.DailyCollection = Array.Empty<Daily>();
        //    this.HourlyCollection = Array.Empty<Hourly>();


        //}
    }
}

[ObservableProperty]
public partial ObservableCollection<WeatherForecast> WeatherForecasts { get; set; } = new ObservableCollection<WeatherForecast>();
[ObservableProperty]
public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();
[ObservableProperty]
public partial DateTime StartDateTime { get; set; }
[ObservableProperty]
public partial DateTime EndDateTime { get; set; }
[ObservableProperty]
public partial ObservableCollection<DateTime> DailyStartTimes { get; set; } = new ObservableCollection<DateTime>();