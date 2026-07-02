using CommunityToolkit.Maui;
using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views;


public partial class TimeLine : ContentPage
{
    private readonly PhotoTimeTrackViewModel _viewModel;
    public TimeLine(PhotoTimeTrackViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = _viewModel;
    }
}