using ThePhotographersForecast.ViewModels;

namespace ThePhotographersForecast.Views
{
    public partial class Forecast : ContentPage
    {
        private ForecastViewModel _vm;

        public Forecast(ForecastViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.RefreshFromSession();
            _vm.StartClock();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.StopClock();
        }
    }
}

//        private void ScrollView_MeasureInvalidated(object sender, EventArgs e)
//        {
//            RefreshScroll();
//        }

//        private void RefreshScroll()
//        {
//            MainThread.BeginInvokeOnMainThread(() =>
//            {
//                // re-measure the content and the scrollview and force layout
//                PageScroll.Content?.InvalidateMeasure();
//                PageScroll.InvalidateMeasure();
//            });
//        }
//    }
//}