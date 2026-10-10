using System;

namespace SunCalcSharp
{
    /// <summary>
    /// Contains times for each of the events of the sun on a given day
    /// </summary>
    public class SunTimes
    {
        /// <summary>
        /// sunrise (top edge of the sun appears on the horizon)
        /// </summary>
        public DateTime Sunrise;

        /// <summary>
        /// sunrise ends (bottom edge of the sun touches the horizon)
        /// </summary>
        public DateTime SunriseEnd;

        /// <summary>
        /// morning golden hour (soft light, best time for photography) ends
        /// </summary>
        public DateTime GoldenHourEnds;

        /// <summary>
        /// solar noon (sun is in the highest position)
        /// </summary>
        public DateTime SolarNoon;

        /// <summary>
        /// evening golden hour starts
        /// </summary>
        public DateTime GoldenHourBegins;

        /// <summary>
        /// sunset starts (bottom edge of the sun touches the horizon)
        /// </summary>
        public DateTime SunsetBegins;

        /// <summary>
        /// sunset (sun disappears below the horizon, evening civil twilight starts)
        /// </summary>
        public DateTime Sunset;


        /// <summary>
        /// blue hour (sun is 4 degrees below the horizon)
        /// </summary>
        public DateTime BlueHourBegins;


        /// <summary>
        /// blue hour ends (sun is 6 degrees below the horizon)
        /// </summary>
        public DateTime BlueHourEnds;

        /// <summary>
        /// dusk (evening nautical twilight starts)
        /// </summary>
        public DateTime CivilTwilightEnds;

        /// <summary>
        /// nautical dusk (evening astronomical twilight starts)
        /// </summary>
        public DateTime NauticalTwilightEnds;

        /// <summary>
        /// night starts (dark enough for astronomical observations)
        /// </summary>
        public DateTime AstronomicalTwilightEnds;

        /// <summary>
        /// nadir (darkest moment of the night, sun is in the lowest position)
        /// </summary>
        public DateTime Midnight;

        /// <summary>
        /// night ends (morning astronomical twilight starts)
        /// </summary>
        public DateTime AstronomicalTwilightBegins;

        /// <summary>
        /// nautical dawn (morning nautical twilight starts)
        /// </summary>
        public DateTime NauticalTwilightBegins;

        /// <summary>
        /// dawn (morning nautical twilight ends, morning civil twilight starts)
        /// </summary>
        public DateTime CivilTwilightBegins;
    }
}