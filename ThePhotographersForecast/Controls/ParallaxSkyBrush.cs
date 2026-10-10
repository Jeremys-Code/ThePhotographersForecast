namespace ThePhotographersForecast.Controls
{
    public class ParallaxSkyBrush : Grid
    {
        readonly Image _imageLayer;
        readonly Border _gradientLayer;

        public ParallaxSkyBrush()
        {
            _gradientLayer = new Border
            {
                Background = new SolidColorBrush(Colors.Transparent),
                InputTransparent = true
            };

            _imageLayer = new Image
            {
                Aspect = Aspect.AspectFill,
                InputTransparent = true
            };

            Children.Add(_gradientLayer);
            Children.Add(_imageLayer);
        }

        public Brush Gradient
        {
            get => _gradientLayer.Background;
            set => _gradientLayer.Background = value;
        }

        public ImageSource SkyImage
        {
            get => _imageLayer.Source;
            set => _imageLayer.Source = value;
        }

        public void ApplyParallax(double offset)
        {
            _imageLayer.TranslationX = offset * 0.15;
            _imageLayer.TranslationY = offset * 0.05;
        }
    }

}
