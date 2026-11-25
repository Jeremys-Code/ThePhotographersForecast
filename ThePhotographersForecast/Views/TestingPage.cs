using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;

public partial class TestingPage : ContentPage
{
	private DailyForecastViewModel _vm;
	public TestingPage(DailyForecastViewModel vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = _vm;
	}
}