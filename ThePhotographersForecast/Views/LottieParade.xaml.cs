using ThePhotographersForecast.Services;

namespace ThePhotographersForecast.Views;

public partial class LottieParade : ContentPage
{
    private ILottieParadeService _lottieService;
    public ThePhotographersForecast.Models.LottieAnimation[] LottieCollection { get; set; }
    private CancellationTokenSource _paradeCts;

    public LottieParade(ILottieParadeService lottieService)
    {
        InitializeComponent();
        _lottieService = lottieService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _paradeCts?.Cancel();
        _paradeCts = new CancellationTokenSource();

        // load parade items
        var items = await _lottieService.BuildLottieParadeAsync().ConfigureAwait(false);
        if (items == null || items.Length == 0)
            return;

        // ensure we are on the main thread to set UI-bound collections
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LottieCollection = items;
            LottieParadeView.ItemsSource = LottieCollection;
        });

        // start the staggered parade loop
        _ = StartParadeLoopAsync(_paradeCts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _paradeCts?.Cancel();
        _paradeCts = null;
    }

    private async Task StartParadeLoopAsync(CancellationToken token)
    {
        int index = 0;
        int length = LottieCollection?.Length ?? 0;
        if (length == 0)
            return;

        while (!token.IsCancellationRequested)
        {
            var current = index % length;

            // make the current item visible in the CarouselView
            MainThread.BeginInvokeOnMainThread(() => LottieParadeView.Position = current);

            // trigger the animation for the current item
            LottieCollection[current].IsActive = true;

            // start next item 10s later to create the requested 5s overlap (animations are 15s long)
            try
            {
                await Task.Delay(10000, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            index++;
        }
    }
}
