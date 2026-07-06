using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace ThePhotographersForecast.Services
{
    public class ShortDateDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                // "ddd, MMM d" → Mon, Jul 6
                return dt.ToString("ddd, MMM d", CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Usually not needed for display-only binding
            if (value is string s &&
                DateTime.TryParseExact(s, "ddd, MMM d", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var dt))
            {
                return dt;
            }

            return null;
        }
    }
}
