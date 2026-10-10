using ThePhotographersForecast.ViewModels.Compnents;

namespace ThePhotographersForecast.Components;

public partial class MoonPhasesCarouselView : ContentView
{
    private readonly MoonPhasesCarouselViewViewModel _vm;
    public MoonPhasesCarouselView()
    {
        InitializeComponent();
        BindingContext = _vm;
    }
}