using ThePhotographersForecast.Services;
using ThePhotographersForecast.ViewModels;
using ThePhotographersForecast.Views;

namespace ThePhotographersForecast;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToSelectLocationAsync();
        return;
    }
}