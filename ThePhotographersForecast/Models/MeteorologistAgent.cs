namespace ThePhotographersForecast.Models
{
    public class MeteorologisticAgent
    {
        public DateTime BinDate { get; set; }
        public Bin EarlyMorning { get; set; } // 3-5
        public Bin Morning { get; set; } // 6-11
        public Bin Afternoon { get; set; } //12-17
        public Bin Evening { get; set; } //18-21
        public Bin Night { get; set; } //22-24
    }
    public class TempAgent
    {
        public DateTime BinDate { get; set; }
        public TempBin EarlyMorning { get; set; } // 3-5
        public TempBin Morning { get; set; } // 6-11
        public TempBin Afternoon { get; set; } //12-17
        public TempBin Evening { get; set; } //18-21
        public TempBin Night { get; set; } //22-24
    }

    public class Bin
    {
        public string Label { get; set; }
        // public MappedHourly[] Forecasts { get; set; }
        public List<double> Values { get; set; }
    }

    public class TempBin
    {
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public List<TempValues> Values { get; set; }
    }
    public class TempValues
    {
        public double Temp { get; set; }
        public double DewPoint { get; set; }
        public double RelativeHumidity { get; set; }
        public double Wbgt { get; set; }
    }

    // Build the Bins to use with the Engines Start

    //public ObservableCollection<MeteorologisticAgent> BuildDailyBins(NwsHourlySupplemental[] nws)
    //{

    //    // Filter hourlies for this day
    //    var groups = nws
    //        .OrderBy(h => h.Time)
    //        .GroupBy(t => (t.Time.Date, t.TimeOfDay))
    //        .ToDictionary(g => g.Key, g => g.Select(x => new TempValues { Temp = x.Temperature.Value, DewPoint = x.DewPoint.Value, RelativeHumidity = x.RelativeHumidity.Value, Wbgt = x.WetBulbGlobeTemperature.Value })
    //        .ToList());

    //    // ObservableCollection<MeteorologisticAgent> agents = new ObservableCollection<MeteorologisticAgent>();

    //    // Build the agent
    //    var agent = new TempAgent
    //    {
    //        BinDate = groups.Keys.First().Date,
    //        EarlyMorning = BuildTempBin("Early Morning", groups),
    //        Morning = BuildTempBin("Morning", groups),
    //        Afternoon = BuildTempBin("Afternoon", groups),
    //        Evening = BuildTempBin("Evening", groups),
    //        Night = BuildTempBin("Night", groups)
    //    };
    //    return agent;
    //}
    //private static TempBin BuildTempBin(DateTime date, string label, Dictionary<string, List<TempValues>> groups)
    //{
    //    if (!groups.TryGetValue(, out var list))
    //        return new TempBin { Date = date, Label = label, Values = new List<TempValues>() };
    //    return new TempBin();
    //}
    //private static TempBin TempBuildBin(DateTime date, string label, Dictionary<(DateTime, string), List<TempValues>> groups)
    //{
    //    if (!groups.TryGetValue((date, label), out var list))
    //        return new TempBin { Date = date, Label = label, Values = new List<TempValues>() };
    //    return new TempBin();
    //}

    //// Bins End

}
