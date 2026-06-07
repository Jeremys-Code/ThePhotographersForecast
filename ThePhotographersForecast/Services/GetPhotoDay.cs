using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Services
{
    public partial class GetPhotoDay : ObservableObject
    {
        [ObservableProperty]
        private PhotoDay[] photoDays;
        public GetPhotoDay()
        {
            var nbrDays = ServiceHelper.GetService<SelectLocationViewModel>().NumberOfPhotoDays;
            if (nbrDays == 0)
            {
                var day = new PhotoDay { IsEmpty = true };
                PhotoDays[0] = day;
            }

            PhotoDays = new PhotoDay[nbrDays];
        }
        public async Task<PhotoDay[]> BuildPhotoDayAsync()
        {

            if (ServiceHelper.GetService<SelectLocationViewModel>().GfsData.DailyForecasts.Length == ServiceHelper.GetService<SelectLocationViewModel>().ForecastCount && ServiceHelper.GetService<SelectLocationViewModel>().EventsModel.EventTimes.Count == ServiceHelper.GetService<SelectLocationViewModel>().EventCount)
            {

                //if (ServiceHelper.GetService<CalendarViewModel>().IsWorking == false)
                //{
                //    MainThread.BeginInvokeOnMainThread(() =>
                //    {
                //        ServiceHelper.GetService<CalendarViewModel>().IsWorking = true;
                //    });
                //}


                var start = ServiceHelper.GetService<SelectLocationViewModel>().PhotoStartDate;

                await Parallel.ForAsync(0, PhotoDays.Length, async (d, cancellationToken) =>
                {
                    var photoDate = start.AddDays(d);


                    var photoDay = new PhotoDay();
                    photoDay.Day = d;
                    photoDay.Date = photoDate;
                    photoDay.IsEventDay = true;
                    photoDay.IsEmpty = false;
                    photoDay.IsFlipped = false;
                    var dailyForecast = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.DailyForecasts.Where(w => w.StartTime.Date == start.Date).OrderBy(d => d.StartTime).ToArray();
                    photoDay.DailyForecasts = new GfsDailyForecast[dailyForecast.Length];
                    for (int a = 0; a < dailyForecast.Length; a++)
                    {
                        photoDay.DailyForecasts[a] = dailyForecast[a];
                    }


                    var hourlyForecast = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.HourlyForecasts.Where(w => w.StartTime.Date == start.Date).OrderBy(d => d.StartTime).ToArray();
                    photoDay.HourlyForecasts = new GfsHourlyForecast[hourlyForecast.Length];
                    for (int b = 0; b < hourlyForecast.Length; b++)
                    {
                        photoDay.HourlyForecasts[b] = hourlyForecast[b];
                    }

                    //photoModel.PhotoDays[d]. = new GfsDailyForecast[dailyForecast.Length];
                    //Array.Copy(dailyForecast, photoModel.PhotoDays[d].DailyForecasts, dailyForecast.Length);
                    var eventData = ServiceHelper.GetService<SelectLocationViewModel>().EventsModel.EventTimes.Where(e => e.WeatherHour.Date == start.AddDays(d).Date).OrderBy(e => e.OrderTime).ToObservableCollection();
                    var hourlyForecasts = ServiceHelper.GetService<SelectLocationViewModel>().GfsData.HourlyForecasts.Where(g => g.StartTime.Date == start.AddDays(d).Date).OrderBy(w => w.StartTime).ToArray();
                    photoDay.PhotoHours = new PhotoHour[hourlyForecasts.Length];
                    await Parallel.ForAsync(0, hourlyForecasts.Length, async (h, cancellationToken) =>
                     {
                         photoDay.PhotoHours[h] = new PhotoHour
                         {
                             Hour = hourlyForecasts[h].StartTime,
                             TimeOfDay = FromTimesOfDay(hourlyForecasts[h].StartTime.Hour),
                             Hourly = new Hourly
                             {
                                 StartTime = hourlyForecasts[h].StartTime,
                                 EndTime = hourlyForecasts[h].EndTime,
                                 HourlyForecast = hourlyForecasts[h].DetailForecast,
                                 HourlyIcon = hourlyForecasts[h].HourlyIcon
                             },
                             Events = eventData.Where(e => e.WeatherHour == hourlyForecasts[h].StartTime).OrderBy(x => x.OrderTime).ToObservableCollection()
                         };
                     });
                    PhotoDays[d] = photoDay;
                });


                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                //    ServiceHelper.GetService<CalendarViewModel>().IsWorking = false;
                //});

                return PhotoDays;

            }
            else
            {
                var day = new PhotoDay { IsEmpty = true };
                PhotoDays[0] = day;
                return PhotoDays;
            }

            string FromTimesOfDay(int hour)
            {
                switch (hour)
                {
                    case int h when (0 <= h && h < 3):
                        return "Night";
                    case int h when (3 <= h && h < 6):
                        return "Early Morning";
                    case int h when (6 <= h && h < 12):
                        return "Morning";
                    case int h when (12 <= h && h < 18):
                        return "Afternoon";
                    case int h when (18 <= h && h < 22):
                        return "Evening";
                    case int h when (22 <= h && h < 24):
                        return "Night";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
