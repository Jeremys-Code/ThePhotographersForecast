using SkiaSharp.Extended.UI.Controls;
using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Services
{
    //public static class ServiceHelper
    //{
    //    public static IServiceProvider? Services { get; private set; }

    //    public static void Initialize(IServiceProvider serviceProvider) =>
    //        Services = serviceProvider;

    //    public static T GetService<T>() => Services.GetService<T>();
    //}
    public static class Helper
    {
        public static SKFileLottieImageSource GetLottieFromRawResources(string assetName)
        {
            return new SKFileLottieImageSource { File = assetName };
        }

        
        //    try
        //    {
        //        using var outputStream = await FileSystem.Current.OpenAppPackageFileAsync(assetName);
        //        using FileStream inputStream = File.Create(cacheFile);

        //        await outputStream.CopyToAsync(inputStream);
        //        return new SKFileLottieImageSource { File = cacheFile };
        //    }
        //    catch (Exception ex)
        //    {

        //        Debug.WriteLine($"The GetLottieFromRawResources Asset Go-Getter, failed to find the file requested.{assetName}");
        //        Debug.WriteLine(ex);
        //    }

        //    return new SKFileLottieImageSource { File = Path.Combine(FileSystem.Current.CacheDirectory, "meteocons", "unavailable.json") };
        //}

    }
}