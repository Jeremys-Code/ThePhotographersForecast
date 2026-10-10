using CommunityToolkit.Mvvm.ComponentModel;

namespace ThePhotographersForecast.Components
{
    public partial class CalendarDayModel : ObservableObject
    {
        public DateTime Date { get; set; }
        public string DayText { get; set; } = string.Empty;
        public bool IsSelectable { get; set; }

        [ObservableProperty]
        public partial Color BackgroundColor { get; set; } = Color.FromArgb("#BEFCA6");

        [ObservableProperty]
        public partial Color TextColor { get; set; } = Colors.Transparent;

        [ObservableProperty]
        public partial Color BorderColor { get; set; } = Color.FromArgb("#88fa5c");
    }
}

