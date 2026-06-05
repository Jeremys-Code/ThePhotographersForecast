using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;
using ThePhotographersForecast.Views;

namespace ThePhotographersForecast
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {

            return new Window(new AppShell());

        }
    }
}