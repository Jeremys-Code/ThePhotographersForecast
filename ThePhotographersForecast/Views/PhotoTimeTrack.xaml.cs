namespace ThePhotographersForecast.Views;

public partial class PhotoTimeTrack : ContentPage
{
    private readonly ViewModels.PhotoTimeTrackViewModel _vm;
    public PhotoTimeTrack(ViewModels.PhotoTimeTrackViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }
}