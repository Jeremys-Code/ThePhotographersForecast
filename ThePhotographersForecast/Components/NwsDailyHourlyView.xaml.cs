using CommunityToolkit.Maui.Core.Extensions;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Components
{
    public partial class NwsDailyHourlyView : ContentView
    {
        private ISession _session;
        public static readonly BindableProperty DailyCollectionProperty = BindableProperty.Create(
            nameof(DailyCollection), typeof(IEnumerable<DayViewModel>), typeof(NwsDailyHourlyView), propertyChanged: OnDailyCollectionPropertyChanged);

        public static readonly BindableProperty SelectedDayProperty = BindableProperty.Create(
            nameof(SelectedDay), typeof(DayViewModel), typeof(NwsDailyHourlyView), null, BindingMode.TwoWay);

        public static readonly BindableProperty SelectedDaysHourlyForecastsProperty = BindableProperty.Create(
            nameof(SelectedDaysHourlyForecasts), typeof(IEnumerable<NwsHourlyForecast>), typeof(NwsDailyHourlyView), null);

        public static readonly BindableProperty SelectedLocationProperty = BindableProperty.Create(
            nameof(SelectedLocation), typeof(PhotoLocation), typeof(NwsDailyHourlyView), null);

        public static readonly BindableProperty CurrentForecastProperty = BindableProperty.Create(
            nameof(CurrentForecast), typeof(NwsHourlyForecast), typeof(NwsDailyHourlyView), null);


        public NwsDailyHourlyView(ISession session)
        {
            InitializeComponent();
            _session = session;
            DailyCollection = _session.Days.ToObservableCollection();
            SelectedDay = DailyCollection.FirstOrDefault();
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
                SelectedLocation = _session.NwsData.SelectedLocation;
                CurrentForecast = DailyCollection.First().SelectedDailyHourlyForecasts[0];
            }
            return;
        }


        public IEnumerable<DayViewModel> DailyCollection
        {
            get => (IEnumerable<DayViewModel>)GetValue(DailyCollectionProperty);
            set => SetValue(DailyCollectionProperty, value);
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

        public PhotoLocation SelectedLocation
        {
            get => (PhotoLocation)GetValue(SelectedLocationProperty);
            set => SetValue(SelectedLocationProperty, value);
        }

        public IEnumerable<NwsHourlyForecast> SelectedDaysHourlyForecasts
        {
            get => (IEnumerable<NwsHourlyForecast>)GetValue(SelectedDaysHourlyForecastsProperty);
            set => SetValue(SelectedDaysHourlyForecastsProperty, value);
        }


        private static void OnDailyCollectionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (NwsDailyHourlyView)bindable;
            // when daily forecasts collection changes, select the first item if nothing is selected
            if (control.DailyCollection == null && newValue is IEnumerable<DayViewModel> list)
            {
                control.SelectedDay = list.FirstOrDefault();
            }
        }

        private async void Collection_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {

            if (e.CurrentItem is VisualElement selected)
            {
                await selected.ScaleToAsync(1.2, 250, Easing.CubicOut);
            }
        }
    }
}