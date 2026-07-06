using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.Modules;
using static ThePhotographersForecast.ViewModels.ForecastViewModel;

namespace ThePhotographersForecast.Shared
{
    public partial class Session : ObservableObject, ISession, IMauiInitializeService
    {
        private IServiceProvider _service;
        public void Initialize(IServiceProvider services)
        {
            _service = services;
        }

        //Weather through the NWS
        //[ObservableProperty]
        //public partial GfsWeatherModel GfsData { get; set; }
        //Subsets of weather information from the NWS
        //[ObservableProperty]
        //public partial DateTime[] Hours { get; set; }

        //[ObservableProperty]
        //public partial MappedDaily[] DailyCollection { get; set; }
        //[ObservableProperty]
        //public partial MappedHourly[] HourlyCollection { get; set; }

        //Gfs weather model from open-meteo. Contains DailyForecasts, HourlyForecasts, Grouped Event TImes, and moon phases.
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> WeatherForecasts { get; set; }

        //Holds all the EventTimes Grouped Collection, DailyAstronomicalData, and Moon stats. Each EventModel represents one Day,
        //this collection holds all the days and is already partitions by date when only one EventModel is retreived.
        [ObservableProperty]
        public partial ObservableCollection<EventModel> EventsData { get; set; }

        //EventTimes for a single day that is grouped by the date and Time Of Day string.
        [ObservableProperty]
        public partial ObservableCollection<EventTime> EventTimes { get; set; }

        //The PhotoLocation object that holds the coordinates, displayname, and timezone info
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; }

        //The first date that is displayed in the UI
        [ObservableProperty]
        public partial DateTime StartDate { get; set; }

        //The last date that is displayed in the UI
        [ObservableProperty]
        public partial DateTime EndDate { get; set; }

        //The collections of dates that are displayed in the UI 
        [ObservableProperty]
        public partial DateTime[] Dates { get; set; }

        //Indicator all the shared session state informaton has been collected
        [ObservableProperty]
        public partial bool IsComplete { get; set; } = false;

        //Determine the colors in the sky hourly
        [ObservableProperty]
        public partial ObservableCollection<SkyColor> SkyColors { get; set; }

        private Dictionary<TimeOfDay, List<EventTime>> _eventsByPeriod =
Enum.GetValues<TimeOfDay>().ToDictionary(p => p, p => new List<EventTime>());

        public Dictionary<TimeOfDay, List<EventTime>> EventsByPeriod
        {
            get => _eventsByPeriod;
            set => SetProperty(ref _eventsByPeriod, value);
        }

        private LinearGradientBrush _skyGradient;
        public LinearGradientBrush SkyGradient
        {
            get => _skyGradient;
            set => SetProperty(ref _skyGradient, value);
        }

        [ObservableProperty]
        public partial ObservableCollection<DayPeriod> Periods { get; set; }
        public Session()
        {
            // Resolve the session service from DI


            SelectedLocation = new PhotoLocation();
            Dates = Array.Empty<DateTime>();
            EventsData = new ObservableCollection<EventModel>();

            EventTimes = new ObservableCollection<EventTime>();
            WeatherForecasts = new ObservableCollection<WeatherForecast>();
            SkyColors = new ObservableCollection<SkyColor>();

            _skyGradient = new LinearGradientBrush();
            StartDate = DateTime.Now.Date;
            EndDate = DateTime.Now.AddDays(7).Date;

            Periods = new ObservableCollection<DayPeriod>();
            Periods.Add(new DayPeriod("Overnight"));
            Periods.Add(new DayPeriod("Early Morning"));
            Periods.Add(new DayPeriod("Morning"));
            Periods.Add(new DayPeriod("Afternoon"));
            Periods.Add(new DayPeriod("Evening"));
            Periods.Add(new DayPeriod("Night"));
        }
    }
}
