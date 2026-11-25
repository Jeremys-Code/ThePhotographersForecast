using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    public partial class GetDays : ObservableObject, IGetDays
    {
        private ISkyColorEngine _skyEngine;
        private ISession _session;
        private IForecastService _forecastService;
        public GetDays(ISession session, ISkyColorEngine skyEngine, IForecastService forecastService)
        {
            _session = session;
            _skyEngine = skyEngine;
            _forecastService = forecastService;
        }
        public async Task<ObservableCollection<DayViewModel>> GetDaysAsync()
        {
            //Days = new ObservableCollection<DayPeriod>();
            if (Application.Current == null) return default;
            var days = new ObservableCollection<DayViewModel>();

            try
            {
                for (int d = 0; d < _session.Dates.Length; d++)
                {
                    var date = _session.Dates[d].Date;
                    //days.Add(new DayViewModel
                    //{
                    //    Date = date,
                    //    Day = date.Day,
                    //    DailyForecasts = _session.NwsData.DailyForecasts.ToArray(),
                    //    IsEventDay = true,
                    //    Events = _session.EventsData.Where(x => x.Date.Date == date.Date).First(),
                    //    TimeOfDayForecasts = _session.NwsData.TimeOfDayForecasts.Where(x => x.StartTime.Date == date.Date).ToArray(),
                    //    CurrentForecast = _session.NwsData.HourlyForecasts.First(),


                    //});


                    //var date = day.Key;
                    //var dayInfo = day.ToArray();
                    //var dayGroups = dayInfo.GroupBy(x => x.TimeOfDay);
                    //if (dayInfo != null)
                    //{
                    //    for (int p = 0; p < dayInfo.Length; p++)
                    //    {
                    var selectedDate = date.Date;
                    var hl = GetDailyHighLow(date.Date);
                    days.Add(new DayViewModel
                    {
                        Date = selectedDate,
                        Day = selectedDate.Day,
                        Events = _session.EventsData.Where(e => e.Date == selectedDate.Date).FirstOrDefault<EventModel>() ?? new EventModel(),
                        IsEmpty = false,
                        IsEventDay = true,
                        IsFlipped = false,
                        IsToday = selectedDate.Date == DateTime.UtcNow.AddSeconds(_session.SelectedTimeZoneOffset).Date,
                        SelectedLocation = new PhotoLocation(_session.NwsData.SelectedLocation),
                        Name = _session.NwsData.TimeOfDayForecasts.FirstOrDefault().TimeOfDayName,
                        Periods = CreatePeriods(_session.NwsData.TimeOfDayForecasts.Where(x => x.StartTime.Date == date.Date).ToArray()),
                        //            SelectedDailyForecast = dayInfo[p],
                        TimeOfDayForecasts = _session.NwsData.TimeOfDayForecasts.Where(x => x.StartTime.Date == date.Date).ToArray(),
                        DailyDetailedForecast = _session.NwsData.TimeOfDayForecasts.Where(x => x.StartTime.Date == date.Date).FirstOrDefault().LongForecast,
                        //DailyScore = _session.DailyPhotoScoringSummary.Where(x => x.Date == DateOnly.FromDateTime(selectedDate)).FirstOrDefault().HumanReadableDailySummary ?? "Missing Summary!!!",
                        DailyHighStatement = hl.high,
                        DailyLowStatement = hl.low,
                        DailySkyBrush = _session.NwsData.TimeOfDayForecasts.Where(x => x.IsDay == true).Any() ? (LinearGradientBrush)Application.Current.Resources["Dusk"] : (LinearGradientBrush)Application.Current.Resources["Night"]
                    });


                    // This requires the method called on line

                    //Debug.WriteLine($"Completed Day for {days[p].Name} {days[p].Date.ToString("F")}");
                    return days;
                }//Finish processing all days
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing Days: {ex}");
            }
            return days;
        }

        private string CreateDailyDescription(NwsDailyForecast daily)
        {
            var desc = new StringBuilder();
            var highOrLow = daily.IsDaytime;
            desc.Append(daily.ShortForecast);
            if (highOrLow)
            {
                desc.Append($", with a high around {daily.TempAndUnit}.");
            }
            else
            {
                desc.Append($", with a low of {daily.TempAndUnit}.");
            }
            desc.Append(daily.WindGusts != null && daily.WindGusts.GustStatement != null ? daily.WindSpeedStatement + daily.WindGusts.GustStatement : daily.WindSpeedStatement + ".");
            return desc.ToString();
        }
        private ObservableCollection<DayPeriod> CreatePeriods(NwsTimeOfDayForecast[] timeOfDayForecast)
        {
            var dh = _session.NwsData.TimeOfDayForecasts;
            var periods = new ObservableCollection<DayPeriod>();
            try
            {

                for (int t = 0; t < timeOfDayForecast.Length; t++)
                {
                    var lotties = new ObservableCollection<LottieAnimation>();
                    for (int l = 0; l < timeOfDayForecast[t].Forecasts.Length; l++)
                    {
                        var animation = new LottieAnimation
                        {
                            LottieWeather = timeOfDayForecast[t].Forecasts[l].SkHourlyAnimation,
                            Forecast = timeOfDayForecast[t].Forecasts[l].ShortForecast,
                            Time = timeOfDayForecast[t].Forecasts[l].StartTime,
                            Temperature = timeOfDayForecast[t].Forecasts[l].TempAndUnit
                        };
                        lotties.Add(animation);
                    }
                    var dp = new DayPeriod
                    {
                        TimeOfDayName = timeOfDayForecast[t].TimeOfDay,
                        StartTime = timeOfDayForecast[t].StartTime,
                        EndTime = timeOfDayForecast[t].EndTime,
                        ForecastName = timeOfDayForecast[t].TimeOfDayName,
                        TimeOfDayBackgroundBrush = timeOfDayForecast[t].TimeOfDayGradientBrush,
                        TimeOfDaySkAnimation = timeOfDayForecast[t].SKTimeOfDayAnimation,
                        TimeOfDayForecast = timeOfDayForecast[t].LongForecast,
                        TimeOfDaySkWeatherAnimations = lotties,
                        SelectedTimeOfDaySkWeatherAnimation = lotties.FirstOrDefault(),
                        HourlyWeather = timeOfDayForecast[t].Forecasts.ToArray(),
                        TimeOfDayEvents = _session.EventTimes.Where(x => timeOfDayForecast[t].StartTime <= x.WeatherHour && x.WeatherHour <= timeOfDayForecast[t].EndTime).Select(e => e).ToObservableCollection(),
                        TimeOfDayTemperature = _forecastService.TempCategory(timeOfDayForecast[t].MaxTemp, timeOfDayForecast[t].DewPointAvg, timeOfDayForecast[t].RelativeHumidityAvg, timeOfDayForecast[t].Forecasts.Select(x => x.Supplemental.WetBulbGlobeTemperature.Value).Average()),
                    };
                    periods.Add(dp);
                }

                return periods;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Building Day Period{timeOfDayForecast.First().StartTime}{ex}");
                return default;
            }

        }

        private string GetStormSummary(NwsHourlyForecast dh)
        {
            if (dh.Supplemental?.SupplementalWeatherStatement == null) return "There are no severe weather warnings for this time period.";
            var summary = dh.Supplemental.SupplementalWeatherStatement;
            if (string.IsNullOrEmpty(summary))
            {
                Debug.WriteLine("Missing supplemental.Weather for hourly forecast at {Time}", dh.StartTime);
                return "There are no severe weather warnings for this time period.";
            }
            return summary;
        }
        private (string high, string low) GetDailyHighLow(DateTime date)
        {
            var highRecord = _session.NwsData.HourlyForecasts.Where(x => x.StartTime.Date == date.Date && x.Temperature.Value != null).MaxBy(y => y.Temperature.Value);
            var lowRecord = _session.NwsData.HourlyForecasts.Where(x => x.StartTime.Date == date.Date && x.Temperature.Value != null).MinBy(z => z.Temperature.Value);
            string high = string.Empty;
            string low = string.Empty;
            if (highRecord == null)
            {
                high = $"The high temperature{Environment.NewLine}is not available.";
            }
            else
            {
                high = $"High of {highRecord.TempAndUnit}{Environment.NewLine}@ {highRecord.StartTime:h tt}.";
            }

            if (lowRecord == null)
            {
                low = $"The Low temperature{Environment.NewLine}is not available.";
            }
            else
            {
                low = $"Low of {lowRecord.TempAndUnit}{Environment.NewLine}@ {lowRecord.StartTime:h tt}.";
            }

            return (high, low);
        }
    }
}
