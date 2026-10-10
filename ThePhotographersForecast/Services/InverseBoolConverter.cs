using System.Globalization;

namespace ThePhotographersForecast.Services
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object Value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Value is bool boolValue)
            {
                return !boolValue;
            }
            return Value;
        }

        public object ConvertBack(object Value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Value is bool boolValue)
            {
                return !boolValue;
            }
            return Value;
        }
    }
}
