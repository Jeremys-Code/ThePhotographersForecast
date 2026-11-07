using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    public partial class GetDays : ObservableObject, IGetDays
    {
        private ISession _session;
        public GetDays(ISession session)
        {
            _session = session;
        }
        public async Task<ObservableCollection<DayViewModel>> GetDaysAsync()
        {
            //Days = new ObservableCollection<DayPeriod>();
            if (Application.Current == null) return default;
            var days = new ObservableCollection<DayViewModel>();

            try
            {
                NwsDailyForecast[] dailyForecasts = _session.NwsData.DailyForecasts.ToArray();
                if (dailyForecasts != null)
                {
                    // Something is broke below
                    for (int p = 0; p < dailyForecasts.Length; p++)
                    {
                        var timeOfDays = dailyForecasts[p].TimeOfDayForecasts.ToArray();

                        var selectedDate = dailyForecasts[p].StartTime;
                        var hl = GetDailyHighLow(selectedDate);
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
                            Name = _session.NwsData.DailyForecasts[p].Name,
                            Periods = CreatePeriods(timeOfDays),
                            SelectedDailyForecast = _session.NwsData.DailyForecasts[p],
                            TimeOfDayForecasts = _session.NwsData.DailyForecasts[p].TimeOfDayForecasts,
                            DailyDetailedForecast = CreateDailyDescription(_session.NwsData.DailyForecasts[p]),
                            DailyHighStatement = hl.high,
                            DailyLowStatement = hl.low,
                            DailySkyBrush = _session.NwsData.DailyForecasts[p].IsDaytime == true ? _session.NwsData.DailyForecasts[p].SkyColor ?? (LinearGradientBrush)Application.Current.Resources["GpsBackground"] : (LinearGradientBrush)Application.Current.Resources["NightBackground"]


                        });


                        // This requires the method called on line

                        Debug.WriteLine($"Completed Day for {days[p].Name} {days[p].Date.ToString("F")}");
                    }//Finish processing all days
                }
                return days;

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
            // var dh = _session.NwsData.TimeOfDayForecasts;
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
                        TimeOfDayForecast = String.Join(". ", timeOfDayForecast[t].TimeOfDayForecast),
                        TimeOfDaySkWeatherAnimations = lotties,
                        SelectedTimeOfDaySkWeatherAnimation = lotties.FirstOrDefault(),
                        HourlyWeather = timeOfDayForecast[t].Forecasts.ToArray(),
                        TimeOfDayEvents = new ObservableCollection<EventTime>(timeOfDayForecast[t].TimeOfDayEvents),
                        TimeOfDayTemperature = timeOfDayForecast[t].TimeOfDayTemperature
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
