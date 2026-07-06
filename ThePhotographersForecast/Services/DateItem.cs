using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Services
{
    public class DateItem
    {
        public DateTime PhotoDate { get; set; }
        public string DateDisplay => this.PhotoDate.ToString("ddd, MMM d");
    }
}
