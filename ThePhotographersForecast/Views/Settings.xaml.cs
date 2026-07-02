using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;


public partial class Settings : ContentPage
{
    public Settings(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}