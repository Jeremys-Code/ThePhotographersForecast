using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;


public partial class Settings : ContentPage
{
    private SettingsViewModel _vm;
    public Settings(SettingsViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;
    }
}