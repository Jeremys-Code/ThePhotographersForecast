using static ThePhotographersForecast.Services.TimeOfDayService;

namespace ThePhotographersForecast.Controls
{
    public static class SkySelector
    {
        public static (LinearGradientBrush gradient, ImageSource image) GetSky(TimeOfDay tod)
        {
            return tod switch
            {
                TimeOfDay.EarlyMorning => (GetBrush("PeriodEarlyMorning"), "sky_deffault.jpg"),
                TimeOfDay.Morning => (GetBrush("PeriodMorning"), "sky_morning.jpg"),
                TimeOfDay.Afternoon => (GetBrush("PeriodAfternoon"), "sky_default.jpg"),
                TimeOfDay.Evening => (GetBrush("PeriodEvening"), "sky_morning.jpg"),
                TimeOfDay.Night => (GetBrush("PeriodNight"), "sky_night.jpg"),
                _ => (GetBrush("PeriodEarlyMorning"), "sky_default.jpg")
            };
        }

        static LinearGradientBrush GetBrush(string key)
            => (LinearGradientBrush)Shell.Current.Resources[key];
    }

}
