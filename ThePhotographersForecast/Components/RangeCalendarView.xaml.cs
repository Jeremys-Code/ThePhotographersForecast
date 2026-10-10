using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ThePhotographersForecast.Components
{
    public partial class RangeCalendarView : ContentView
    {
        public static readonly BindableProperty StartDateProperty = BindableProperty.Create(
            nameof(StartDate),
            typeof(DateTime),
            typeof(RangeCalendarView),
            DateTime.Today,
            BindingMode.TwoWay,
            propertyChanged: OnDateRangeChanged);

        public static readonly BindableProperty EndDateProperty = BindableProperty.Create(
            nameof(EndDate),
            typeof(DateTime),
            typeof(RangeCalendarView),
            DateTime.Today.AddDays(6),
            BindingMode.TwoWay,
            propertyChanged: OnDateRangeChanged);

        public static readonly BindableProperty MinDateProperty = BindableProperty.Create(
            nameof(MinDate),
            typeof(DateTime),
            typeof(RangeCalendarView),
            DateTime.Today,
            propertyChanged: OnMinMaxChanged);

        public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(
            nameof(MaxDate),
            typeof(DateTime),
            typeof(RangeCalendarView),
            DateTime.Today.AddDays(15),
            propertyChanged: OnMinMaxChanged);

        public static readonly BindableProperty MonthRangeTextProperty = BindableProperty.Create(
            nameof(MonthRangeText),
            typeof(string),
            typeof(RangeCalendarView),
            string.Empty);

        public static readonly BindableProperty CalendarDaysProperty = BindableProperty.Create(
            nameof(CalendarDays),
            typeof(ObservableCollection<CalendarDayModel>),
            typeof(RangeCalendarView),
            null);

        public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
            nameof(SelectedDate),
            typeof(DateTime),
            typeof(RangeCalendarView),
            DateTime.Now.Date,
            BindingMode.TwoWay,
            propertyChanged: OnSelectedDateChanged);

        private bool _isSelectingRange = false;

        public DateTime StartDate
        {
            get => (DateTime)GetValue(StartDateProperty);
            set => SetValue(StartDateProperty, value);
        }
        public DateTime EndDate
        {
            get => (DateTime)GetValue(EndDateProperty);
            set => SetValue(EndDateProperty, value);
        }

        public DateTime MinDate
        {
            get => (DateTime)GetValue(MinDateProperty);
            set => SetValue(MinDateProperty, value);
        }

        public DateTime MaxDate
        {
            get => (DateTime)GetValue(MaxDateProperty);
            set => SetValue(MaxDateProperty, value);
        }

        public string MonthRangeText
        {
            get => (string)GetValue(MonthRangeTextProperty);
            set => SetValue(MonthRangeTextProperty, value);
        }

        public ObservableCollection<CalendarDayModel> CalendarDays
        {
            get => (ObservableCollection<CalendarDayModel>)GetValue(CalendarDaysProperty);
            set => SetValue(CalendarDaysProperty, value);
        }

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public ICommand DayTappedCommand { get; }

        public RangeCalendarView()
        {
            DayTappedCommand = new RelayCommand<CalendarDayModel>(OnDayTapped);
            InitializeComponent();

            CalendarDays = new ObservableCollection<CalendarDayModel>();
            GenerateCalendar();
        }

        private static void OnDateRangeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RangeCalendarView control)
            {
                control.RefreshDayStates();
            }
        }
        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RangeCalendarView control)
            {
                control.SelectedDate = (DateTime)newValue;
            }
        }

        private static void OnMinMaxChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RangeCalendarView control)
            {
                control.GenerateCalendar();
            }
        }


        private void GenerateCalendar()
        {
            var min = MinDate.Date;
            var max = MaxDate.Date;

            if (max < min) max = min.AddDays(15);

            // Month and Year Range Header
            if (min.Month == max.Month && min.Year == max.Year)
            {
                MonthRangeText = min.ToString("MMMM yyyy");
            }
            else
            {
                MonthRangeText = $"{min.ToString("MMMM yyyy")} - {max.ToString("MMMM yyyy")}";
            }

            // Calculate boundary start (Sunday of MinDate week) and end (Saturday of MaxDate week)
            int startOffset = (int)min.DayOfWeek; // 0 for Sunday
            DateTime gridStart = min.AddDays(-startOffset);

            int endOffset = (int)max.DayOfWeek;
            DateTime gridEnd = max.AddDays(6 - endOffset);

            var days = new ObservableCollection<CalendarDayModel>();

            for (DateTime curr = gridStart; curr <= gridEnd; curr = curr.AddDays(1))
            {
                bool isSelectable = curr >= min && curr <= max;

                days.Add(new CalendarDayModel
                {
                    Date = curr,
                    DayText = isSelectable ? curr.Day.ToString() : string.Empty,
                    IsSelectable = isSelectable
                });
            }

            CalendarDays = days;
            RefreshDayStates();
        }

        private void OnDayTapped(CalendarDayModel? model)
        {
            if (model == null || !model.IsSelectable)
                return;

            DateTime tappedDate = model.Date;

            if (!_isSelectingRange)
            {
                // First tap: starts selection with single day
                StartDate = tappedDate;
                EndDate = tappedDate;
                _isSelectingRange = true;
            }
            else
            {
                // Second tap
                if (tappedDate >= StartDate.Date)
                {
                    EndDate = tappedDate;
                    _isSelectingRange = false; // Completed range
                }
                else
                {
                    // Tapped before StartDate, reset start to this date
                    StartDate = tappedDate;
                    EndDate = tappedDate;
                    _isSelectingRange = true;
                }
            }

            RefreshDayStates();
        }

        private void RefreshDayStates()
        {
            if (CalendarDays == null) return;

            var start = StartDate.Date;
            var end = EndDate.Date;
            if (end < start) end = start;

            foreach (var day in CalendarDays)
            {
                if (!day.IsSelectable || day.DayText == string.Empty)
                {
                    // Empty / padding day styling
                    day.BackgroundColor = Color.FromArgb("#34373D");
                    day.BorderColor = Color.FromArgb("#42464D");
                    day.TextColor = Colors.LightGray;
                    continue;
                }

                DateTime d = day.Date;

                if (d == start || d == end)
                {
                    // Selected Start or End: vibrant green highlight
                    day.BackgroundColor = Color.FromArgb("#BEFCA6");
                    day.BorderColor = Colors.White;
                    day.TextColor = Colors.Black;
                }
                else if (d > start && d < end)
                {
                    // In-between range days: soft green fill
                    day.BackgroundColor = Color.FromArgb("#BEFCA6");
                    day.BorderColor = Color.FromArgb("#88fa5c");
                    day.TextColor = Colors.White;
                }
                else
                {
                    // Normal selectable day: crisp white tile
                    day.BackgroundColor = Colors.White;
                    day.BorderColor = Color.FromArgb("#020600");
                    day.TextColor = Colors.Black;
                }
            }
        }
    }
}

