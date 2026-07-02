
namespace ThePhotographersForecast.Services
{
    public class TimeConverter
    {
        public object Convert(object value, Type targetType, object parameter)
        {
            if (value is DateTime time)
                return time.ToString("F");
            return string.Empty; // or return null if you prefer no output
        }

        public object ConvertBack(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }

        public static DateTime UtcStringToLocal(string validTime, TimeZoneInfo timeZone)
        {
            DateTime time = DateTime.Parse(validTime);
            if (timeZone.IsDaylightSavingTime(time))
            {
                return time.Add(timeZone.GetAdjustmentRules()[^1].DaylightDelta);
            }
            return time;
        }
    }
}

