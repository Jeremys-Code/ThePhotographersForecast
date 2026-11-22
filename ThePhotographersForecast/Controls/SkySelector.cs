using static ThePhotographersForecast.Services.TimeOfDayService;

namespace ThePhotographersForecast.Controls
{
    public static class SkySelector
    {
        public static (LinearGradientBrush gradient, ImageSource image) GetSky(TimeOfDay tod)
        {
            return tod switch
            {
                TimeOfDay.EarlyMorning => ((LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"], "sky_default.jpg"),
                TimeOfDay.Morning => ((LinearGradientBrush)Application.Current.Resources["PeriodMorning"], "sky_morning.jpg"),
                TimeOfDay.Afternoon => ((LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"], "sky_default.jpg"),
                TimeOfDay.Evening => ((LinearGradientBrush)Application.Current.Resources["PeriodEvening"], "sky_morning.jpg"),
                TimeOfDay.Night => ((LinearGradientBrush)Application.Current.Resources["PeriodNight"], "sky_night.png"),
                _ => ((LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"], "sky_default.jpg")
            };
        }

        static LinearGradientBrush GetBrush(string key)
            => (LinearGradientBrush)Shell.Current.Resources[key];
    }

}
