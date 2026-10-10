using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Services
{
    public class DateItem
    {
        public DateTime Date { get; set; }
        public string DateDisplay => this.Date.ToString("ddd, MMM d");
    }
}
