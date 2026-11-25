using ThePhotographersForecast.Views;

namespace ThePhotographersForecast
{

    public partial class App : Application
    {
        //private SelectLocation _selectLocation;
        public App(IServiceProvider service)
        {
            InitializeComponent();
            //AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            //{
            //    System.Diagnostics.Debug.WriteLine($"[XAML ERROR] {e.Exception}");
            //};
          //  _selectLocation = service.GetService<SelectLocation>();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
