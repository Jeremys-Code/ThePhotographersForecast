// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ThePhotographersForecast.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        // Implement the abstract CreateMauiApp required by MauiWinUIApplication
        protected override MauiApp CreateMauiApp()
        {
            // Return the app created by your Maui program entry point
            return MauiProgram.CreateMauiApp();
        }
    }

}
