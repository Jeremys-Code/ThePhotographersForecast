using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class EventPlannerViewModel : ObservableObject
    {
        private readonly ISession _session;

        // Core event planning properties
        [ObservableProperty]
        private DayViewModel[] days;

        [ObservableProperty]
        private DayViewModel selectedDay;

        [ObservableProperty]
        private DateTime startDate;

        [ObservableProperty]
        private DateTime endDate;

        [ObservableProperty]
        private ObservableCollection<CalendarDayModel> dates;

        [ObservableProperty]
        private DateOnly selectedDate;

        // Location and instruction text
        [ObservableProperty]
        private string locationName;

        [ObservableProperty]
        private string elevationStatement;

        [ObservableProperty]
        private string instructions;

        // Astronomical event data
        [ObservableProperty]
        private EventModel selectedDayEventModel;

        [ObservableProperty]
        private EventTime[] selectedDayEventTimes;

        // Sky color and atmospheric information
        [ObservableProperty]
        private LinearGradientBrush selectedDaySkyGradient;

        [ObservableProperty]
        private string skyColorExplanation;

        // Day/Sun/Moon properties
        [ObservableProperty]
        private string dayLengthStatement;

        [ObservableProperty]
        private string sunriseStatement;

        [ObservableProperty]
        private string sunsetStatement;

        [ObservableProperty]
        private string sunshineDurationStatement;

        [ObservableProperty]
        private string moonPhaseStatement;

        [ObservableProperty]
        private FutureMoonPhase selectedDayMoonPhase;

        // Weather suitability context
        [ObservableProperty]
        private NwsHourlyForecast[] selectedDayHourlyForecasts;

        [ObservableProperty]
        private string weatherSuitabilityStatement;

        [ObservableProperty]
        private bool isWorking;

        public static DateTime MinDate => DateTime.Now.Date;
        public static DateTime MaxDate => DateTime.Now.AddDays(15).Date;

        [RelayCommand]
        private void PreviousDate()
        {
            if (Dates == null || Dates.Count == 0) return;
            var currentIndex = Dates.IndexOf(Dates.FirstOrDefault(d => d.Date == SelectedDate));
            if (currentIndex > 0)
                SelectedDate = Dates[currentIndex - 1].Date;
        }

        [RelayCommand]
        private void NextDate()
        {
            if (Dates == null || Dates.Count == 0) return;
            var currentIndex = Dates.IndexOf(Dates.FirstOrDefault(d => d.Date == SelectedDate));
            if (currentIndex < Dates.Count - 1)
                SelectedDate = Dates[currentIndex + 1].Date;
        }

        public EventPlannerViewModel(ISession session)
        {
            _session = session;

            Days = _session.Days.ToArray();
            StartDate = _session.StartDate;
            EndDate = _session.EndDate;
            Dates = _session.Dates.ToObservableCollection();
            SelectedDate = Dates.FirstOrDefault()?.Date ?? DateOnly.FromDateTime(DateTime.Now.Date);
            LocationName = _session.NwsData.SelectedLocation.DisplayName;
            ElevationStatement = _session.SelectedLocation.Elevation.ElevationStatement;

            Instructions = $"Astronomical events and atmospheric planning for {LocationName} from {StartDate:dddd, MMM d} to {EndDate:dddd, MMM d}.";

            if (Days != null && Days.Length > 0)
            {
                SelectedDay = Days.FirstOrDefault() ?? new DayViewModel();
                RefreshSelectedDateData();
            }

        }

        /// <summary>
        /// Updates all data when a new date is selected.
        /// </summary>
        partial void OnSelectedDateChanged(DateOnly value)
        {
            if (Days == null || Days.Length == 0) return;
            SelectedDay = Days.FirstOrDefault(d => d.Date == value);
            RefreshSelectedDateData();
        }

        /// <summary>
        /// Refreshes all data for the currently selected date.
        /// </summary>
        private void RefreshSelectedDateData()
        {
            if (SelectedDay == null) return;

            var selectedDate = SelectedDay.Date;
            var dateOnly = selectedDate;

            // Astronomical Events
            SelectedDayEventModel = _session.EventsData?.FirstOrDefault(e => DateOnly.FromDateTime(e.Date.Date) == selectedDate) ?? new EventModel();
            SelectedDayEventTimes = SelectedDayEventModel?.EventTimes ?? Array.Empty<EventTime>();

            // Sky Color
            if (_session.SkyColors != null && _session.SkyColors.TryGetValue(dateOnly, out var skyGradient))
            {
                SelectedDaySkyGradient = skyGradient;
            }
            SkyColorExplanation = BuildSkyColorExplanation(SelectedDayEventModel?.AstronomicalData);

            // Day/Sun/Moon Information
            if (SelectedDayEventModel?.AstronomicalData != null)
            {
                var astro = SelectedDayEventModel.AstronomicalData;
                DayLengthStatement = $"Day Length: {astro.DayLength.Hours}h {astro.DayLength.Minutes}m";

                var openMeteoDaily = _session.WeatherForecasts?.DailyForecasts?.FirstOrDefault(d => d.Date == dateOnly);
                if (openMeteoDaily != null)
                {
                    SunriseStatement = $"Sunrise: {openMeteoDaily.Sunrise:h:mm tt}";
                    SunsetStatement = $"Sunset: {openMeteoDaily.Sunset:h:mm tt}";
                }

                MoonPhaseStatement = $"Moon Phase: {astro.MoonPhase}";
            }

            // Sunshine Duration
            var gfsDaily = _session.WeatherForecasts?.DailyForecasts?.FirstOrDefault(d => d.Date == dateOnly);
            if (gfsDaily?.SunshineDuration != null && gfsDaily.SunshineDuration.Value > 0)
            {
                var sunshineSpan = TimeSpan.FromSeconds(gfsDaily.SunshineDuration.Value);
                SunshineDurationStatement = $"Sunshine Duration: {sunshineSpan.Hours}h {sunshineSpan.Minutes}m";
            }
            else
            {
                SunshineDurationStatement = "Sunshine Duration: Not available";
            }

            // Moon Phase Visual
            //    SelectedDayMoonPhase = _session.MoonPhases?.FirstOrDefault(m => m.Date == dateOnly);

            // Hourly Forecasts for Weather Context
            SelectedDayHourlyForecasts = _session.NwsData?.HourlyForecasts?.Where(h => DateOnly.FromDateTime(h.StartTime.DateTime) == selectedDate).ToArray() ?? Array.Empty<NwsHourlyForecast>();
            WeatherSuitabilityStatement = BuildWeatherSuitabilityStatement(SelectedDayHourlyForecasts);
        }

        /// <summary>
        /// Builds a human-readable explanation of sky color for the selected day.
        /// </summary>
        private string BuildSkyColorExplanation(Astronomical astro)
        {
            if (astro == null)
                return "Sky color varies with time of day, cloud cover, humidity, and atmospheric particles (aerosols, dust, pollution).";

            var sunAlt = astro.SunAltitude;
            var explanation = "Sky color is determined by:\n\n";

            if (sunAlt > 6)
                explanation += "• Golden/Blue Hour: Sun is low, creating warm amber and deep blue tones.\n";
            else if (sunAlt > 0)
                explanation += "• Sunrise/Sunset Window: Intense color from refracted light through particles.\n";
            else
                explanation += "• Night: Dark sky colored by twilight, moon, and light pollution.\n";

            explanation += "• Cloud Cover: Reflects light, muting vibrant colors and creating pastels.\n";
            explanation += "• Humidity & Particles: PM2.5, dust, and pollution flatten gradients.\n";
            explanation += "• Atmospheric Pressure: Higher pressure intensifies Rayleigh scattering (deeper blues).\n";

            return explanation;
        }

        /// <summary>
        /// Builds a weather suitability statement based on hourly conditions.
        /// </summary>
        private string BuildWeatherSuitabilityStatement(NwsHourlyForecast[] hourlies)
        {
            if (hourlies == null || hourlies.Length == 0)
                return "No weather data available.";

            var rainCount = hourlies.Where(h => h.ProbabilityOfPrecipitation?.Value > 30).Count();
            var avgWindSpeed = hourlies.Average(h => h.WindSpeed?.Value ?? 0);
            var precipHigh = hourlies.Where(h => h.ProbabilityOfPrecipitation?.Value > 50).Count();

            var status = precipHigh > hourlies.Length / 4 ? "Light Rain Possible" : "Clear to Partly Cloudy";
            var windDesc = avgWindSpeed < 5 ? "calm" : avgWindSpeed < 12 ? "light breeze" : avgWindSpeed < 20 ? "moderate winds" : "strong winds";

            return $"🌤️ Atmospheric Conditions: {status}, {windDesc} (~{(int)avgWindSpeed} km/h). Ideal for golden-hour and twilight photography outside precipitation windows.";
        }
    }
}
