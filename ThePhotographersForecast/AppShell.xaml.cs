using Microsoft.Maui.Storage;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Views;

namespace ThePhotographersForecast
{
    public partial class AppShell : Shell
    {

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SelectLocation), typeof(SelectLocation));
            Routing.RegisterRoute(nameof(Settings), typeof(Settings));
            Routing.RegisterRoute(nameof(Forecast), typeof(Forecast));
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await NavigationService.NavigateToLaunchPage();
        }
    }
}


