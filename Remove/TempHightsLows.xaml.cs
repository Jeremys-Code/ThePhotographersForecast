using ThePhotographersForecast.Components;

namespace ThePhotographersForecast.ViewModels.Compnents;

public partial class TempHightsLows : ContentView
{
    public TempHightsLows()
    {

         public static readonly BindableProperty HighTemp = BindableProperty.Create(
            nameof(HighTemp),
            typeof(DateTime),
            typeof(RangeCalendarView),
            DateTime.Today,
            BindingMode.TwoWay,
            propertyChanged: OnDateRangeChanged);

    InitializeComponent();
}
}