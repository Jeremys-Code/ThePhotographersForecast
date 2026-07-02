using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class Forecast : ContentPage
{
    private readonly SelectLocationViewModel _vm;
    public Forecast(SelectLocationViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }
}