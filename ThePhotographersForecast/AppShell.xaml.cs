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

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (Preferences.ContainsKey("FirstRun"))
            {
                var results = Preferences.Get("FirstRun", true);
                if (results == true)
                {
                    Preferences.Set("FirstRun", false);
                    await NavigationService.NavigateToSettingsAsync();
                }
                else
                {
                    await NavigationService.NavigateToSelectLocationAsync();
                }
            }
            else
            {
                await NavigationService.NavigateToSettingsAsync();
                Preferences.Set("FirstRun", false);
            }
        }
    }
}


