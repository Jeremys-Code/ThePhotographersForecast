using Microsoft.Maui.Controls.Maps;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Shared
{
    public interface ISession
    {
        Polygon Coordinates { get; set; }
        CalendarDayModel[] Dates { get; set; }
        ObservableCollection<DayViewModel> Days { get; set; }
        DateTime EndDate { get; set; }
        Dictionary<string, ObservableCollection<EventTime>> EventsByPeriod { get; set; }
        EventModel[] EventsData { get; set; }
        EventTime[] EventTimes { get; set; }
        bool IsComplete { get; set; }
        //FutureMoonPhase[] MoonPhases { get; set; }
        NwsWeatherModel NwsData { get; set; }
        PhotoLocation SelectedLocation { get; set; }
        TimeZoneInfo SelectedTimeZone { get; set; }
        int SelectedTimeZoneOffset { get; set; }
        Dictionary<DateOnly, LinearGradientBrush> SkyColors { get; set; }
        LinearGradientBrush SkyGradient { get; set; }
        DateTime StartDate { get; set; }
        WeatherForecast WeatherForecasts { get; set; }

        Task ResetSessionAsync();
    }
}