using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Data
{
    public class AppUri
    {
        [PrimaryKey, AutoIncrement]
        public int SessionId { get; set; }
        public Guid DeviceId { get; set; }
        public Uri DailyForecastUri { get; set; }
        public Uri HourlyForecastUri { get; set; }
        public Uri DetailForecastUri { get; set; }
        public Uri AltForecastUri { get; set; }
        public Uri EventUri { get; set; }
    }
}
