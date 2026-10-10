namespace ThePhotographersForecast.Services;

// Implementation: var json = await JsonResourceLoader.LoadJsonAsync("myfile.json");

public static class JsonResourceLoader
{
    public static async Task<Stream> LoadJsonAsync(string filename)
    {
        var assembly = typeof(JsonResourceLoader).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith(filename, StringComparison.OrdinalIgnoreCase));

        return assembly.GetManifestResourceStream(resourceName);
    }
}
