using System.Globalization;

namespace ThePhotographersForecast.Services; // Replace with your actual namespace

public class DateTimeVisibilityConverter
{
    public object Convert(object Value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the Value is a DateTime and not equal to MinValue
        if (Value is DateTime dateTime)
        {
            return dateTime != DateTime.MinValue;
        }

        // If it's null or not a DateTime, hide it just to be safe
        return false;
    }

    public object ConvertBack(object Value, Type targetType, object parameter, CultureInfo culture)
    {
        // ConvertBack is not needed for OneWay bindings like IsVisible
        throw new NotImplementedException();
    }
}