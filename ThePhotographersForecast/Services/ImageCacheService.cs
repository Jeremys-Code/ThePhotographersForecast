namespace ThePhotographersForecast.Services
{
    public class ImageCacheService : IImageCacheService
    {
        private readonly HttpClient _client;
        private Char[] rand = new char[16] { 'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public ImageCacheService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("photographer") ?? new HttpClient();
        }
        public async Task<ImageSource> GetCachedFileAsync(string url)
        {
            var urlong = url.Split('/').ToArray();
            string name = string.Empty;
            foreach (var u in urlong.Skip(4)) { name += u.Replace(",", ""); }
            ;
            name = name.Replace("?size=", "");
            var cache = FileSystem.Current.CacheDirectory;
            var cacheResults = Directory.EnumerateFiles(cache, name, SearchOption.TopDirectoryOnly);
            if (cacheResults.Any())
            {
                return cacheResults.First();
            }
            else
            {
                try
                {
                    //Local file path
                    var imagePath = Path.Combine(cache, name);

                    // Download the image as a byte[]
                    var result = await _client.GetByteArrayAsync(url);

                    // Write the byte[] out to a physical file location and then make it an ImageSource
                    File.AppendAllBytes(imagePath, result);
                    ImageSource imageSource = ImageSource.FromFile(imagePath);

                    // Return the FromFile ImageSource
                    return imageSource;

                }
                catch (Exception)
                {
                    // If anything goes wrong return the unknown image
                    ImageSource imageSource = ImageSource.FromFile("unknown.png");
                    return imageSource;

                }
            }

        }

    }
}
