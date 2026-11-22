using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.ML;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels;

public partial class ForecastViewModel : ObservableObject
{
    private readonly ISession _session;
    private IDispatcherTimer? _timer;

    [ObservableProperty]
    public partial string LocationName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LocationTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CalendarDayModel[] Dates { get; set; } = Array.Empty<CalendarDayModel>();

    [ObservableProperty]
    public partial CalendarDayModel? SelectedCalendarDay { get; set; }

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; }

    [ObservableProperty]
    public partial string DailySummaryTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DailyForecast SelectedDaily { get; set; } = new DailyForecast();

    [ObservableProperty]
    public partial DayViewModel SelectedDay { get; set; } = new DayViewModel();

    [ObservableProperty]
    public partial ObservableCollection<DayPeriod> SelectedDayEventsByPeriod { get; set; } = new ObservableCollection<DayPeriod>();

    [ObservableProperty]
    public partial string SelectedHighTemp { get; set; } = "Daily high isn't available";

    [ObservableProperty]
    public partial string SelectedHighTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedLowTemp { get; set; } = "Daily low isn't available";

    [ObservableProperty]
    public partial string SelectedLowTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SKFileLottieImageSource? SelectedDailyMeteocon { get; set; }

    [ObservableProperty]
    public partial string SelectedHealthSafetySummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedWeatherSummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDayLength { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedNightLength { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedMoonPhase { get; set; } = string.Empty;

    // Daily ONNX Prediction Summary & Hourly Predictions
    [ObservableProperty]
    public partial string SelectedDailyPhotoScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailyDroneScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailyDominantCondition { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailyScoreSummaryText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasSelectedDayPredictions { get; set; }

    [ObservableProperty]
    public partial string SelectedDailyBlueHourScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailySunriseScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailySunsetScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailyLunarScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailyGalaxyScoreText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDailyBestHoursText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<HourlyForecastPrediction> SelectedDayHourlyPredictions { get; set; } = new ObservableCollection<HourlyForecastPrediction>();

    // Kept for backward compatibility
    [ObservableProperty]
    public partial string PeriodName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedSunshine { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SKFileLottieImageSource? SelectedMoonImage { get; set; }

    [ObservableProperty]
    public partial EventTime[] SelectedDailyEvents { get; set; } = Array.Empty<EventTime>();

    [ObservableProperty]
    public partial HourlyForecast SelectedEarlyMorningHourlyForecast { get; set; } = new HourlyForecast();

    [ObservableProperty]
    public partial HourlyForecast SelectedMorningHourlyForecast { get; set; } = new HourlyForecast();

    [ObservableProperty]
    public partial HourlyForecast SelectedAfternoonHourlyForecast { get; set; } = new HourlyForecast();

    [ObservableProperty]
    public partial HourlyForecast SelectedEveningHourlyForecast { get; set; } = new HourlyForecast();

    [ObservableProperty]
    public partial HourlyForecast SelectedNightHourlyForecast { get; set; } = new HourlyForecast();

    public ForecastViewModel(ISession session)
    {
        _session = session;
        RefreshFromSession();
        StartClock();
    }

    public void RefreshFromSession()
    {
        LocationName = _session.NwsData?.SelectedLocation?.DisplayName
                       ?? _session.SelectedLocation?.DisplayName
                       ?? string.Empty;

        Dates = _session.Dates?.ToArray() ?? Array.Empty<CalendarDayModel>();

        var matching = Dates.FirstOrDefault(d => d.Date.Date == SelectedDate.Date) ?? Dates.FirstOrDefault();
        SelectedCalendarDay = matching;
        SelectedDate = matching?.Date ?? DateTime.Today;

        _ = UpdateForDate(SelectedDate);
        UpdateLocationTime();
    }

    public void StartClock()
    {
        if (_timer == null)
        {
            _timer = Application.Current?.Dispatcher.CreateTimer();
            if (_timer != null)
            {
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += (_, _) => UpdateLocationTime();
            }
        }
        if (_timer != null && !_timer.IsRunning)
        {
            UpdateLocationTime();
            _timer.Start();
        }
    }

    public void StopClock()
    {
        _timer?.Stop();
    }

    private void UpdateLocationTime()
    {
        int offsetSeconds = _session.SelectedTimeZoneOffset != 0
            ? _session.SelectedTimeZoneOffset
            : (_session.NwsData?.TimezoneOffsetSeconds ?? 0);

        DateTime localTime = DateTime.UtcNow.AddSeconds(offsetSeconds);
        LocationTime = $"Location Current Time: {localTime:dddd, MMMM d, yyyy h:mm:ss tt}";
    }

    [RelayCommand]
    public async Task SelectDate(CalendarDayModel? day)
    {
        if (day == null) return;
        SelectedCalendarDay = day;
        SelectedDate = day.Date;
        await UpdateForDate(SelectedDate);
    }

    [RelayCommand]
    private async Task OnSelectedDateChanged(CalendarDayModel? value)
    {
        if (value == null) return;
        await SelectDate(value);
    }

    [RelayCommand]
    private async Task GoToNws()
    {
        await Browser.Default.OpenAsync("https://www.wpc.ncep.noaa.gov/heatrisk/");
    }

    private async Task UpdateForDate(DateTime date)
    {
        SelectedDate = date;
        DailySummaryTitle = $"Daily Summary for {SelectedDate:dddd, MMMM d, yyyy}";

        var forecast = _session.WeatherForecasts?.DailyForecasts?.FirstOrDefault(x => x.Date == DateOnly.FromDateTime(date))
                       ?? new DailyForecast { Date = DateOnly.FromDateTime(date) };
        SelectedDaily = forecast;

        SelectedDay = _session.Days?.FirstOrDefault(d => d.Date.Date == date.Date) ?? new DayViewModel();
        SelectedDayEventsByPeriod = SelectedDay.Periods ?? new ObservableCollection<DayPeriod>();

        // Daily High and Low
        if (forecast.DailyForecastSummary != null && !string.IsNullOrWhiteSpace(forecast.DailyForecastSummary.DailyHighAndTime))
        {
            var parts = forecast.DailyForecastSummary.DailyHighAndTime.Split('@');
            SelectedHighTemp = parts[0].Trim();
            SelectedHighTime = parts.Length > 1 ? $"~ {parts[1].Trim()}" : string.Empty;
        }
        else if (forecast.Temperature2mMax != null)
        {
            SelectedHighTemp = $"{Math.Round(forecast.Temperature2mMax.Value)} F";
            SelectedHighTime = !string.IsNullOrWhiteSpace(forecast.TimeTempMax) ? $"~ {forecast.TimeTempMax}" : string.Empty;
        }
        else
        {
            SelectedHighTemp = "Daily high isn't available";
            SelectedHighTime = string.Empty;
        }

        if (forecast.DailyForecastSummary != null && !string.IsNullOrWhiteSpace(forecast.DailyForecastSummary.DailyLowAndTime))
        {
            var parts = forecast.DailyForecastSummary.DailyLowAndTime.Split('@');
            SelectedLowTemp = parts[0].Trim();
            SelectedLowTime = parts.Length > 1 ? $"~ {parts[1].Trim()}" : string.Empty;
        }
        else if (forecast.Temperature2mMin != null)
        {
            SelectedLowTemp = $"{Math.Round(forecast.Temperature2mMin.Value)} F";
            SelectedLowTime = !string.IsNullOrWhiteSpace(forecast.TimeTempMin) ? $"~ {forecast.TimeTempMin}" : string.Empty;
        }
        else
        {
            SelectedLowTemp = "Daily low isn't available";
            SelectedLowTime = string.Empty;
        }

        // Daily Lottie Meteocon
        SelectedDailyMeteocon = forecast.SkDailyMeteocon ?? MeteoconsResolver.ResolveIcon(forecast.WeatherCode, true);

        // Health & Safety Summary
        SelectedHealthSafetySummary = !string.IsNullOrWhiteSpace(forecast.DailyForecastSummary?.HealthSafetySummary)
            ? forecast.DailyForecastSummary.HealthSafetySummary
            : "Health & safety summary isn't available.";

        // Weather Summary
        SelectedWeatherSummary = !string.IsNullOrWhiteSpace(forecast.DailyForecastSummary?.WeatherSummary)
            ? forecast.DailyForecastSummary.WeatherSummary
            : (!string.IsNullOrWhiteSpace(forecast.WeatherCodeDescription)
                ? $"{forecast.WeatherCodeDescription}. Winds from the {forecast.WindDirection10mDominant?.Description}."
                : "Weather summary isn't available.");

        // Sun & Moon Summary
        if (forecast.DailyEvents?.AstronomicalData != null && forecast.DailyEvents.AstronomicalData.DayLength > TimeSpan.Zero)
        {
            var dayLen = forecast.DailyEvents.AstronomicalData.DayLength;
            SelectedDayLength = $"{dayLen.Hours} h {dayLen.Minutes} m";
            var nightLen = TimeSpan.FromHours(24) - dayLen;
            SelectedNightLength = $"{nightLen.Hours} h {nightLen.Minutes} m";
            SelectedMoonPhase = !string.IsNullOrWhiteSpace(forecast.DailyEvents.AstronomicalData.MoonPhase)
                ? forecast.DailyEvents.AstronomicalData.MoonPhase
                : "Not available";
        }
        else
        {
            SelectedDayLength = "Not available";
            SelectedNightLength = "Not available";
            SelectedMoonPhase = "Not available";
        }

        // Update photo and drone score summaries on periods from _session.ScoreSummaries
        var dailyScore = _session.ScoreSummaries?.FirstOrDefault(s => s.Date == DateOnly.FromDateTime(date))
                         ?? _session.DailyPhotoScoringSummary?.FirstOrDefault(s => s.Date == DateOnly.FromDateTime(date));

        if (dailyScore != null)
        {
            HasSelectedDayPredictions = true;
            SelectedDailyPhotoScoreText = $"Overall Photo Score: {dailyScore.DailyPhotoScore:F0}/100";
            SelectedDailyDroneScoreText = $"Avg Drone Flight Score: {dailyScore.AverageDroneSafetyScore:F0}/100";
            SelectedDailyDominantCondition = $"Dominant Condition: {dailyScore.DominantCondition}";
            SelectedDailyScoreSummaryText = dailyScore.HumanReadableDailySummary;

            var hourlyPreds = dailyScore.HourlyPredictions ?? Array.Empty<HourlyForecastPrediction>();
            if (hourlyPreds.Count > 0)
            {
                SelectedDailyBlueHourScoreText = $"Peak Blue Hour: {hourlyPreds.Max(h => h.BlueHourScore):F0}/100";
                SelectedDailySunriseScoreText = $"Peak Sunrise: {hourlyPreds.Max(h => h.SunriseScore):F0}/100";
                SelectedDailySunsetScoreText = $"Peak Sunset: {hourlyPreds.Max(h => h.SunsetScore):F0}/100";
                SelectedDailyLunarScoreText = $"Peak Lunar: {hourlyPreds.Max(h => h.LunarEventScore):F0}/100";
                SelectedDailyGalaxyScoreText = $"Peak Galaxy: {hourlyPreds.Max(h => h.GalaxyScore):F0}/100";
            }
            else
            {
                SelectedDailyBlueHourScoreText = "Peak Blue Hour: N/A";
                SelectedDailySunriseScoreText = "Peak Sunrise: N/A";
                SelectedDailySunsetScoreText = "Peak Sunset: N/A";
                SelectedDailyLunarScoreText = "Peak Lunar: N/A";
                SelectedDailyGalaxyScoreText = "Peak Galaxy: N/A";
            }

            SelectedDailyBestHoursText = dailyScore.BestHours is { Count: > 0 }
                ? $"Best Hours: {string.Join(", ", dailyScore.BestHours.Select(h => h.Timestamp.LocalDateTime.ToString("h tt")))}"
                : "Best Hours: None";

            SelectedDayHourlyPredictions = new ObservableCollection<HourlyForecastPrediction>(hourlyPreds);
        }
        else
        {
            HasSelectedDayPredictions = false;
            SelectedDailyPhotoScoreText = "Overall Photo Score: N/A";
            SelectedDailyDroneScoreText = "Avg Drone Flight Score: N/A";
            SelectedDailyDominantCondition = string.Empty;
            SelectedDailyBlueHourScoreText = string.Empty;
            SelectedDailySunriseScoreText = string.Empty;
            SelectedDailySunsetScoreText = string.Empty;
            SelectedDailyLunarScoreText = string.Empty;
            SelectedDailyGalaxyScoreText = string.Empty;
            SelectedDailyBestHoursText = string.Empty;
            SelectedDailyScoreSummaryText = "No AI predictions available for this date.";
            SelectedDayHourlyPredictions = new ObservableCollection<HourlyForecastPrediction>();
        }

        if (SelectedDayEventsByPeriod != null)
        {
            foreach (var period in SelectedDayEventsByPeriod)
            {
                if (dailyScore?.HourlyPredictions != null && dailyScore.HourlyPredictions.Count > 0)
                {
                    var preds = dailyScore.HourlyPredictions
                        .Where(p => TimeOfDayService.CalcTimeOfDay(p.LocalTime.DateTime.Hour) == period.TimeOfDayName)
                        .ToList();

                    if (preds.Count > 0)
                    {
                        float pScore = preds.Average(x => x.PhotoScore);
                        float dScore = preds.Average(x => x.DroneSafetyScore);
                        period.PhotoScore = $"Photo: {pScore:F0}/100";
                        period.DroneScore = $"Drone: {dScore:F0}/100";
                        period.ScoreSummary = preds.First().HumanReadableForecast;
                    }
                    else
                    {
                        period.PhotoScore = $"Photo: {dailyScore.DailyPhotoScore:F0}/100";
                        period.DroneScore = $"Drone: {dailyScore.AverageDroneSafetyScore:F0}/100";
                        period.ScoreSummary = dailyScore.HumanReadableDailySummary;
                    }
                }
                else
                {
                    period.PhotoScore = "Photo: N/A";
                    period.DroneScore = "Drone: N/A";
                    period.ScoreSummary = "Score summary isn't available.";
                }
            }
        }
    }
}