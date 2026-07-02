using System.Globalization;

namespace ThePhotographersForecast.Services
{
    public class PickerNullToBoolConverter : IValueConverter
    {
        // Returns true when SelectedIndex == -1 (no selection)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int idx)
                return idx == -1;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
