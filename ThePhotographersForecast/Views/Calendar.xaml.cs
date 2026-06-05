using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class Calendar : ContentPage
{
    private readonly CalendarViewModel _cvm;
    public Calendar(CalendarViewModel cvm)
    {
        InitializeComponent();
        _cvm = cvm;
        BindingContext = _cvm;
    }
}