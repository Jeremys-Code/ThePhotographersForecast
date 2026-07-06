using System;

namespace ThePhotographersForecast.Services.SunMoonCalc.Dtos
{
    /// <summary>
    /// Contains times for when the moon rises and sets each day
    /// </summary>
    public class MoonTimes
    {
        /// <summary>
        /// Time of moon rise
        /// </summary>
        public DateTime Rise { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Time of moon set
        /// </summary>
        public DateTime Set { get; set; } = DateTime.MinValue;

        /// <summary>
        /// True if the moon never rises/sets and is always above the horizon during the day
        /// </summary>
        public bool AlwaysUp { get; set; } = false;

        /// <summary>
        /// True if the moon is always below the horizon
        /// </summary>
        public bool AlwaysDown { get; set; } = false;
    }
}