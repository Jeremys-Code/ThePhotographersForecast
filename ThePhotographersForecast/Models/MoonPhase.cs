using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models
{
    public class FutureMoonPhase
    {
        public DateTime UtcDateTime { get; set; }
        public DateTime LocalDateTime { get; set; }
        public string LocalDateString => LocalDateTime.ToString("ddd, MMM d, yyyy");
        public string LocalTimeString => LocalDateTime.ToString("hh:mm tt");
        public required string MoonPhaseName { get; set; }
        public ImageSource MoonImage => GetMoonImages();
        public SKFileLottieImageSource SkMoonSource => GetSkMoon();

        private ImageSource GetMoonImages()
        {
            var modifyPhaseName = this.MoonPhaseName.ToLowerInvariant().Replace(" ", "") + ".png";
            ImageSource moonSource = ImageSource.FromFile(modifyPhaseName);
            return moonSource;
        }

        private SKFileLottieImageSource GetSkMoon()
        {
            var lottieFile = this.MoonPhaseName.ToLowerInvariant().Replace(" ", "_") switch { "first_quarter" => "moon_first_quarter.json", "full_moon" => "moon_full.json", "last_quarter" => "moon_last_quarter.json", "waning_cresent" => "moon_waning_cresent.json", "waning_gibbous" => "moon_waning_gibbous.json", "waxing_cresent" => "moon_waxing_cresent.json", "waxing_gibbous" => "moon_waxing_gibbous.json", "new_moon" => "moon_new.json", _ => "not_available.json" };
            var fileLocation = Path.Combine(FileSystem.CacheDirectory, "meteocons", lottieFile);
            if (File.Exists(fileLocation))
                return new SKFileLottieImageSource { File = fileLocation };

            Task.Run(async () =>
            {
                var stream = await FileSystem.OpenAppPackageFileAsync(lottieFile);
                var read = new StreamReader(stream);
                await File.WriteAllTextAsync(fileLocation, await read.ReadToEndAsync());
                return new SKFileLottieImageSource { File = fileLocation };
            });
            return new SKFileLottieImageSource { File = "unavailable.json" };
        }

    }
}
