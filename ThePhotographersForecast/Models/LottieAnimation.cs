using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;

namespace ThePhotographersForecast.Models;

public partial class LottieAnimation : ObservableObject
{
    [ObservableProperty]
    public partial SKFileLottieImageSource LottieWeather { get; set; }
    [ObservableProperty]
    public partial string Forecast { get; set; }
    [ObservableProperty]
    public partial string Temperature { get; set; }
    [ObservableProperty]
    public partial DateTime Time { get; set; }
    public string TimeString => "Time: " + Time.ToString("ddd, MMM d @ h tt");
    [ObservableProperty]
    public partial LinearGradientBrush SkyTransition { get; set; }
    [ObservableProperty]
    public partial bool IsActive { get; set; }
}
