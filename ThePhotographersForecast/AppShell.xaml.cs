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
            Routing.RegisterRoute(nameof(LottieParade), typeof(LottieParade));
            Routing.RegisterRoute(nameof(ForecastDetails), typeof(ForecastDetails));
            Routing.RegisterRoute(nameof(Calendar), typeof(Calendar));
            Routing.RegisterRoute(nameof(DataCheck), typeof(DataCheck));
            Routing.RegisterRoute(nameof(EventPlanner), typeof(EventPlanner));
        }


        protected override async void OnParentSet()
        {
            base.OnParentSet();

        }
    }
}


