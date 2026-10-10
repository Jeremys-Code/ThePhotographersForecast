using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels;

public partial class ForecastViewModel : ObservableObject
{
    private System.Timers.Timer _timer;

    [ObservableProperty]
    public partial string LocationName { get; set; }
    [ObservableProperty]
    public partial LinearGradientBrush BackgroundGradient { get; set; }
    [ObservableProperty]
    public partial string LocationTime { get; set; }

    // Collections
    [ObservableProperty]
    public partial CalendarDayModel[] Dates { get; set; }
    [ObservableProperty]
    public partial DayViewModel[] Days { get; set; }
    [ObservableProperty]
    public partial DayViewModel SelectedDay { get; set; }
    [ObservableProperty]
    public partial WeatherForecast[] WeatherForecasts { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<WeatherStatementsValue?> AllWeatherStatementsValues { get; set; }

    // Selected Day Collections
    [ObservableProperty]
    public partial EventTime[] SelectedDailyEvents { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<DayPeriod> SelectedDayEventsByPeriod { get; set; }

    //Selected Day Properties
    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; }
    [ObservableProperty]
    public partial DailyForecast SelectedDaily { get; set; }
    [ObservableProperty]
    public partial string PeriodName { get; set; }
    [ObservableProperty]
    public partial string SelectedDayLength { get; set; }
    [ObservableProperty]
    public partial string SelectedSunshine { get; set; }
    [ObservableProperty]
    public partial string SelectedMoonPhase { get; set; }
    [ObservableProperty]
    public partial SKFileLottieImageSource SelectedMoonImage { get; set; }
    [ObservableProperty]
    public partial HourlyForecast SelectedEarlyHoursHourlyForecast { get; set; }
    [ObservableProperty]
    public partial HourlyForecast SelectedEarlyMorningHourlyForecast { get; set; }
    [ObservableProperty]
    public partial HourlyForecast SelectedMorningHourlyForecast { get; set; }
    [ObservableProperty]
    public partial HourlyForecast SelectedAfternoonHourlyForecast { get; set; }
    [ObservableProperty]
    public partial HourlyForecast SelectedEveningHourlyForecast { get; set; }
    [ObservableProperty]
    public partial HourlyForecast SelectedNightHourlyForecast { get; set; }
    [ObservableProperty]
    public partial Dictionary<DateOnly, LinearGradientBrush> DailyGradients { get; set; }

    [ObservableProperty]
    public partial Services.TimeOfDayService.TimeOfDay SelectedTimeOfDay { get; set; }

    [ObservableProperty]
    public partial Session SharedSession { get; set; }
    public ForecastViewModel(Session sharedSession)
    {
        SharedSession = sharedSession;
        //_skyEngine = skyEngine;
        MainThread.BeginInvokeOnMainThread(() => Dates = [.. SharedSession.Dates]);
        if (Dates.Length == SharedSession.Dates.Length)
        {
            SelectedDate = Dates[0].Date;
        }
        //  MainThread.BeginInvokeOnMainThread(() => Days = [.. SharedSession.Days]);
        if (Dates?.Length == 0) return;
        if (WeatherForecasts?.Length == 0) return;
        if (SharedSession.Days.Length == 0) return;
        LocationName = SharedSession.SelectedLocation.DisplayName;
        SelectedDay = SharedSession.Days.Where(d => d.Date.Date == Dates[0].Date).FirstOrDefault<DayViewModel>();
        if (SelectedDay == null) return;
        //MainThread.BeginInvokeOnMainThread(async ()=>
        //{
        //    DailyGradients = await _skyEngine.GetDailySkyColorGradientsAsync();
        //    BackgroundGradient = DailyGradients.TryGetValue(DateOnly.FromDateTime(Dates[0].Time)) ? background : (LinearGradientBrush)Application.Current.Resources["Noon"];
        //});

        SelectedDaily = SharedSession.WeatherForecasts.DailyForecasts.Where(w => w.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();

        SelectedTimeOfDay = TimeOfDayService.GetTimeOfDay(DateTime.UtcNow.AddSeconds(SharedSession.SelectedTimeZoneOffset).Hour);

        SelectedEarlyHoursHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Early Hours" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedEarlyMorningHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Early Morning" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedMorningHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Morning" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedAfternoonHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Afternoon" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedEveningHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Evening" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedNightHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Night" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();

        PeriodName = SelectedDaily?.TimePeriod ?? string.Empty;

        SelectedDayLength = $"{SelectedDaily.DailyEvents.AstronomicalData.DayLength.Hours} h {SelectedDaily.DailyEvents.AstronomicalData.DayLength.Minutes} m";
        SelectedSunshine = $"{TimeSpan.FromSeconds(SelectedDaily.SunshineDuration.Value).Hours} h {TimeSpan.FromSeconds(SelectedDaily.SunshineDuration.Value).Minutes} m";
        SelectedMoonPhase = SelectedDaily.DailyEvents.AstronomicalData.MoonPhase;
        SelectedMoonImage = SelectedDaily.DailyEvents.AstronomicalData.SkMoon;

        SelectedDailyEvents = SelectedDaily.DailyEvents.EventTimes;
        SelectedDayEventsByPeriod = SelectedDay.Periods;
        //var earlyHours = new SKFileLottieImageSource();
        //var earlyMorning = new SKFileLottieImageSource();
        //var morning = new SKFileLottieImageSource();
        //var afternoon = new SKFileLottieImageSource();
        //var evening = new SKFileLottieImageSource();
        //var night = new SKFileLottieImageSource();
        //MainThread.BeginInvokeOnMainThread(async () =>
        //{
        //    earlyHours.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_late_night.json"));
        //    earlyMorning.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_morning.json"));
        //    morning.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_late_morning.json"));
        //    afternoon.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_afternoon.json"));
        //    evening.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_evening.json"));
        //    night.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_night.json"));
        //});
        //EventsByPeriod = new ObservableCollection<DayPeriod>
        // {
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Early Hours",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.Date && d.WeatherHour < SelectedDate.AddHours(3)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyHours"],
        //            TimeOfDayImage = earlyHours,
        //            HourlyName = SelectedEarlyHoursHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedEarlyHoursHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedEarlyHoursHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedEarlyHoursHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Early Morning",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(3) && d.WeatherHour < SelectedDate.AddHours(6)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"],
        //            TimeOfDayImage = earlyMorning,
        //            HourlyName = SelectedEarlyMorningHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedEarlyMorningHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedEarlyMorningHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedEarlyMorningHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Morning",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(6) && d.WeatherHour < SelectedDate.AddHours(12)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodMorning"],
        //            TimeOfDayImage = morning,
        //            HourlyName = SelectedMorningHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedMorningHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedMorningHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedMorningHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Afternoon",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(12) && d.WeatherHour < SelectedDate.AddHours(18)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"],
        //            TimeOfDayImage = afternoon,
        //            HourlyName = SelectedAfternoonHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedAfternoonHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedAfternoonHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedAfternoonHourlyForecast.HourlyForecastSummary.StormSummary
        //         },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Evening",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(18) && d.WeatherHour < SelectedDate.AddHours(22)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEvening"],
        //            TimeOfDayImage = evening,
        //           HourlyName = SelectedEveningHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedEveningHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedEveningHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedEveningHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Night",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(22) && d.WeatherHour < SelectedDate.AddHours(24)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodNight"],
        //            TimeOfDayImage = night,
        //            HourlyName = SelectedNightHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedNightHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedNightHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedNightHourlyForecast.HourlyForecastSummary.StormSummary
        //        }
        //};
        // Initialize timer
        LocationTime = $"as of {DateTime.UtcNow.AddSeconds(SharedSession.SelectedTimeZoneOffset).ToString("F")}";
        _timer = new System.Timers.Timer(1000); // 1 second interval
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }
    [RelayCommand]
    private void OnSelectedDateChanged(CalendarDayModel Value)
    {
        SelectedDate = Value.Date;
        UpdateForDate(SelectedDate);
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        DateTime time = DateTime.UtcNow.AddSeconds(SharedSession.SelectedTimeZoneOffset);
        LocationTime = $"where the local time is: {time.ToString("F")}";
    }

    private string BuildStormSummaryByTimeOfDay(DateTime date, string timeOfDay)
    {
        try
        {
            var dayStorms = SharedSession.NwsData.HourlySupplementals.Where(d => d.Time.Date == date.Date && d.TimeOfDay == timeOfDay && d.Weather != null).OrderBy(a => a.Time).ToArray();
            if (dayStorms.Length > 0)
            {
                string stormTimeOfDay = string.Empty;
                foreach (var item in dayStorms)
                {
                    stormTimeOfDay += item.Weather.Statement;
                }
                return stormTimeOfDay;
            }
        }
        catch (NullReferenceException)
        {
            return "";

        }
        catch (Exception)
        {
            return "";
        }
        return "";
    }

    [RelayCommand]
    private async Task GoToNws()
    {
        //await Launcher.OpenAsync("https://www.wpc.ncep.noaa.gov/heatrisk/");
        await Browser.Default.OpenAsync("https://www.wpc.ncep.noaa.gov/heatrisk/");
        return;
    }

    private async Task UpdateForDate(DateTime date)
    {
        SelectedDate = date;
        //    BackgroundGradient = DailyGradients.TryGetValue(DateOnly.FromDateTime(Dates[0].Time)) : (LinearGradientBrush)Application.Current.Resources["Noon"];
        var forecast = SharedSession.WeatherForecasts.DailyForecasts.Where(x => x.Date == DateOnly.FromDateTime(SelectedDate)).SingleOrDefault();
        SelectedDaily = forecast;

        if (SelectedDaily == null)
        {
            // No forecast available yet for this date - clear selections and return early
            SelectedEarlyHoursHourlyForecast = null;
            SelectedEarlyMorningHourlyForecast = null;
            SelectedMorningHourlyForecast = null;
            SelectedAfternoonHourlyForecast = null;
            SelectedEveningHourlyForecast = null;
            SelectedNightHourlyForecast = null;
            PeriodName = string.Empty;
            SelectedDayLength = string.Empty;
            SelectedSunshine = string.Empty;
            SelectedMoonPhase = string.Empty;
            SelectedMoonImage = null;
            SelectedDailyEvents = null;
            SelectedDayEventsByPeriod = new ObservableCollection<DayPeriod>();
            return;
        }

        SelectedEarlyHoursHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Early Hours" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedEarlyMorningHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Early Morning" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedMorningHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Morning" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedAfternoonHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Afternoon" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedEveningHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Evening" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
        SelectedNightHourlyForecast = SelectedDaily.HourlyForecasts.Where(x => x.HourlyTime.Date == SelectedDate.Date && x.TimeOfDay == "Night" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();

        PeriodName = SelectedDaily.TimePeriod ?? string.Empty;

        SelectedDayLength = SelectedDaily.DailyEvents?.AstronomicalData != null ? $"{SelectedDaily.DailyEvents.AstronomicalData.DayLength.Hours} h {SelectedDaily.DailyEvents.AstronomicalData.DayLength.Minutes} m" : string.Empty;
        SelectedSunshine = SelectedDaily.SunshineDuration != null ? $"{TimeSpan.FromSeconds(SelectedDaily.SunshineDuration.Value).Hours} h {TimeSpan.FromSeconds(SelectedDaily.SunshineDuration.Value).Minutes} m" : string.Empty;
        SelectedMoonPhase = SelectedDaily.DailyEvents?.AstronomicalData?.MoonPhase ?? string.Empty;
        SelectedMoonImage = SelectedDaily.DailyEvents?.AstronomicalData?.SkMoon;
        SelectedDailyEvents = SelectedDaily.DailyEvents.EventTimes;

        //var earlyHours = new SKFileLottieImageSource();
        //var earlyMorning = new SKFileLottieImageSource();
        //var morning = new SKFileLottieImageSource();
        //var afternoon = new SKFileLottieImageSource();
        //var evening = new SKFileLottieImageSource();
        //var night = new SKFileLottieImageSource();
        //MainThread.BeginInvokeOnMainThread(async () =>
        //{
        //    earlyHours.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_late_night.json"));
        //    earlyMorning.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_morning.json"));
        //    morning.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_late_morning.json"));
        //    afternoon.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_afternoon.json"));
        //    evening.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_evening.json"));
        //    night.File = await Helper.GetLottieFromRawResources(Path.Combine("meteocons", "time_night.json"));
        //});
        //EventsByPeriod = new ObservableCollection<DayPeriod>
        // {
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Early Hours",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.Date && d.WeatherHour < SelectedDate.AddHours(3)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyHours"],
        //            TimeOfDayImage = earlyHours,
        //            HourlyName = SelectedEarlyHoursHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedEarlyHoursHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedEarlyHoursHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedEarlyHoursHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Early Morning",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(3) && d.WeatherHour < SelectedDate.AddHours(6)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"],
        //            TimeOfDayImage = earlyMorning,
        //            HourlyName = SelectedEarlyMorningHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedEarlyMorningHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedEarlyMorningHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedEarlyMorningHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Morning",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(6) && d.WeatherHour < SelectedDate.AddHours(12)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodMorning"],
        //            TimeOfDayImage = morning,
        //            HourlyName = SelectedMorningHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedMorningHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedMorningHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedMorningHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Afternoon",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(12) && d.WeatherHour < SelectedDate.AddHours(18)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"],
        //            TimeOfDayImage = afternoon,
        //            HourlyName = SelectedAfternoonHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedAfternoonHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedAfternoonHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedAfternoonHourlyForecast.HourlyForecastSummary.StormSummary
        //         },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Evening",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(18) && d.WeatherHour < SelectedDate.AddHours(22)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEvening"],
        //            TimeOfDayImage = evening,
        //           HourlyName = SelectedEveningHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedEveningHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedEveningHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedEveningHourlyForecast.HourlyForecastSummary.StormSummary
        //        },
        //        new DayPeriod
        //        {
        //            TimeOfDayName = "Night",
        //            Time = SelectedDate,
        //            TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(22) && d.WeatherHour < SelectedDate.AddHours(24)).OrderBy(x => x.OrderTime)),
        //            TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodNight"],
        //            TimeOfDayImage = night,
        //            HourlyName = SelectedNightHourlyForecast.Name,
        //            TimeOfDaySummary = SelectedNightHourlyForecast.HourlyForecastSummary.WeatherSummary,
        //            TimeOfDayWeatherImage = SelectedNightHourlyForecast.SkHourlyMeteocon,
        //            TimeOfDayStormSummary = SelectedNightHourlyForecast.HourlyForecastSummary.StormSummary
        //        }
        //};
    }
}



//Bound properties that do not change with the select day
//[ObservableProperty]
//public partial bool IsWorking { get; set; }
//[ObservableProperty]
//public partial PhotoLocation SelectedLocation { get; set; }

//[ObservableProperty]
//public partial ObservableCollection<HeatRiskValue?> AllHeatRiskValues { get; set; }


//[ObservableProperty]
//public partial string SelectedSummary { get; set; }
//   [ObservableProperty]
//    public partial HourlySupplement SelectedHourlySupplement { get; set; }
//[ObservableProperty]
//public partial string selectedNwsNightSummary { get; set; }
//[ObservableProperty]
//public partial string SelectedSafetySummary { get; set; }
//[ObservableProperty]
//public partial HeatRiskValue SelectDayHeatRiskValue { get; set; }
//[ObservableProperty]
//public partial string SelectedDailyStormSummary { get; set; }
//[ObservableProperty]
//public partial string SelectedDayHeatRiskSummary { get; set; }
//[ObservableProperty]
//public partial HourlyForecast SelectedHigh { get; set; }
//[ObservableProperty]
//public partial string SelectedTimeHighTemp { get; set; }
//[ObservableProperty]
//public partial HourlyForecast SelectedLow { get; set; }
//[ObservableProperty]
//public partial string SelectedTimeLowTemp { get; set; }
//[ObservableProperty]
//public partial SKFileLottieImageSource SelectedDailyWeatherImage { get; set; }
//[ObservableProperty]
//public partial ImageSource selectedImageDay { get; set; }
//[ObservableProperty]
//public partial ImageSource? selectedImageNight { get; set; }

////[ObservableProperty]
////public partial Color SelectedUvRiskColor { get; set; }

//[ObservableProperty]
//public partial SKFileLottieImageSource DailyHighAnimation { get; set; }
//[ObservableProperty]
//public partial SKFileLottieImageSource DailyLowAnimation { get; set; }
//[ObservableProperty]
//public partial double High { get; set; }
//[ObservableProperty]
//public partial HourlyForecast HighTime { get; set; }
//[ObservableProperty]
//public partial double Low { get; set; }
//[ObservableProperty]
//public partial HourlyForecast LowTime { get; set; }



//private GetWeatherService _weatherService;
//private GetNwsData _nwsWeatherService;
//private GetEventService _eventService;
//private GetMoonService _moonService;

//        if (Session.WeatherForecasts?.Select(x => x.DailyForecasts.SelectedDailyHourlyForecasts) == null || Session.NwsData?.SelectedDailyHourlyForecasts == null) return;
//  LocationName = SharedSession.SelectedLocation.DisplayName;

//     MainThread.BeginInvokeOnMainThread(() => WeatherForecasts = new ObservableCollection<WeatherForecasts>(SharedSession.WeatherForecasts.ToList()));
//    MainThread.BeginInvokeOnMainThread(() => Dates = new ObservableCollection<DateItem>(SharedSession.Dates));
//WeatherForecasts.CollectionChanged += (s, e) => MainThread.BeginInvokeOnMainThread(() =>
//{
//    foreach (var forecast in SharedSession.WeatherForecasts.OrderBy(w => w.Time))
//    {
//        WeatherForecasts.Add(forecast);
//        Debug.WriteLine($"WeatherForecasts.CollectionChanged invoked on thread {Environment.CurrentManagedThreadId}; count={WeatherForecasts.Count}");
//        UpdateForDate(SelectedDate);
//    }
//});

//if (SharedSession.Dates == null)
//{
//    return;
//}

//var heatRisks = Session.NwsData.HourlySupplementals.Where(z => z.ForecastHeatRisk?.Value != null).Select(x => x.ForecastHeatRisk).ToArray();
//if (heatRisks.Length > 1)
//{
//    AllHeatRiskValues = new ObservableCollection<HeatRiskValue>(heatRisks);
//}



//    SelectedUvRiskColor = SelectedDaily?.DailyForecastSummary?.UVIndexColor ?? Colors.Green;

// SelectDayHeatRiskValue = AllHeatRiskValues?.Where(x => x.Time.Time == SelectedDate.Time).First() ?? new HeatRiskValue { Time = SelectedDate, Value = 0 };
//SelectedUvRiskColor = SelectedDaily?.DailyForecastSummary?.UVIndexColor ?? Colors.Green;

//SelectedSummary = string.Empty;
//SelectedSafetySummary = string.Empty;
//SelectedUvRiskColor = default;
//SelectedDailyStormSummary = string.Empty;
//SelectedDayHeatRiskSummary = "There is no Heat Risk assigned today.";
//SelectedHigh = null;
//SelectedLow = null;
//SelectedTimeHighTemp = string.Empty;
//SelectedTimeLowTemp = string.Empty;
//SelectedDailyWeatherImage = null;

//SelectedSummary = SelectedDaily?.DailyForecastSummary?.WeatherSummary ?? "There are no forecast layers to load";
//SelectedSafetySummary = SelectedDaily?.DailyForecastSummary?.HealthSafetySummary ?? "There are no health and safety layers to load";
//SelectedDailyStormSummary = SelectedDaily?.DailyForecastSummary?.StormSummary ?? "There are no storms to report.";
//SelectedDayHeatRiskSummary = Session.NwsData?.HourlySupplementals?.Where(h => h.Time.Time == SelectedDate.Time).Select(x => x.ForecastHeatRisk?.HeatRiskDescription).ToString() ?? "There is no Heat Risk assigned today.";


//SelectedEarlyHoursHourlyForecast = SelectedDaily.SelectedDailyHourlyForecasts.Where(x => x.HourlyTime.Time == SelectedDate.Time && x.TimeOfDay == "Early Hours" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
//SelectedEarlyMorningHourlyForecast = SelectedDaily.SelectedDailyHourlyForecasts.Where(x => x.HourlyTime.Time == SelectedDate.Time && x.TimeOfDay == "Early Morning" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
//SelectedMorningHourlyForecast = SelectedDaily.SelectedDailyHourlyForecasts.Where(x => x.HourlyTime.Time == SelectedDate.Time && x.TimeOfDay == "Morning" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
//SelectedAfternoonHourlyForecast = SelectedDaily.SelectedDailyHourlyForecasts.Where(x => x.HourlyTime.Time == SelectedDate.Time && x.TimeOfDay == "Afternoon" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
//SelectedEveningHourlyForecast = SelectedDaily.SelectedDailyHourlyForecasts.Where(x => x.HourlyTime.Time == SelectedDate.Time && x.TimeOfDay == "Evening" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
//SelectedNightHourlyForecast = SelectedDaily.SelectedDailyHourlyForecasts.Where(x => x.HourlyTime.Time == SelectedDate.Time && x.TimeOfDay == "Night" && x.WeatherCodeDescription != null).OrderByDescending(y => y.WeatherCode).FirstOrDefault();
//PeriodName = SelectedDaily.TimePeriod ?? string.Empty;
//SelectedSummary = SelectedDaily.DailyForecastSummary?.WeatherSummary ?? string.Empty;
//SelectedSafetySummary = SelectedDaily.DailyForecastSummary?.HealthSafetySummary ?? string.Empty;
//SelectedUvRiskColor = SelectedDaily.DailyForecastSummary?.UVIndexColor ?? default;
//SelectedDailyStormSummary = SelectedDaily.DailyForecastSummary?.StormSummary ?? string.Empty;
//SelectedDayHeatRiskSummary = AllHeatRiskValues?.Where(h => h.Time.Time == SelectedDate.Time).FirstOrDefault()?.HeatRiskDescription ?? "There is no Heat Risk assigned today.";

//High = SelectedDaily.Temperature2mMax.Value;
//HighTime = SelectedDaily?.SelectedDailyHourlyForecasts.Where(x => x.IsDay == 1 && x.Temperature2m.Value == High).OrderBy(t => t.HourlyTime).FirstOrDefault();
//Low = SelectedDaily.Temperature2mMin.Value;
//LowTime = SelectedDaily?.SelectedDailyHourlyForecasts.Where(x => x.IsDay == 0 && x.Temperature2m.Value == Low).OrderByDescending(t => t.HourlyTime).FirstOrDefault();
//SelectedTimeHighTemp = $"{Math.Round((double)High, 0)}{HighTime.Temperature2m.Unit} at {HighTime.HourlyTime.ToString("htt")}.";
//SelectedTimeLowTemp = $"{Math.Round((double)Low, 0)}{LowTime.Temperature2m.Unit} at {LowTime.HourlyTime.ToString("htt")}";

//SelectedHigh = SelectedDaily.SelectedDailyHourlyForecasts?.Where(t => t.IsDay == 1).OrderByDescending(h => h.Temperature2m?.Value ?? double.MinValue).FirstOrDefault();
//SelectedLow = SelectedDaily.SelectedDailyHourlyForecasts?.Where(t => t.IsDay == 0 && t.HourlyTime.Hour >= 12).OrderBy(c => c.Temperature2m?.Value ?? double.MaxValue).FirstOrDefault();

//SelectedTimeHighTemp = SelectedHigh != null && SelectedHigh.Temperature2m != null ? $"{Math.Round(SelectedHigh.Temperature2m.Value, 0)}{SelectedHigh.Temperature2m.Unit} at {SelectedHigh.HourlyTime.ToString("h tt")}." : string.Empty;
//SelectedTimeLowTemp = SelectedLow != null && SelectedLow.Temperature2m != null ? $"{Math.Round(SelectedLow.Temperature2m.Value, 0)}{SelectedLow.Temperature2m.Unit} at {SelectedLow.HourlyTime.ToString("h tt")}" : string.Empty;

// SelectedDailyWeatherImage = SelectedDaily.SkDailyMeteocon;



////private DateTime _selectedDate;
////public DateTime SelectedDate
////{
////    get => _selectedDate;D
////    set
////    {
////        if (SetProperty(ref _selectedDate, Value))
////        {
////            UpdateForDate(Value);
////        }
////    }
////}

////private DailyForecasts _selectedDaily;

////{
////    get => _selectedDaily;
////    set => SetProperty(ref _selectedDaily, Value);
////}


////{
////    get => _selectedDateItem;
////    set
////    {
////        if (_selectedDateItem != Value)
////        {
////            _selectedDateItem = Value;
////            OnPropertyChanged(nameof(SelectedDateItem));
////            // Run the async update without blocking the UI thread
////            SelectedDate = _selectedDateItem.Time;
////            IsWorking = true;
////            UpdateForDate(SelectedDateItem.Time);
////            IsWorking = false;
////        }
////    }
////}

//// Gradient brush representing the sky color
////private LinearGradientBrush _skyGradient = new LinearGradientBrush();
////public LinearGradientBrush SkyGradient
////{
////    get => field;
////    private set
////    {
////        field = Value;
////        OnPropertyChanged(nameof(SkyGradient));
////    }
////} = new LinearGradientBrush();

////[RelayCommand]
////private void ViewDate(object parameter)
////{
////    if (parameter is DateTime selectedDate)
////    {
////        IsWorking = true;
////        SelectedDate = selectedDate;
////        //SelectedDateItem = selectedDateItem;
////        //_selectedDateItem = SelectedDateItem;


////        _selectedDaily = WeatherForecasts.Where(x => x.Time.Time == SelectedDate.Time).Select(d => d.DailyForecasts).FirstOrDefault();
////        SelectedDaily = _selectedDaily;
////        var storms = Session.AdditionalWeatherDesc.Where(x => x.Time.Time == SelectedDate.Time).OrderBy(o => o.Time).Select(s => s.WeatherSummary).ToArray();
////        if (storms.Any())
////        {
////            SelectedDailyStormSummary = String.Join(" ", storms);
////        }
////        else
////        {
////            SelectedDailyStormSummary = "There are no storms to report.";
////        }
////        var heatrisk = Session.AllHeatriskDetails.Where(h => h.Time.Time == SelectedDate.Time).FirstOrDefault();
////        if (heatrisk != null)
////        {
////            SelectedDailyHeatrisk = $"There is a ForecastHeatRisk today, category {heatrisk.Heatrisk}. {heatrisk.HeatriskDescription}";
////        }
////        else
////        {
////            SelectedDailyHeatrisk = "There has been no ForecastHeatRisk issued today.";
////        }
////        SelectedDailyEvents = SelectedDaily.DailyEvents.EventTimes;
////        SelectedSummary = SelectedDaily.WeatherSummary;
////        SelectedSafetySummary = SelectedDaily.HealthSafetySummary;
////        SelectedImage = SelectedDaily.DailyIcon;
////        SelectedLowTemp = SelectedDaily.Temperature2mMin.Value;
////        SelectedTimeLowTemp = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.Temperature2m.Value == SelectedLowTemp).FirstOrDefault().HourlyTime.ToString("hh:mm tt");
////        SelectedHighTemp = SelectedDaily.Temperature2mMax.Value;
////        SelectedTimeHighTemp = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.Temperature2m.Value == SelectedHighTemp).FirstOrDefault().HourlyTime.ToString("hh:mm tt");
////        SelectedDayLength = $"{SelectedDaily.DailyEvents.AstronomicalData.DayLength.Hours} h {SelectedDaily.DailyEvents.AstronomicalData.DayLength.Minutes} m";
////        SelectedMoonPhase = SelectedDaily.DailyEvents.AstronomicalData.MoonPhase;
////        SelectedSunshine = $"{TimeSpan.FromSeconds(SelectedDaily.SunshineDuration.Value).Hours} h {TimeSpan.FromSeconds(SelectedDaily.SunshineDuration.Value).Minutes} m";
////        PeriodName = SelectedDaily.TimePeriod;

////        EventsByPeriod = new ObservableCollection<DayPeriod>
////           {

////            new DayPeriod
////            {
////                TimeOfDayName = "EarlyHours",
////                Time = SelectedDate,
////                TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.Time && d.WeatherHour < SelectedDate.AddHours(3)).OrderBy(x => x.OrderTime)),
////                TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyHours"],
////                TimeOfDayStormSummary = BuildStormSummaryByTimeOfDay(SelectedDate, "EarlyHours"),
////                Summary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 1).FirstOrDefault().WeatherSummary ?? "",
////                PrecipitationSummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 1).FirstOrDefault().PrecipitationSummary ?? "",
////                SafetySummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 1).FirstOrDefault().HealthSafetySummary ?? "",
////                TimeOfDayWeatherImage = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 1).FirstOrDefault().SkHourlyMeteocon ?? ""
////            },
////            new DayPeriod
////            {
////                TimeOfDayName = "Early Morning",
////                Time = SelectedDate,
////                TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(3) && d.WeatherHour < SelectedDate.AddHours(6)).OrderBy(x => x.OrderTime)),
////                TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"],
////                TimeOfDayStormSummary = BuildStormSummaryByTimeOfDay(SelectedDate, "Early Morning"),
////                Summary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 3).FirstOrDefault().WeatherSummary,
////                PrecipitationSummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 3).FirstOrDefault().PrecipitationSummary,
////                SafetySummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 3).FirstOrDefault().HealthSafetySummary,
////                TimeOfDayWeatherImage = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 3).FirstOrDefault().SkHourlyMeteocon
////            },
////            new DayPeriod
////            {
////                TimeOfDayName = "Morning",
////                Time = SelectedDate,
////                TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(6) && d.WeatherHour < SelectedDate.AddHours(12)).OrderBy(x => x.OrderTime)),
////                TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodMorning"],
////                TimeOfDayStormSummary = BuildStormSummaryByTimeOfDay(SelectedDate, "Morning"),
////                Summary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 6).FirstOrDefault().WeatherSummary,
////                PrecipitationSummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 6).FirstOrDefault().PrecipitationSummary,
////                SafetySummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 6).FirstOrDefault().HealthSafetySummary,
////                TimeOfDayWeatherImage = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 6).FirstOrDefault().SkHourlyMeteocon
////            },
////            new DayPeriod
////            {
////                TimeOfDayName = "Afternoon",
////                Time = SelectedDate,
////                TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(12) && d.WeatherHour < SelectedDate.AddHours(18)).OrderBy(x => x.OrderTime)),
////                TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"],
////                TimeOfDayStormSummary = BuildStormSummaryByTimeOfDay(SelectedDate, "Afternoon"),
////                Summary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 12).FirstOrDefault().WeatherSummary,
////                PrecipitationSummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 12).FirstOrDefault().PrecipitationSummary,
////                SafetySummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 12).FirstOrDefault().HealthSafetySummary,
////                TimeOfDayWeatherImage = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 12).FirstOrDefault().SkHourlyMeteocon
////            },
////            new DayPeriod
////            {
////                TimeOfDayName = "Evening",
////                Time = SelectedDate,
////                TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(18) && d.WeatherHour < SelectedDate.AddHours(22)).OrderBy(x => x.OrderTime)),
////                TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEvening"],
////                TimeOfDayStormSummary = BuildStormSummaryByTimeOfDay(SelectedDate, "Evening"),
////                Summary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 18).FirstOrDefault().WeatherSummary,
////                PrecipitationSummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 18).FirstOrDefault().PrecipitationSummary,
////                SafetySummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 18).FirstOrDefault().HealthSafetySummary,
////                TimeOfDayWeatherImage = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 18).FirstOrDefault().SkHourlyMeteocon
////            },
////            new DayPeriod
////            {
////                TimeOfDayName = "Night",
////                Time = SelectedDate,
////                TimeOfDayEvents = new ObservableCollection<EventTime>(SelectedDailyEvents.Where(d => d.WeatherHour >= SelectedDate.AddHours(22) && d.WeatherHour < SelectedDate.AddHours(24)).OrderBy(x => x.OrderTime)),
////                TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodNight"],
////                TimeOfDayStormSummary = BuildStormSummaryByTimeOfDay(SelectedDate, "Night"),
////                Summary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 22).FirstOrDefault().WeatherSummary,
////                PrecipitationSummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 22).FirstOrDefault().PrecipitationSummary,
////                SafetySummary = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 22).FirstOrDefault().HealthSafetySummary,
////                TimeOfDayWeatherImage = SelectedDaily.SelectedDailyHourlyForecasts.Where(h => h.HourlyTime.Time == SelectedDate && h.HourlyTime.Hour == 22).FirstOrDefault().HourlyIcon
////            }
////           };
////        IsWorking = false;
////    }
////    return;
////}

////[RelayCommand]
////private void ViewDate(object args)
////{
////    // CollectionView passes a SelectionChangedEventArgs: handle both possibilities.
////    if (args is SelectionChangedEventArgs sce && sce.CurrentSelection?.FirstOrDefault() is DateTime dt)
////    {
////        SelectedDate = dt;
////        return;
////    }
////    if (args is DateTime dt2)
////    {
////        SelectedDate = dt2;
////    }
////}
