using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Foldable;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models.DayData;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.ML;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;
using ThePhotographersForecast.ViewModels;
using ThePhotographersForecast.Views;
using ThePhotographersForecast.Database;


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
                .UseFoldable()
                .UseMauiMaps()
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("NotoSansECLight.ttf", "NotoSans");
                    fonts.AddFont("NotoSansBlack.ttf", "NotoThick");
                    fonts.AddFont("HennyPenny.ttf", "HennyPenny");
                });
            //Create logging service
            //  builder.Services.AddLogging(loggingBuilder =>
            //  {
            //      var logFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, "thephotographersforecast.log");
            //      // NReco Implementation
            //      loggingBuilder.AddFile(logFilePath, opts =>
            //{
            //    opts.MaxRollingFiles = 5;
            //    opts.FileSizeLimitBytes = 10_000_000; // 10 MB
            //    opts.Append = true;
            //    opts.RollingFilesConvention = FileLoggerOptions.FileRollingConvention.Descending;
            //    opts.MinLevel = LogLevel.Information;
            //});
            //      // SeriLog Implementation
            //      loggingBuilder.AddSerilog(new LoggerConfiguration()
            //      .MinimumLevel.Information()
            //      .MinimumLevel.Override("System.Net", LogEventLevel.Information)
            //      .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
            //      .MinimumLevel.Override("System.Net.Sockets", LogEventLevel.Information)
            //      .MinimumLevel.Override("System.Net.Security", LogEventLevel.Information)
            //      .Enrich.WithProperty("App", "PhotographersForecast")
            //      .Enrich.WithMachineName()
            //      .WriteTo.File(
            //                 Path.Combine(FileSystem.Current.AppDataDirectory, "thephotographersforecast.json"),
            //                 rollingInterval: RollingInterval.Day,
            //                 retainedFileCountLimit: 7,
            //                 shared: true)
            //      .CreateLogger());
            //  });
            //  // Logging Service
            //  builder.Services.AddSingleton<ILoggingService, LoggingService>();
            //  builder.Logging.AddFilter("System.Net.Http", LogLevel.Trace);
            //  builder.Logging.AddFilter("System.Net.Sockets", LogLevel.Trace);
            //  builder.Logging.AddFilter("System.Net.Security", LogLevel.Trace);
            //Registered HttpClientFactory


            builder.Services.AddHttpClient("photographer", c =>
             {
                 c.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
                 c.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0 (airmail@bostonfromabove.com)");
                 c.DefaultRequestHeaders.Add("Accept", "application/geo+json");
             });
            builder.Services.AddHttpClient<IGetMoonService, GetMoonService>(c =>
            {
                c.BaseAddress = new Uri("https://aa.usno.navy.mil/api/moon/phases/");
                c.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "6.0"));
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            builder.Services.AddHttpClient<IGetNwsData, GetNwsData>(c =>
            {
                c.BaseAddress = new Uri("https://api.weather.gov/");
                c.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0 (airmail@bostonfromabove.com)");
                c.DefaultRequestHeaders.Add("Accept", "application/geo+json");
            });
            builder.Services.AddHttpClient<IGetWeatherService, GetWeatherService>(c =>
            {
                c.BaseAddress = new Uri("https://api.open-meteo.com/");
                c.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0 (airmail@bostonfromabove.com)");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            builder.Services.AddHttpClient<ISkyMetricsService, SkyMetricsService>(c =>
            {
                c.BaseAddress = new Uri("https://api.open-meteo.com/");
                c.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/7.0 (airmail@bostonfromabove.com)");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            builder.Services.AddHttpClient<ISkyColorEngine, SkyColorEngine>(c =>
            {
                c.DefaultRequestHeaders.UserAgent.ParseAdd("ThePhotographersForecast/6.0 (airmail@bostonfromabove.com)");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Register Shared ISession Service

            //builder.Services.AddSingleton<IIpGeolocationApiClient, IpGeolocationApiClient>();
            //builder.Services.AddSingleton<IOpenMeteoApiClient, OpenMeteoApiClient>();
            //builder.Services.AddSingleton<IGetEventService, GetEventService>();
            //Replace the event service with the ephemeris service calculating all the times of day locally
            //builder.Services.AddSingleton<IGetNwsData, GetNwsData>();
            builder.Services.AddSingleton<IGetEphemerisService, GetEphemerisService>();
            //builder.Services.AddSingleton<IGetWeatherService, GetWeatherService>();
            builder.Services.AddSingleton<IWeatherReporter, WeatherReporter>();
            builder.Services.AddSingleton<IImageCacheService, ImageCacheService>();
            //builder.Services.AddSingleton<IGetMoonService, GetMoonService>();
            // IGetNwsData is registered via AddHttpClient above (typed client), so don't register it again as singleton.
            //builder.Services.AddSingleton<ISkyColorEngine, SkyColorEngine>();
            builder.Services.AddSingleton<IForecastService, ForecastService>();
            builder.Services.AddSingleton<IBackgroundMapService, BackgroundMapService>();
            builder.Services.AddSingleton<IDailyForecastEventsView, DailyForecastEventsView>();
            builder.Services.AddSingleton<ISession, Session>();
            builder.Services.AddSingleton<ILottieParadeService, LottieParadeService>();
            builder.Services.AddSingleton<IGetDays, GetDays>();
            builder.Services.AddSingleton<PhotographersForecastOnnxService>();
            builder.Services.AddSingleton<ISkyColorCalculator, SkyColorCalculator>();
            // Register components


            // Register the View Models

            builder.Services.AddSingleton<SelectLocationViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<ForecastViewModel>();
            builder.Services.AddSingleton<ForecastDetailsViewModel>();
            builder.Services.AddSingleton<CalendarViewModel>();
            builder.Services.AddSingleton<DataCheckViewModel>();
            builder.Services.AddSingleton<DailyForecastViewModel>();
            builder.Services.AddSingleton<EventPlannerViewModel>();
            builder.Services.AddSingleton<TestingPageViewModel>();
            // Register the Views and inject the VM's
            builder.Services.AddSingleton<SelectLocation>();
            builder.Services.AddSingleton<Forecast>();
            builder.Services.AddSingleton<ForecastDetails>();
            builder.Services.AddSingleton<DataCheck>();
            builder.Services.AddSingleton<EventPlanner>();
            builder.Services.AddSingleton<TestingPage>();
            // Register the ContentViews
            builder.Services.AddSingleton<NwsDailyHourlyView>();
            builder.Services.AddSingleton<DataAccess>();


            //#if IOS || MACCATALYST
            //        // Register the platform-specific implementation
            //        builder.Services.AddSingleton<NLEmbeddingGenerator>();

            //        // Register as IEmbeddingGenerator with middleware
            //        builder.Services.AddEmbeddingGenerator(sp =>
            //            sp.GetRequiredService<NLEmbeddingGenerator>()
            //              .AsBuilder()
            //              .UseLogging()
            //              .Build(sp));
            //#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif


            MauiApp app = builder.Build();
            //NavigationService.Initialize(app.Services);
            return app;
        }
    }
}