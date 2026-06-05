using CommunityToolkit.Mvvm.ComponentModel;
using PhotographersDay.Services;


namespace PhotographersDay.ViewModels
{
    public class CombineViewModels : ObservableObject
    {
        [ObservableProperty]
        private SelectLocationViewModel selectLocationVM;
        [ObservableProperty]
        private ForecastViewModel forecastVM;

        public CombineViewModels(SelectLocationViewModel selectLocationVM, ForecastViewModel forecastVM)
        {
            selectLocationVM = ServiceHelper.GetService<SelectLocationViewModel>();
            forecastVM = ServiceHelper.GetService<ForecastViewModel>();
        }
    }
}
