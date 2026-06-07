using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Foldable;
using System.Net.Http.Headers;
using ThePhotographersForecast.Models;
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

                });
            builder.UseFoldable();

            // GLOBAL User-Agent for all image downloads
            // Override HttpClient used by UriImageSourceService
            //IHttpClientBuilder httpClientBuilder = builder.Services.AddHttpClient<IImageSourceService<UriImageSource>, UriImageSourceService>()
            //        .ConfigureHttpClient(client =>
            //        {
            //            client.DefaultRequestHeaders.UserAgent.Add(
            //                new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0")
            //            );
            //        });
            builder.Services.AddHttpClient("photographer", c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
                c.Timeout = TimeSpan.FromSeconds(10);
            });

            // Register Single Instance Models
            builder.Services.AddSingleton<Models.PhotoLocation>();

            // Register Services
            builder.Services.AddSingleton<ImageCacheService>();
            builder.Services.AddSingleton<GetGfsData>();
            builder.Services.AddSingleton<GetTimesOfDayData>();
            builder.Services.AddSingleton<GetPhotoDay>();
            builder.Services.AddSingleton<GetDaySchedules>();

            // Register the View Models
            builder.Services.AddSingleton<ViewModels.SelectLocationViewModel>(sp =>
            {
                var gfs = sp.GetRequiredService<GetGfsData>();
                var gtodd = sp.GetRequiredService<GetTimesOfDayData>();
                var gpd = sp.GetRequiredService<GetPhotoDay>();
                var sl = sp.GetRequiredService<Models.PhotoLocation>();

                return new ViewModels.SelectLocationViewModel(gfs, gtodd, gpd, sl);
            });

            builder.Services.AddSingleton<ViewModels.PhotoTimeTrackViewModel>(sp =>
            {
                var slvm = sp.GetRequiredService<SelectLocationViewModel>();
                var gds = sp.GetRequiredService<GetDaySchedules>();
                var location = sp.GetRequiredService<PhotoLocation>();
                return new PhotoTimeTrackViewModel(slvm, gds, location);
            });

            builder.Services.AddSingleton<ViewModels.ForecastViewModel>(sp =>
            {
                var slvm = sp.GetRequiredService<SelectLocationViewModel>();
                var location = sp.GetRequiredService<PhotoLocation>();
                return new ForecastViewModel(slvm, location);
            });
            // Register the Views
            builder.Services.AddSingletonWithShellRoute<Views.SelectLocation, ViewModels.SelectLocationViewModel>("//Home");
            builder.Services.AddSingletonWithShellRoute<Views.PhotoTimeTrack, ViewModels.PhotoTimeTrackViewModel>("//Home/PhotoTimeTrack");

#if DEBUG
            builder.Logging.AddDebug();
#endif


            MauiApp app = builder.Build();
            Services.ServiceHelper.Initialize(app.Services);
            Services.NavigationService.Initialize(app.Services);
            return app;
        }
    }
}
