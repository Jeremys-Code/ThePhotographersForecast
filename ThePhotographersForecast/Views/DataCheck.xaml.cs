using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class DataCheck : ContentPage
{
    private DataCheckViewModel _vm;
    //    private IBackgroundMapService _mapService;
    public DataCheck(DataCheckViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.RefreshFromSession();
    }
}

