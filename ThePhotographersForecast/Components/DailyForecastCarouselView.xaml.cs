using SkiaSharp.Extended.UI.Controls;
using ThePhotographersForecast.Models;


namespace ThePhotographersForecast.Components;

public partial class DailyForecastCarouselView : ContentView
{
    public static readonly BindableProperty TimeZoneOffsetProperty = BindableProperty.Create(
        nameof(TimeZoneOffset),
        typeof(int),
        typeof(DailyForecastCarouselView),
        0);

    //public static readonly BindableProperty RunDatesProperty = BindableProperty.Create(
    //    nameof(RunDates),
    //    typeof(ObservableCollection<DateItem>),
    //    typeof(DailyForecastView),
    //    propertyChanged: OnRunDatesChanged);

    // Parameters that need to be passed into the component
    public static readonly BindableProperty DailiesProperty = BindableProperty.Create(
        nameof(Dailies),
        typeof(IEnumerable<NwsDailyForecast>),
        typeof(DailyForecastCarouselView),
        default(IEnumerable<NwsDailyForecast>),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDailyForecastChanged);

    public static readonly BindableProperty HourliesProperty = BindableProperty.Create(
    nameof(Hourlies),
    typeof(IEnumerable<NwsHourlyForecast>),
    typeof(DailyForecastCarouselView),
    default(IEnumerable<NwsHourlyForecast>),
    BindingMode.OneWay,
 propertyChanged: OnSelectedDailyForecastChanged);

    public static readonly BindableProperty SupplementalsProperty = BindableProperty.Create(
        nameof(Supplementals),
        typeof(IEnumerable<NwsHourlySupplemental>),
        typeof(DailyForecastCarouselView),
        default(IEnumerable<NwsHourlySupplemental>),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDailyForecastChanged);

    public static readonly BindableProperty SelectedDailyProperty = BindableProperty.Create(
        nameof(SelectedDaily),
        typeof(NwsDailyForecast),
        typeof(DailyForecastCarouselView),
        default(NwsDailyForecast),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDailyForecastChanged);

    public static readonly BindableProperty SelectedDailyEventsProperty = BindableProperty.Create(
        nameof(SelectedDailyEvents),
        typeof(IEnumerable<EventTime>),
        typeof(DailyForecastCarouselView),
        default(IEnumerable<EventTime>),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDailyForecastChanged);

    public static readonly BindableProperty DailyHighAnimationProperty = BindableProperty.Create(
        nameof(DailyHighAnimation),
        typeof(SKFileLottieImageSource),
        typeof(DailyForecastCarouselView),
        default(SKFileLottieImageSource),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDailyForecastChanged);

    // The members, mapped to the Values passed in
    //public ObservableCollection<DateItem> RunDates
    //{
    //    get => (ObservableCollection<DateItem>)GetValue(RunDatesProperty);
    //    set => SetValue(RunDatesProperty, value);
    //}

    public IEnumerable<NwsDailyForecast> Dailies
    {
        get => (IEnumerable<NwsDailyForecast>)GetValue(DailiesProperty);
        set => SetValue(DailiesProperty, value);
    }

    public string TimePeriod
    {
        get => field;
        set
        {
            field = $"Start Time: {SelectedDaily.StartTime.ToString("F")} - {SelectedDaily.EndTime.ToString("F")}";
            OnPropertyChanged();
        }
    }

    public SKFileLottieImageSource DailyHighAnimation
    {
        get => field;
        set => SetValue(DailyHighAnimationProperty, value);
    }
    public IEnumerable<EventTime> SelectedDailyEvents
    {
        get => (IEnumerable<EventTime>)GetValue(SelectedDailyEventsProperty);
        set => SetValue(SelectedDailyEventsProperty, value);
    }
    public IEnumerable<NwsHourlyForecast> Hourlies
    {
        get => (IEnumerable<NwsHourlyForecast>)GetValue(HourliesProperty);
        set => SetValue(HourliesProperty, value);
    }
    public IEnumerable<NwsHourlySupplemental> Supplementals
    {
        get => (IEnumerable<NwsHourlySupplemental>)GetValue(SupplementalsProperty);
        set => SetValue(SupplementalsProperty, value);
    }
    public NwsDailyForecast SelectedDaily
    {
        get => (NwsDailyForecast)GetValue(SelectedDailyProperty);
        set => SetValue(SelectedDailyProperty, value);
    }
    public int TimeZoneOffset
    {
        get => (int)GetValue(TimeZoneOffsetProperty);
        set => SetValue(TimeZoneOffsetProperty, value);
    }

    private bool _isSelectedRange = true;

    public DailyForecastCarouselView()
    {
        InitializeComponent();

        // Ensure we have a non-null enumerable to avoid XAML binding issues when no data supplied
        if (Dailies == null)
            Dailies = new List<NwsDailyForecast>();
    }

    private static void UpdateForecasts(object bindable, object oldValue, object newValue)
    {
        // Placeholder for future update logic when bound collections/properties change
    }

    private static void OnSelectedDailyForecastChanged(object bindable, object oldValue, object newValue)
    {
        // Currently we don't need to react to changes here, but keep the method for future use.
    }


    async void OnCardTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // The sender is the Border that has the gesture recognizer in the DataTemplate
            if (sender is VisualElement ve)
            {
                // Attempt to animate a subtle lift (scale + translate) to simulate elevation
                var card = ve as VisualElement;

                // If the element is a Border, we can try to tweak its Shadow opacity for extra effect
                Border? border = ve as Border;
                double originalShadowOpacity = -1;
                if (border?.Shadow != null)
                {
                    originalShadowOpacity = border.Shadow.Opacity;
                    try { border.Shadow.Opacity = Math.Min(1.0f, ((float)originalShadowOpacity <= 0f ? 0.6f : (float)originalShadowOpacity + 0.15f)); } catch { }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.03, 120, Easing.CubicOut),
                        card.TranslateToAsync(0, -6, 120, Easing.CubicOut)
                    );
                }

                var front = ve.FindByName<Grid>("FrontView");
                var back = ve.FindByName<Grid>("BackView");
                if (front != null && back != null)
                {
                    if (front.IsVisible)
                    {
                        await front.RotateYToAsync(180, 180, Easing.CubicIn);
                        front.IsVisible = false;
                        back.RotationY = -1800;
                        back.IsVisible = true;
                        await back.RotateYToAsync(0, 220, Easing.CubicOut);
                    }
                    else
                    {
                        await back.RotateYToAsync(-180, 180, Easing.CubicIn);
                        back.IsVisible = false;
                        front.RotationY = -180;
                        front.IsVisible = true;
                        await front.RotateYToAsync(0, 220, Easing.CubicOut);
                    }
                }

                if (card != null)
                {
                    await Task.WhenAll(
                        card.ScaleToAsync(1.0, 120, Easing.CubicIn),
                        card.TranslateToAsync(0, 0, 120, Easing.CubicIn)
                    );
                }

                if (border?.Shadow != null && originalShadowOpacity >= 0)
                {
                    try { border.Shadow.Opacity = (float)originalShadowOpacity; } catch { }
                }
            }

            if (sender is BindableObject bo && bo.BindingContext is NwsDailyForecast daily)
            {
                SelectedDaily = daily;
            }
        }
        catch (Exception)
        {
            if (sender is BindableObject bo && bo.BindingContext is NwsDailyForecast daily)
                SelectedDaily = daily;
        }
    }
}