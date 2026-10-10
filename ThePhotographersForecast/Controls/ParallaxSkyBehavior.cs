namespace ThePhotographersForecast.Controls
{
    public class ParallaxSkyBehavior : Behavior<ScrollView>
    {
        ParallaxSkyContainer _container;

        protected override void OnAttachedTo(ScrollView scrollView)
        {
            base.OnAttachedTo(scrollView);
            scrollView.Scrolled += OnScrolled;
            _container = scrollView.Parent as ParallaxSkyContainer;
        }

        void OnScrolled(object sender, ScrolledEventArgs e)
        {
            _container?.ApplyParallax(e.ScrollY);
        }

        protected override void OnDetachingFrom(ScrollView scrollView)
        {
            base.OnDetachingFrom(scrollView);
            scrollView.Scrolled -= OnScrolled;
        }
    }

}
