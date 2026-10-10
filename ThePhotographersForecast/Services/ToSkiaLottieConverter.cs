using SkiaSharp.Extended.UI.Controls;
using System.Globalization;

namespace ThePhotographersForecast.Services
{
    public class ToSkiaLottieConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            // Already a SKFileLottieImageSource, return it directly if the target type is compatible
            if (value is SKFileLottieImageSource lottie)
            {
                if (targetType.IsAssignableFrom(lottie.GetType()))
                    return lottie;

                // Recreate a new array instance to avoid runtime type identity issues
                return new SKFileLottieImageSource { File = lottie.File };
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}