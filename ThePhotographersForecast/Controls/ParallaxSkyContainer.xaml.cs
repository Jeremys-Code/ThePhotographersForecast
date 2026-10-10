using static ThePhotographersForecast.Services.TimeOfDayService;

namespace ThePhotographersForecast.Controls;

public partial class ParallaxSkyContainer : ContentView
{

    public ParallaxSkyContainer()
    {
        InitializeComponent();
    }

    protected async override void OnParentSet()
    {
        base.OnParentSet();
        await UpdateSkyAsync(Services.TimeOfDayService.GetTimeOfDay(DateTime.Now.Hour));
    }

    public async Task UpdateSkyAsync(TimeOfDay tod)
    {
        var (gradient, image) = SkySelector.GetSky(tod);


        {
            await Sky.FadeToAsync(0, 150);
            this.SetSky(gradient, image);
            await Sky.FadeToAsync(1, 250);
        }
    }
    public void SetSky(Brush gradient, ImageSource image)
    {
        Sky.Gradient = gradient;
        Sky.SkyImage = image;
    }

    public void ApplyParallax(double offset)
    {
        Sky.ApplyParallax(offset);
    }
}
