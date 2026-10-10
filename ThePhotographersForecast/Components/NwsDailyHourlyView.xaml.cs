using ThePhotographersForecast.Models;

namespace ThePhotographersForecast.Components
{
    public partial class NwsDailyHourlyView : ContentView
    {
        public static readonly BindableProperty DaysProperty = BindableProperty.Create(
            nameof(Days), typeof(IEnumerable<DayViewModel>), typeof(NwsDailyHourlyView), propertyChanged: OnDaysPropertyChanged);

        public static readonly BindableProperty SelectedDayProperty = BindableProperty.Create(
            nameof(SelectedDay), typeof(DayViewModel), typeof(NwsDailyHourlyView), null, BindingMode.TwoWay);

        public static readonly BindableProperty SelectedDaysHourlyForecastsProperty = BindableProperty.Create(
            nameof(SelectedDaysHourlyForecasts), typeof(IEnumerable<NwsHourlyForecast>), typeof(NwsDailyHourlyView), null, BindingMode.TwoWay);

        public static readonly BindableProperty SelectedLocationProperty = BindableProperty.Create(
            nameof(SelectedLocation), typeof(LocationViewModel), typeof(NwsDailyHourlyView), null, BindingMode.TwoWay);

        public static readonly BindableProperty CurrentForecastProperty = BindableProperty.Create(
            nameof(CurrentForecast), typeof(NwsHourlyForecast), typeof(NwsDailyHourlyView), null, BindingMode.TwoWay);


        public NwsDailyHourlyView()
        {
            InitializeComponent();
        }

        private async Task OnFlip(DayViewModel day)
        {
            if (day == null || day.IsEmpty || !day.IsEventDay)
                return;

            day.IsFlipped = !day.IsFlipped;

            if (day.IsFlipped && day.Events.EventTimes != null)
            {
                SelectedDay = day;
                SelectedDay.Events.EventTimes = day.Events.EventTimes;

                SelectedDaysHourlyForecasts = SelectedDay.SelectedDailyHourlyForecasts;
                SelectedLocation = SelectedDay.SelectedLocation;
                CurrentForecast = Days.First().SelectedDailyHourlyForecasts[0];
            }
            return;
        }


        public IEnumerable<DayViewModel> Days
        {
            get => (IEnumerable<DayViewModel>)GetValue(DaysProperty);
            set => SetValue(DaysProperty, value);
        }

        public NwsHourlyForecast CurrentForecast
        {
            get => (NwsHourlyForecast)GetValue(CurrentForecastProperty);
            set => SetValue(CurrentForecastProperty, value);
        }
        public DayViewModel SelectedDay
        {
            get => (DayViewModel)GetValue(SelectedDayProperty);
            set => SetValue(SelectedDayProperty, value);
        }

        public LocationViewModel SelectedLocation
        {
            get => (LocationViewModel)GetValue(SelectedLocationProperty);
            set => SetValue(SelectedLocationProperty, value);
        }

        public IEnumerable<NwsHourlyForecast> SelectedDaysHourlyForecasts
        {
            get => (IEnumerable<NwsHourlyForecast>)GetValue(SelectedDaysHourlyForecastsProperty);
            set => SetValue(SelectedDaysHourlyForecastsProperty, value);
        }


        private static void OnDaysPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (NwsDailyHourlyView)bindable;
            // when daily forecasts collection changes, select the first item if nothing is selected
            if (control.Days == null && newValue is IEnumerable<DayViewModel> list)
            {
                control.SelectedDay = list.FirstOrDefault();
            }
        }
        private async void DailyCollection_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            if (e.CurrentItem is VisualElement selected)
            {
                await selected.ScaleToAsync(1.2, 250, Easing.CubicOut);
            }
        }
    }
}