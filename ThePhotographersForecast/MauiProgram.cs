using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Foldable;
using Microsoft.Maui.Handlers;
using System.Collections.ObjectModel;
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
            builder.Services.AddHttpClient<IImageSourceService<UriImageSource>, UriImageSourceService>()
                    .ConfigureHttpClient(client =>
                    {
                        client.DefaultRequestHeaders.UserAgent.Add(
                            new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0")
                        );
                    });

            //Registered ViewModels
            builder.Services.AddSingleton<ViewModels.ForecastViewModel>();


            builder.Services.AddSingleton<Models.PhotoLocation>(sp =>
            {
                return new Models.PhotoLocation();
            });

            builder.Services.AddSingleton<GetPhotoDay>();
            builder.Services.AddSingleton<GetDaySchedules>();
            builder.Services.AddHttpClient("nws", c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
                c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/geo+json"));
            });
            builder.Services.AddSingleton<GetGfsData>();

            builder.Services.AddHttpClient("eventsApi", client =>
             {
                 client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ThePhotographersForecast", "1.0"));
                 client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
             });
            builder.Services.AddSingleton<GetTimesOfDayData>();


            builder.Services.AddSingleton<SelectLocationViewModel>(sp =>
            {
                var gfs = sp.GetRequiredService<GetGfsData>();
                var getTimesOfDayData = sp.GetRequiredService<GetTimesOfDayData>();
                var photoday = sp.GetRequiredService<GetPhotoDay>();
                var gds = sp.GetRequiredService<GetDaySchedules>();

                return new SelectLocationViewModel(gfs, getTimesOfDayData, photoday, gds);
            });

            builder.Services.AddSingleton<ViewModels.PhotoTimeTrackViewModel>(sp =>
            {
                var slvm = sp.GetRequiredService<SelectLocationViewModel>();
                return new PhotoTimeTrackViewModel(slvm);
            });
            builder.Services.AddSingleton<ViewModels.CalendarViewModel>(sp =>
            {
                var slvm = sp.GetRequiredService<SelectLocationViewModel>();
                return new CalendarViewModel(slvm);
            });



            //builder.Services.AddSingleton<Models.GfsWeatherModel>(sp =>
            //{
            //    var selectedLocation = sp.GetRequiredService<Models.PhotoLocation>();
            //    var gfsWeatherModel = Services.GetGfsData.GetWeatherAsync(selectedLocation.Latitude, selectedLocation.Longitude);
            //    return gfsWeatherModel.Result;
            //});
            //builder.Services.AddSingleton<Models.EventModel>(sp =>
            //{
            //    var selectedLocation = sp.GetRequiredService<Models.PhotoLocation>();
            //    var eventTimes = Services.GetTimesOfDayData.GetDaysAsync(selectedLocation.Latitude, selectedLocation.Longitude);
            //    return eventTimes.Result;
            //});
            //builder.Services.AddSingleton<ViewModels.CombineViewModels>(sp =>
            //{
            //    var selectLocationVM = sp.GetRequiredService<ViewModels.SelectLocationViewModel>();
            //    var forecastVM = sp.GetRequiredService<ViewModels.ForecastViewModel>();
            //    return new ViewModels.CombineViewModels(selectLocationVM, forecastVM);
            //});
            //builder.Services.AddSingleton<Views.SelectLocation>(rc =>
            //{
            //    var vm = rc.GetRequiredService<ViewModels.SelectLocationViewModel>();
            //    return new Views.SelectLocation(vm);
            //});
            //builder.Services.AddSingleton<Views.ShortForecast>(rc =>
            //{
            //    var slvm = rc.GetRequiredService<ViewModels.SelectLocationViewModel>();
            //    var vm = rc.GetRequiredService<ViewModels.ForecastViewModel>();
            //    return new Views.ShortForecast(vm, slvm);
            //});

            //builder.Services.AddSingleton<Services.NavigationService>(sp =>
            //{
            //    Services.NavigationService navigationService = new Services.NavigationService();
            //    return navigationService;
            //});



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
