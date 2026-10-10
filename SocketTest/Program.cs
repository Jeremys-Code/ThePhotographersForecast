// csharp
using System.Net.Sockets;

async Task Test()
{
    try
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0 (debug)");
        var url = "https://api.weather.gov/points/42.3601,-71.0589";
        Console.WriteLine($"GET {url}");
        var resp = await client.GetAsync(url);
        Console.WriteLine("Status: " + resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine(body.Length > 1000 ? body.Substring(0, 1000) : body);
    }
    catch (Exception ex)
    {
        Console.WriteLine("=== Exception ===");
        Console.WriteLine(ex.ToString());
        if (ex is HttpRequestException hre && hre.InnerException != null)
            Console.WriteLine("=== InnerHttpRequestException ===n" + hre.InnerException.ToString());
        if (ex.InnerException is SocketException se)
        {
            Console.WriteLine("=== SocketException ===");
            Console.WriteLine($"SocketErrorCode: {se.SocketErrorCode}");
            Console.WriteLine($"Message: {se.Message}");
        }
        Console.WriteLine("OS: " + Environment.OSVersion);
    }
}

await Test();
