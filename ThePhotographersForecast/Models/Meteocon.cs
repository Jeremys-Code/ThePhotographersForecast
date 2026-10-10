using System.Collections.ObjectModel;

namespace ThePhotographersForecast.Models
{
    public static class Meteocon
    {
        //public string Name { get; set; }
        //public SKFileLottieImageSource Animation { get; set; }
        //public SKUriLottieImageSource Online { get; set; }

        public static async Task GetIniLottiesAsync()
        {
            //Extract initial lottie list and cast the file names to an array
            var stream = await FileSystem.Current.OpenAppPackageFileAsync("lottie.dat");
            var read = new StreamReader(stream);
            var contentList = new List<string>();
            using (var sr = new StreamReader(stream))
            {
                foreach (sr != null)
                {
                    contentList.Add(sr.ReadLine());
                    if (sr == null)
                        break;
                }
            }
            var contentArray = contentList.OrderBy(o => o).ToArray();
            //Set the folder in the cache directory where the lottie files will be called from
            var cachePath = Path.Combine(FileSystem.CacheDirectory, "meteocons");
            //Iterate through the list of Lotties
            for (int r = 0; r < contentArray.Length; r++)
            {
                var cacheFile = Path.Combine(cachePath, contentList[r]);
                content += read[r];
            }
            using (string content = read.ReadLine)
            {

            }
            var content = await read.ReadLineAsync().Result.ToList();
            var list = content.OrderBy(o => o).ToArray<string>();


            for (int i = 0; i < list.Length; i++)
            {
                var stream = await FileSystem.OpenAppPackageFileAsync(list[i]);
                var read = new StreamReader(stream).ReadToEnd().ToCharArray();
                var content = await read.ReadToEndAsync();
                File.WriteAllText(cachePath + list[i], content);
            }
        }

    }
}