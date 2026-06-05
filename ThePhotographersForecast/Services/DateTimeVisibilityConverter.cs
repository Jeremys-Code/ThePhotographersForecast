using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace ThePhotographersForecast.Services; // Replace with your actual namespace

public class DateTimeVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the value is a DateTime and not equal to MinValue
        if (value is DateTime dateTime)
        {
            return dateTime != DateTime.MinValue;
        }

        // If it's null or not a DateTime, hide it just to be safe
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // ConvertBack is not needed for OneWay bindings like IsVisible
        throw new NotImplementedException();
    }
}