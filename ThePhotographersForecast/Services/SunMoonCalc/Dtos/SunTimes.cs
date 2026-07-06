namespace ThePhotographersForecast.Services.SunMoonCalc.Dtos
{
    /// <summary>
    /// Contains times for each of the events of the sun on a given day
    /// </summary>
    public class SunTimes
    {
        /// <summary>
        /// sunrise (top edge of the sun appears on the horizon)
        /// </summary>
        public DateTime Sunrise { get; set; } = DateTime.MinValue;

        /// <summary>
        /// sunrise ends (bottom edge of the sun touches the horizon)
        /// </summary>
        public DateTime SunriseEnd { get; set; } = DateTime.MinValue;

        /// <summary>
        /// morning golden hour (soft light, best time for photography) ends
        /// </summary>
        public DateTime GoldenHourEnd { get; set; } = DateTime.MinValue;

        /// <summary>
        /// solar noon (sun is in the highest position)
        /// </summary>
        public DateTime SolarNoon { get; set; } = DateTime.MinValue;

        /// <summary>
        /// evening golden hour starts
        /// </summary>
        public DateTime GoldenHour { get; set; } = DateTime.MinValue;

        /// <summary>
        /// sunset starts (bottom edge of the sun touches the horizon)
        /// </summary>
        public DateTime SunsetStart { get; set; } = DateTime.MinValue;

        /// <summary>
        /// sunset (sun disappears below the horizon, evening civil twilight starts)
        /// </summary>
        public DateTime Sunset { get; set; } = DateTime.MinValue;

        /// <summary>
        /// blue hour (sun is 4 degrees below the horizon)
        /// </summary>
        public DateTime BlueHour { get; set; } = DateTime.MinValue;

        /// <summary>
        /// blue hour ends (sun is 6 degrees below the horizon)
        /// </summary>
        public DateTime BlueHourEnd { get; set; } = DateTime.MinValue;

        /// <summary>
        /// dusk (evening nautical twilight starts)
        /// </summary>
        public DateTime Dusk { get; set; } = DateTime.MinValue;

        /// <summary>
        /// nautical dusk (evening astronomical twilight starts)
        /// </summary>
        public DateTime NauticalDusk { get; set; } = DateTime.MinValue;

        /// <summary>
        /// night starts (dark enough for astronomical observations)
        /// </summary>
        public DateTime Night { get; set; } = DateTime.MinValue;

        /// <summary>
        /// nadir (darkest moment of the night, sun is in the lowest position)
        /// </summary>
        public DateTime Nadir { get; set; } = DateTime.MinValue;

        /// <summary>
        /// night ends (morning astronomical twilight starts)
        /// </summary>
        public DateTime NightEnd { get; set; } = DateTime.MinValue;

        /// <summary>
        /// nautical dawn (morning nautical twilight starts)
        /// </summary>
        public DateTime NauticalDawn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// dawn (morning nautical twilight ends, morning civil twilight starts)
        /// </summary>
        public DateTime Dawn;
    }
}