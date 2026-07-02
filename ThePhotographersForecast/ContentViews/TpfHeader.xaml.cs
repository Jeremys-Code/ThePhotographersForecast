namespace ThePhotographersForecast.ContentViews;

public partial class TpfHeader : ContentView
{

    public TpfHeader()
    {
        this.InitializeComponent();
        this.SizeChanged += OnPageSizeChanged;
        this.Loaded += async (s, e) =>
        {
            // Animation starts after the view is fully loaded
            await Logo.FadeToAsync(1, 300, Easing.CubicIn);
            await Title.FadeToAsync(1, 500, Easing.CubicIn);
        };
    }

    private async void OnPageSizeChanged(object? sender, EventArgs e)
    {

        double width = this.Width;

        // Simple scaling formula: adjust divisor to control size
        double newFontSize = width / 20;

        // Set a minimum and maximum font size to avoid extremes
        newFontSize = Math.Max(10, Math.Min(newFontSize, 60));

        Title.FontSize = newFontSize;

    }
}
