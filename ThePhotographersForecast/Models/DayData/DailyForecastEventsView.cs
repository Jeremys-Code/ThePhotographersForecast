using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Models.DayData
{
    public partial class DailyForecastEventsView : ObservableObject, IDailyForecastEventsView
    {
        private readonly ISession _session;
        private IForecastService _forecastService;
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel SelectedDate { get; set; }
        [ObservableProperty]
        public partial EventModel[] EventModels { get; set; }
        [ObservableProperty]
        public partial EventModel SelectEventModel { get; set; }
        [ObservableProperty]
        public partial EventTime[] SelectedEventModelEventTimes { get; set; }
        [ObservableProperty]
        public partial EventTime SelectedEventModelSelectedEventTime { get; set; }
        [ObservableProperty]
        public partial HourlyForecast SelectedEventTimeMeteoForecast { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedEventTimeNwsForecast { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast[] NwsDailies { get; set; }
        [ObservableProperty]
        public partial NwsDailyForecast SelectedNwsDaily { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedNwsDailyHourlies { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedNwsDailySelectedHourly { get; set; }
        [ObservableProperty]
        public partial DailyForecast[] MeteoDailies { get; set; }
        [ObservableProperty]
        public partial DailyForecast SelectedMeteoDaily { get; set; }
        [ObservableProperty]
        public partial HourlyForecast[] SelectedMeteoDailyHourlies { get; set; }
        [ObservableProperty]
        public partial HourlyForecast SelectedMeteoDailySelectedHourly { get; set; }
        public DailyForecastEventsView(ISession session, IForecastService forecastService)
        {
            _session = session;
            _forecastService = forecastService;

        }

        public async Task<MappedHourly[]> BuildMappedHourlyAsync()
        {
            var result = await BuildHourlyDataPoolAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
            if (result == null) return Array.Empty<MappedHourly>();
            var mapped = result.OrderBy(o => o.StartTime).ToArray();
            return mapped;
        }
        private async Task<List<MappedHourly>> BuildHourlyDataPoolAsync()
        {
            var nwsHours = _session.NwsData.HourlyForecasts.OrderBy(n => n.StartTime).ToArray();
            var nwsHourSupplementals = _session.NwsData.HourlySupplementals.OrderBy(s => s.Time).ToArray();
            var meteoHours = _session.WeatherForecasts.HourlyForecasts.OrderBy(w => w.HourlyTime).ToArray();
            var astroHours = _session.EventTimes.OrderBy(e => e.WeatherHour).ToArray();
            List<MappedHourly> combo = new List<MappedHourly>();
            int nbrOfHours = _session.EndDate.Subtract(_session.StartDate).Hours + 1;
            var currentDate = _session.StartDate;
            var hours = new DateTime[nbrOfHours];
            //for (int i = 0; i < nbrOfHours; i++)
            //{
            //    currentDate = currentDate;
            //    hours[i] = currentDate;
            //    currentDate = currentDate.AddHours(1);
            //}


            //for (int h = 0; h < hours.Length; h++)
            //{
            var results =
    from n in nwsHours
    from s in nwsHourSupplementals
    from m in meteoHours
    from a in astroHours
    where n.StartTime == s.Time &&
          n.StartTime == m.HourlyTime &&
          n.StartTime == a.WeatherHour
    select new MappedHourly
    {
        StartTime = n.StartTime,
        EndTime = n.EndTime,
        IsDay = n.IsDaytime,
        Temperature = new TemperatureValue { Value = n.Temperature.Value },
        IsDailyHigh = n.IsHigh,
        IsDailyLow = n.IsLow,
        SkHourlyAnimation = n.SkHourlyAnimation,
        HourlyForecast = n.DetailedForecast,
        Events = _session.EventTimes.Where(e => e.WeatherHour == n.StartTime).OrderBy(o => o.AstroTime).ToArray(),
        TemperatureStatement = _forecastService.TempCategory(n.Temperature.Value, n.DewPoint.Value, n.RelativeHumidity.Value, m.WetBulbTemperature2m.Value),
        SkyCoverStatement = s.Skycover != null && s.Skycover.Description != null ? s.Skycover.Description : m.CloudCover.Description,
        WindSpeedStatement = n.WindStatement != null ? n.WindStatement : m.WindSpeed10m.Description,
        BriefForecast = n.ShortForecast,
        PrecipitationStatement = (s.QuantitativePrecipitation != null && s.QuantitativePrecipitation.Value > 0) && (s.Weather != null && s.SupplementalWeatherStatement != null) ? $"{s.SupplementalWeatherStatement} totaling {s.QuantitativePrecipitation.Value}{s.QuantitativePrecipitation.UnitCode}." : "No precipitation expected.",
        StormStatement = s.Weather != null && s.SupplementalWeatherStatement != null ? s.SupplementalWeatherStatement : "No storms to report.",
        HealthSafetyStatement = m.HourlyForecastSummary.HealthSafetySummary
    };
            combo.AddRange(results);

            return combo;
        }

        public class NwsHeatriskDetail
        {
            public DateTime Date { get; set; }
            public int Heatrisk { get; set; }
            public string HeatriskDescription => GetHeatRiskDescription();
            public Color HeatRiskColor => GetHeatRiskColor();
            private Color GetHeatRiskColor()
            {
                switch (this.Heatrisk)
                {
                    case 0:
                        return Color.FromRgb(0, 255, 0);
                    case 1:
                        return Color.FromRgb(255, 255, 0);
                    case 2:
                        return Color.FromRgb(255, 165, 0);
                    case 3:
                        return Color.FromRgb(255, 0, 0);
                    case 4:
                        return Color.FromRgb(255, 0, 255);
                    default:
                        return Color.FromRgb(255, 255, 255);
                }

            }
            private string GetHeatRiskDescription()
            {
                switch (this.Heatrisk)
                {
                    case 0:
                        return "There is a category 0 (Green) ForecastHeatRisk. Little to no risk from expected heat.";
                    case 1:
                        return "There is a category 1 (Yellow) ForecastHeatRisk. Minor - Primarily affects those who are extremely sensitive to heat and without cooling/hydration.";
                    case 2:
                        return "There is a category 2 (Orange) ForecastHeatRisk. Moderate - Affects those who are sensitive to heat, especially those without cooling/hydration, and some health systems and industries.";
                    case 3:
                        return "There is a category 3 (Red) ForecastHeatRisk. Major - Affects anyone without cooling/hydration as well as health systems and industries.";
                    case 4:
                        return "There is a category 4 (Magenta/Purple) ForecastHeatRisk. Extreme - Rare and/or long-duration extreme heat with no overnight relief affecting anyone without cooling/hydration as well as health systems, industries, and infrastructure.";
                    default:
                        return "There is no ForecastHeatRisk category issued today.";
                }
            }
        }
    }
}
