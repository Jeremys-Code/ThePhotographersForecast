using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class Calendar : ContentPage
{
    private CalendarViewModel _vm;
    public Calendar(CalendarViewModel vm)
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