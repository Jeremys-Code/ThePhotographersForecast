using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Models
{
    public class MeteorologisticAgent
    {
        public DateTime BinDate { get; set; }
        public Bin Overnight { get; set; } // 0-2
        public Bin EarlyMorning { get; set; } // 3-5
        public Bin Morning { get; set; } // 6-11
        public Bin Afternoon { get; set; } //12-17
        public Bin Evening { get; set; } //18-21
        public Bin Night { get; set; } //22-24
    }

    public class Bin
    {
        public string Label { get; set; }
        public IEnumerable<double> Values { get; set; }
    }
}
