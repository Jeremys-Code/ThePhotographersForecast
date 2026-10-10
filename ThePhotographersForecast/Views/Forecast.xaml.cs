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

            // subscribe to the correct event
            //PageScroll.MeasureInvalidated += ScrollView_MeasureInvalidated;

            // if your viewmodel collections change, force a relayout/remeasure
            //if (_vm.EventsByPeriod is INotifyCollectionChanged events)
            //    events.CollectionChanged += (_, __) => RefreshScroll();
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