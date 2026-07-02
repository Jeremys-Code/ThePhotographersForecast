using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Foldable;
using System.Net.Http.Headers;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;



namespace ThePhotographersForecast
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
                    fonts.AddFont("hennypenny.ttf", "ManFont");
                    fonts.AddFont("jost.ttf", "Jost");
                    fonts.AddFont("notosans.ttf", "NotoSans");
                    fonts.AddFont("notosansitalic.ttf", "NotoSansItalic");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentUI");
                    fonts.AddFont("FluentSystemIcons-Filled.ttf", "FluentFilled");
                    fonts.AddFont("FluentSystemIcons-Light.ttf", "FluentLight");
                    fonts.AddFont("FluentSystemIcons-Resizable.ttf", "FluentResizable");
                });
            builder.UseFoldable();


            // Registered HttpClientFactory
            builder.Services.AddHttpClient("photographer", c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
            });
            // Register the database
            //          builder.Services.AddSingleton<Database.DataAccess>();
            // Register Shared Session Service
            //builder.Services.AddSingleton<ISessionStore, SessionStore>();
            // Register the View Models
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<SelectLocationViewModel>();
            builder.Services.AddSingleton<ForecastViewModel>();

            // Register Services
            builder.Services.AddSingleton<GetWeatherService>();
            builder.Services.AddSingleton<WeatherReporter>();
            builder.Services.AddSingleton<GetTimesOfDayData>();
            //   builder.Services.AddSingleton<AzureMapsLocationService>();
            // Register the Views





#if DEBUG
            builder.Logging.AddDebug();
#endif


            MauiApp app = builder.Build();
            Services.NavigationService.Initialize(app.Services);
            return app;
        }
    }
}