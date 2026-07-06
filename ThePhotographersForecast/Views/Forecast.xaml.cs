using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class Forecast : ContentPage
{
    public Forecast(ForecastViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}