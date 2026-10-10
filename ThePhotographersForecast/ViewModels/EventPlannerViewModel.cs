using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class EventPlannerViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial Session SharedSession { get; set; }
        //private readonly ISkyColorEngine _skyEngine;
        //[ObservableProperty]
        //public partial string LocationName { get; set; }
        [ObservableProperty]
        public partial DayViewModel[] Days { get; set; }
        [ObservableProperty]
        public partial DayViewModel SelectedDay { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast[] SelectedDayHourlyForecasts { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<DayPeriod> SelectedDayDayPeriods { get; set; }
        [ObservableProperty]
        public partial LottieParade[] SelectedDayDayPeriodsParade { get; set; }
        [ObservableProperty]
        public partial EventModel SelectedDayEventModel { get; set; }
        [ObservableProperty]
        public partial EventTime[] SelectedDayEventTimes { get; set; }
        [ObservableProperty]
        public partial EventTime SelectedDayEventTime { get; set; }
        [ObservableProperty]
        public partial DailyForecast SelectedDayAdditionalMetrics { get; set; }
        [ObservableProperty]
        public partial HourlyForecast[] SelectedDayAdditionalMetricsHourlyForecasts { get; set; }
        [ObservableProperty]
        public partial ForecastSummaries SelectedDayAdditionalMetricsForecastSummary { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedHigh { get; set; }
        [ObservableProperty]
        public partial string SelectedTimeHighTemp { get; set; }
        [ObservableProperty]
        public partial NwsHourlyForecast SelectedLow { get; set; }
        [ObservableProperty]
        public partial string SelectedTimeLowTemp { get; set; }
        public static DateTime MinDate => DateTime.Now.Date;
        public static DateTime MaxDate => DateTime.Now.AddDays(15).Date;
        [ObservableProperty]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        public partial DateTime EndDate { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<CalendarDayModel> Dates { get; set; }
        [ObservableProperty]
        public partial DateTime SelectedDate { get; set; }
        [ObservableProperty]
        public partial string Instructions { get; set; }
        [ObservableProperty]
        public partial string LocationName { get; set; }
        [ObservableProperty]
        public partial string ElevationStatement { get; set; }
        [ObservableProperty]
        public partial string DailyHigh { get; set; }
        [ObservableProperty]
        public partial string DailyLow { get; set; }
        [ObservableProperty]
        public partial bool IsWorking { get; set; }
        public EventPlannerViewModel(Session sharedSession)
        {
            SharedSession = sharedSession;
            MainThread.BeginInvokeOnMainThread(async () =>
            {

                Days = SharedSession.Days.ToArray();

                StartDate = SharedSession.StartDate;
                EndDate = SharedSession.EndDate;
                Dates = SharedSession.Dates.ToObservableCollection();
                SelectedDate = Dates[0].Date;
                //while (Days.Length != Session.NwsData.DailyForecasts.Length)
                //{
                //    IsWorking = true;
                //    bool isCompleted = Task.Delay(100).GetAwaiter().IsCompleted;
                //}

                if (Days == null) return;
                SelectedDay = Days[0];
                SelectedDayHourlyForecasts = SharedSession.NwsData.HourlyForecasts.Where(d => d.StartTime.Date == SelectedDate.Date).ToArray();
                LocationName = SharedSession.SelectedLocation.DisplayName;
                ElevationStatement = SharedSession.SelectedLocation.Elevation.ElevationStatement;
                Instructions = $"Here are the daily astronomical events and the forecast from {StartDate.ToString("F")} through {EndDate.ToString("F")} in {SharedSession.SelectedLocation.DisplayName}.";
                SelectedDayEventModel = SharedSession.EventsData[0];
                SelectedDayEventTimes = SelectedDayEventModel.EventTimes;
                SelectedDayEventTime = SelectedDayEventTimes[0];
                SelectedDayAdditionalMetrics = SharedSession.WeatherForecast.DailyForecasts[0];
                SelectedDayAdditionalMetricsHourlyForecasts = SelectedDayAdditionalMetrics.HourlyForecasts;
                SelectedDayAdditionalMetricsForecastSummary = SelectedDayAdditionalMetrics.DailyForecastSummary;
                var high = SharedSession.NwsData.HourlyForecasts.Where(x => x.StartTime.Date == SelectedDate.Date).MaxBy(h => h.Temperature.Value);
                DailyHigh = $"{high.TempAndUnit} at {high.StartTime.ToString("h tt")}";
                var low = SharedSession.NwsData.HourlyForecasts.Where(x => x.StartTime.Date == SelectedDate.Date).MinBy(h => h.Temperature.Value);
                DailyLow = $"{low.TempAndUnit} at {low.StartTime.ToString("h tt")}";


            });

        }

        partial void OnSelectedDateChanged(DateTime Value)
        {
            SelectedDay = Days.Where(d => d.Date == SelectedDate).FirstOrDefault();
            LocationName = SharedSession.SelectedLocation.DisplayName;
            ElevationStatement = SharedSession.SelectedLocation.Elevation.ElevationStatement;
            Instructions = $"Here are the daily astronomical events and the forecast from {StartDate.ToString("F")} through {EndDate.ToString("F")} in {SharedSession.SelectedLocation.DisplayName}.";
            SelectedDayEventModel = SharedSession.EventsData.Where(e => e.Date == DateOnly.FromDateTime(SelectedDate)).FirstOrDefault<EventModel>();
            SelectedDayEventTimes = SelectedDayEventModel.EventTimes;
            SelectedDayEventTime = SelectedDayEventTimes[0];
            SelectedDayAdditionalMetrics = SharedSession.WeatherForecast.DailyForecasts.Where(w => w.Date == DateOnly.FromDateTime(SelectedDate)).FirstOrDefault();
            SelectedDayAdditionalMetricsHourlyForecasts = SelectedDayAdditionalMetrics.HourlyForecasts;
            SelectedDayAdditionalMetricsForecastSummary = SelectedDayAdditionalMetrics.DailyForecastSummary;
            //      SelectedDayDayPeriods = SharedSession.Periods;
            //     SelectedDayDayPeriodsParade = SelectedDayDayPeriods.SelectMany(p => p.LottieParades).OrderBy(o => o.Time).ToArray();
            var hourlies = SharedSession.NwsData.HourlyForecasts.Where(h => h.StartTime.Date == SelectedDate).ToArray();
            var high = hourlies.MaxBy(h => h.Temperature.Value);
            DailyHigh = $"{high.TempAndUnit} at {high.StartTime.ToString("h tt")}";
            var low = hourlies.MinBy(h => h.Temperature.Value);
            DailyLow = $"{low.TempAndUnit} at {low.StartTime.ToString("h tt")}";
        }
    }
}
