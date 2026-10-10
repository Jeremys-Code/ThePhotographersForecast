namespace ThePhotographersForecast.Models;

// A PhotoDay organizes daily and hourly weather with event data by date.
public class PhotoDay
{
    public int Day { get; set; }
    public DateTime Date { get; set; }
    public bool IsEmpty { get; set; }
    public bool IsEventDay { get; set; }
    public bool IsEventWindow { get; set; }
    public bool IsEventWindowDay => (this.Hourlies.Count() > 0) ? true : false;
    public bool IsFlipped { get; set; }

    public bool IsDay => (this.Daily.IsDay) ? true : false;
    public bool IsToday { get; set; }
    public MappedDaily Daily { get; set; }
    public MappedHourly[] Hourlies { get; set; }

    //  public ObservableCollection<IGrouping<GroupedHourly, Hourly>> Groups { get; set; }
}



