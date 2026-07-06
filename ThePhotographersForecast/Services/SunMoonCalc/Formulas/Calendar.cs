using System;

namespace ThePhotographersForecast.Services.SunMoonCalc.Formulas
{
    internal static class Calendar
    {
        // date/time constants and conversions
        public const double dayMs = 86400000;
        public const double J1970 = 2440588;
        public const double J2000 = 2451545;

        public static double ToJulian(DateTime date)
        {
            DateTime j2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSinceJ2000 = date - j2000;
            return 2451545.0 + timeSinceJ2000.TotalDays;
        }

        public static DateTime FromJulian(double j, TimeZoneInfo timeZone)
        {
            DateTime j2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            TimeSpan ts = TimeSpan.FromDays(j - 2451545.0);
            DateTime utcTime = j2000.Add(ts);

            // Convert UTC to local time zone
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
        }

        public static double ToDays(DateTime date)
        {
            DateTime j2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSinceJ2000 = date - j2000;
            return 2451545.0 + timeSinceJ2000.TotalDays;
        }
    }
}