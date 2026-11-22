using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class EventComparisonRow : ObservableObject
    {
        public string EventName { get; set; } = string.Empty;
        public string TimeLabel { get; set; } = string.Empty;
        public string OtherTimeLabel { get; set; } = string.Empty;
        public bool HasOtherTime => !string.IsNullOrWhiteSpace(OtherTimeLabel);
        public DateTime WeatherHour { get; set; }

        public string NwsTemp { get; set; } = "--";
        public string NwsWind { get; set; } = string.Empty;
        public string NwsForecast { get; set; } = string.Empty;
        public SKFileLottieImageSource? NwsAnimation { get; set; }

        public string MeteoTemp { get; set; } = "--";
        public string MeteoWind { get; set; } = string.Empty;
        public string MeteoForecast { get; set; } = string.Empty;
        public SKFileLottieImageSource? MeteoAnimation { get; set; }

        public Color RowBackgroundColor { get; set; } = Color.FromArgb("#14FFFFFF");
    }

    public partial class DataCheckViewModel : ObservableObject
    {
        private readonly ISession _session;

        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;

        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; } = new PhotoLocation();

        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; } = Array.Empty<CalendarDayModel>();

        [ObservableProperty]
        public partial CalendarDayModel? SelectedDay { get; set; }

        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; }

        [ObservableProperty]
        public partial EventModel[] EventModels { get; set; } = Array.Empty<EventModel>();

        [ObservableProperty]
        public partial EventModel SelectedEventModel { get; set; } = new EventModel();

        [ObservableProperty]
        public partial EventTime[] SelectedEventModelEventTimes { get; set; } = Array.Empty<EventTime>();

        [ObservableProperty]
        public partial EventTime? SelectedEventModelSelectedEventTime { get; set; }

        [ObservableProperty]
        public partial string SelectedEventTimeMeteoForecast { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SelectedEventTimeNwsForecast { get; set; } = string.Empty;

        [ObservableProperty]
        public partial NwsDailyForecast[] NwsDailies { get; set; } = Array.Empty<NwsDailyForecast>();

        [ObservableProperty]
        public partial NwsDailyForecast SelectedNwsDaily { get; set; } = new NwsDailyForecast();

        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedNwsDailyHourlies { get; set; } = Array.Empty<NwsHourlyForecast>();

        [ObservableProperty]
        public partial NwsHourlyForecast SelectedNwsDailySelectedHourly { get; set; } = new NwsHourlyForecast();

        [ObservableProperty]
        public partial DailyForecast[] MeteoDailies { get; set; } = Array.Empty<DailyForecast>();

        [ObservableProperty]
        public partial DailyForecast SelectedMeteoDaily { get; set; } = new DailyForecast();

        [ObservableProperty]
        public partial HourlyForecast[] SelectedMeteoDailyHourlies { get; set; } = Array.Empty<HourlyForecast>();

        [ObservableProperty]
        public partial HourlyForecast SelectedMeteoDailySelectedHourly { get; set; } = new HourlyForecast();

        [ObservableProperty]
        public partial string NwsDailySnapshot { get; set; } = "No NWS daily forecast available.";

        [ObservableProperty]
        public partial string MeteoDailySnapshot { get; set; } = "No Open-Meteo daily forecast available.";

        [ObservableProperty]
        public partial ObservableCollection<EventComparisonRow> ComparisonRows { get; set; } = new ObservableCollection<EventComparisonRow>();

        public DataCheckViewModel(ISession session)
        {
            _session = session;
            RefreshFromSession();
        }

        public void RefreshFromSession()
        {
            NwsDailies = _session.NwsData?.DailyForecasts?.ToArray() ?? Array.Empty<NwsDailyForecast>();
            MeteoDailies = _session.WeatherForecasts?.DailyForecasts?.ToArray() ?? Array.Empty<DailyForecast>();
            EventModels = _session.EventsData?.ToArray() ?? Array.Empty<EventModel>();
            Dates = _session.Dates?.ToArray() ?? Array.Empty<CalendarDayModel>();

            SelectedLocation = new PhotoLocation
            {
                City = _session.NwsData?.SelectedLocation?.City ?? _session.SelectedLocation?.City ?? string.Empty,
                State = _session.NwsData?.SelectedLocation?.State ?? _session.SelectedLocation?.State ?? string.Empty,
            };

            var matching = Dates.FirstOrDefault(d => d.Date.Date == SelectedDate.Date) ?? Dates.FirstOrDefault();
            SelectedDay = matching;
            SelectedDate = matching?.Date ?? DateTime.Today;

            _ = UpdateForDate(SelectedDate);
        }

        [RelayCommand]
        public async Task SelectDate(CalendarDayModel? day)
        {
            if (day == null) return;
            SelectedDay = day;
            SelectedDate = day.Date;
            await UpdateForDate(SelectedDate);
        }

        partial void OnSelectedDayChanged(CalendarDayModel? value)
        {
            if (value == null) return;
            if (SelectedDate.Date != value.Date.Date)
            {
                SelectedDate = value.Date;
                _ = UpdateForDate(SelectedDate);
            }
        }

        private void UpdateDateCardHighlights(DateTime activeDate)
        {
            foreach (var d in Dates)
            {
                bool isSelected = d.Date.Date == activeDate.Date;
                d.BackgroundColor = isSelected ? Color.FromArgb("#38BDF8") : Colors.White;
                d.TextColor = isSelected ? Color.FromArgb("#081F43") : Colors.Black;
                d.BorderColor = isSelected ? Color.FromArgb("#BAE6FD") : Color.FromArgb("#D0D7DE");
            }
        }

        private Task UpdateForDate(DateTime date)
        {
            SelectedDate = date;
            UpdateDateCardHighlights(date);

            string locDisplay = !string.IsNullOrWhiteSpace(SelectedLocation.DisplayName) && SelectedLocation.DisplayName != ", "
                ? SelectedLocation.DisplayName
                : _session.SelectedLocation?.DisplayName ?? string.Empty;

            Title = string.IsNullOrWhiteSpace(locDisplay)
                ? $"{date:dddd, MMMM d, yyyy}"
                : $"{locDisplay}  •  {date:dddd, MMMM d, yyyy}";

            // 1. NWS Daily & Hourlies for selected date
            SelectedNwsDaily = NwsDailies.FirstOrDefault(x => x.StartTime.Date == date.Date) ?? new NwsDailyForecast();
            SelectedNwsDailyHourlies = SelectedNwsDaily.HourlyWeathers?.ToArray()
                ?? _session.NwsData?.HourlyForecasts?.Where(h => h.StartTime.Date == date.Date).ToArray()
                ?? Array.Empty<NwsHourlyForecast>();
            SelectedNwsDailySelectedHourly = SelectedNwsDailyHourlies.FirstOrDefault() ?? new NwsHourlyForecast();

            NwsDailySnapshot = !string.IsNullOrWhiteSpace(SelectedNwsDaily.DetailForecast)
                ? SelectedNwsDaily.DetailForecast
                : (!string.IsNullOrWhiteSpace(SelectedNwsDaily.ShortForecast)
                    ? SelectedNwsDaily.ShortForecast
                    : "No NWS daily forecast available for this date.");

            // 2. Open-Meteo Daily & Hourlies for selected date
            var targetDateOnly = DateOnly.FromDateTime(date);
            SelectedMeteoDaily = MeteoDailies.FirstOrDefault(x => x.Date == targetDateOnly) ?? new DailyForecast();
            SelectedMeteoDailyHourlies = SelectedMeteoDaily.HourlyForecasts?.ToArray()
                ?? _session.WeatherForecasts?.HourlyForecasts?.Where(h => h.HourlyTime.Date == date.Date).ToArray()
                ?? Array.Empty<HourlyForecast>();
            SelectedMeteoDailySelectedHourly = SelectedMeteoDailyHourlies.FirstOrDefault() ?? new HourlyForecast();

            MeteoDailySnapshot = !string.IsNullOrWhiteSpace(SelectedMeteoDaily.DailyForecastSummary?.WeatherSummary)
                ? SelectedMeteoDaily.DailyForecastSummary.WeatherSummary
                : (!string.IsNullOrWhiteSpace(SelectedMeteoDaily.WeatherCodeDescription)
                    ? $"{SelectedMeteoDaily.WeatherCodeDescription} (High {SelectedMeteoDaily.Temperature2mMax?.Value:F0}°, Low {SelectedMeteoDaily.Temperature2mMin?.Value:F0}°)"
                    : "No Open-Meteo daily forecast available for this date.");

            // 3. Events for selected date
            SelectedEventModel = EventModels.FirstOrDefault(e => e.Date.Date == date.Date) ?? new EventModel();
            SelectedEventModelEventTimes = SelectedEventModel.EventTimes?.OrderBy(e => e.AstroTime).ToArray() ?? Array.Empty<EventTime>();
            SelectedEventModelSelectedEventTime = SelectedEventModelEventTimes.FirstOrDefault();

            // 4. Build per-event comparison rows
            var rows = new ObservableCollection<EventComparisonRow>();
            var allNwsHourlies = _session.NwsData?.HourlyForecasts ?? Array.Empty<NwsHourlyForecast>();
            var allMeteoHourlies = _session.WeatherForecasts?.HourlyForecasts ?? Array.Empty<HourlyForecast>();

            for (int i = 0; i < SelectedEventModelEventTimes.Length; i++)
            {
                var ev = SelectedEventModelEventTimes[i];

                var nwsHour = SelectedNwsDailyHourlies.FirstOrDefault(n => n.StartTime == ev.WeatherHour)
                              ?? allNwsHourlies.FirstOrDefault(n => n.StartTime == ev.WeatherHour)
                              ?? SelectedNwsDailyHourlies.OrderBy(n => Math.Abs((n.StartTime - ev.AstroTime).TotalMinutes)).FirstOrDefault();

                var meteoHour = SelectedMeteoDailyHourlies.FirstOrDefault(m => m.HourlyTime == ev.WeatherHour)
                                ?? allMeteoHourlies.FirstOrDefault(m => m.HourlyTime == ev.WeatherHour)
                                ?? SelectedMeteoDailyHourlies.OrderBy(m => Math.Abs((m.HourlyTime - ev.AstroTime).TotalMinutes)).FirstOrDefault();

                string nwsForecastText = nwsHour != null
                    ? (!string.IsNullOrWhiteSpace(nwsHour.DetailedForecast) ? nwsHour.DetailedForecast : nwsHour.ShortForecast ?? "No details.")
                    : "No NWS hourly forecast available.";

                string meteoForecastText = meteoHour != null
                    ? (!string.IsNullOrWhiteSpace(meteoHour.HourlyForecastSummary?.WeatherSummary)
                        ? meteoHour.HourlyForecastSummary.WeatherSummary
                        : meteoHour.WeatherCodeDescription ?? "No details.")
                    : "No Open-Meteo hourly forecast available.";

                string nwsTempText = nwsHour?.Temperature != null ? nwsHour.TempAndUnit : "--";
                string nwsWindText = nwsHour?.WindSpeed != null
                    ? $"{nwsHour.WindSpeed.Value} {nwsHour.WindSpeed.UnitCode} {nwsHour.WindDirection}".Trim()
                    : string.Empty;

                string meteoTempText = meteoHour?.Temperature2m != null
                    ? $"{Math.Round(meteoHour.Temperature2m.Value)}{meteoHour.Temperature2m.Unit}"
                    : "--";
                string meteoWindText = meteoHour?.WindSpeed10m != null
                    ? $"{meteoHour.WindSpeed10m.Value} {meteoHour.WindSpeed10m.Unit?.Replace("/", "")}".Trim()
                    : string.Empty;

                rows.Add(new EventComparisonRow
                {
                    EventName = ev.EventName,
                    TimeLabel = ev.TimeLabel,
                    OtherTimeLabel = ev.OtherTimeLabel ?? string.Empty,
                    WeatherHour = ev.WeatherHour,
                    NwsTemp = nwsTempText,
                    NwsWind = nwsWindText,
                    NwsForecast = nwsForecastText,
                    NwsAnimation = nwsHour?.SkHourlyAnimation,
                    MeteoTemp = meteoTempText,
                    MeteoWind = meteoWindText,
                    MeteoForecast = meteoForecastText,
                    MeteoAnimation = meteoHour?.SkHourlyMeteocon,
                    RowBackgroundColor = i % 2 == 0 ? Color.FromArgb("#1AFFFFFF") : Color.FromArgb("#0DFFFFFF")
                });
            }

            ComparisonRows = rows;

            var firstRow = rows.FirstOrDefault();
            SelectedEventTimeNwsForecast = firstRow?.NwsForecast ?? string.Empty;
            SelectedEventTimeMeteoForecast = firstRow?.MeteoForecast ?? string.Empty;

            return Task.CompletedTask;
        }
    }
}