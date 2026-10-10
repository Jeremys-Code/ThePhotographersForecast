using CommunityToolkit.Maui;

namespace NationWeatherService
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddHttpClient<INwsService, NwsService>();
            builder.Services.AddSingleton<MainPage>();
#if DEBUG
            // builder.Logging.
#endif

            return builder.Build();
        }
    }
}
