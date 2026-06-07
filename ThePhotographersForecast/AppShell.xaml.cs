using ThePhotographersForecast.Views;

namespace ThePhotographersForecast
{
    public partial class AppShell : Shell
    {

        public AppShell()
        {
            InitializeComponent();

        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Services.NavigationService.NavigateToSelectLocationAsync();
        }
    }
}
