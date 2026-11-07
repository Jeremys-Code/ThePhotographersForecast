using SkiaSharp.Extended.UI.Controls;
using System.ComponentModel;

namespace ThePhotographersForecast.Views.Behaviors;

public class LottieParadeItemBehavior : Behavior<Grid>
{
    Grid? _associated;
    ThePhotographersForecast.Models.LottieAnimation? _model;
    bool _running;

    protected override void OnAttachedTo(Grid bindable)
    {
        base.OnAttachedTo(bindable);
        _associated = bindable;
        bindable.BindingContextChanged += Bindable_BindingContextChanged;
        TryWireModel(bindable.BindingContext);
    }

    protected override void OnDetachingFrom(Grid bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.BindingContextChanged -= Bindable_BindingContextChanged;
        if (_model != null)
            _model.PropertyChanged -= Model_PropertyChanged;
        _associated = null;
    }

    private void Bindable_BindingContextChanged(object? sender, EventArgs e)
    {
        TryWireModel(((BindableObject)sender!).BindingContext);
    }

    private void TryWireModel(object? ctx)
    {
        if (_model != null)
            _model.PropertyChanged -= Model_PropertyChanged;

        _model = ctx as ThePhotographersForecast.Models.LottieAnimation;
        if (_model != null)
            _model.PropertyChanged += Model_PropertyChanged;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThePhotographersForecast.Models.LottieAnimation.IsActive) && _model?.IsActive == true)
        {
            _ = RunAnimationsAsync();
        }
    }

    private async Task RunAnimationsAsync()
    {
        if (_running || _associated == null || _model == null)
            return;
        _running = true;

        try
        {
            // find named children
            var timeLabel = _associated.FindByName<Label>("Time") ?? _associated.FindByName<Label>("TimeLabel");
            var tempLabel = _associated.FindByName<Label>("Temp") ?? _associated.FindByName<Label>("TempLabel");
            var forecastLabel = _associated.FindByName<Label>("Forecast") ?? _associated.FindByName<Label>("ForecastLabel");
            var lottie = _associated.FindByName<SKLottieView>("Lottie") ?? _associated.FindByName<SKLottieView>("skLottie");

            // wait for layout width
            double width = _associated.Width;
            if (width <= 0)
            {
                var tcs = new TaskCompletionSource<bool>();
                void handler(object s, EventArgs e)
                {
                    if (_associated.Width > 0) { _associated.SizeChanged -= handler; tcs.SetResult(true); }
                }
                _associated.SizeChanged += handler;
                await tcs.Task.ConfigureAwait(false);
                width = _associated.Width;
            }

            // initialize
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (lottie != null) lottie.TranslationX = -width;
                if (timeLabel != null) timeLabel.Opacity = 0;
                if (tempLabel != null) tempLabel.Opacity = 0;
                if (forecastLabel != null) forecastLabel.Opacity = 0;

            });

            var movementTask = lottie?.TranslateToAsync(width, 0, 15000u, Easing.Linear) ?? Task.CompletedTask;

            var labelsTask = Task.Run(async () =>
            {
                var fades = new List<Task>();
                if (timeLabel != null) fades.Add(timeLabel.FadeToAsync(1, 5000u, Easing.Linear));
                if (tempLabel != null) fades.Add(tempLabel.FadeToAsync(1, 5000u, Easing.Linear));
                if (forecastLabel != null) fades.Add(forecastLabel.FadeToAsync(1, 5000u, Easing.Linear));
                await Task.WhenAll(fades).ConfigureAwait(false);

                await Task.Delay(5000).ConfigureAwait(false);

                var fadesOut = new List<Task>();
                if (timeLabel != null) fadesOut.Add(timeLabel.FadeToAsync(0, 5000u, Easing.Linear));
                if (tempLabel != null) fadesOut.Add(tempLabel.FadeToAsync(0, 5000u, Easing.Linear));
                if (forecastLabel != null) fadesOut.Add(forecastLabel.FadeToAsync(0, 5000u, Easing.Linear));
                await Task.WhenAll(fadesOut).ConfigureAwait(false);
            });

            await Task.WhenAll(movementTask, labelsTask).ConfigureAwait(false);
        }
        catch
        {
            // swallow
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() => { if (_model != null) _model.IsActive = false; });
            _running = false;
        }
    }
}
