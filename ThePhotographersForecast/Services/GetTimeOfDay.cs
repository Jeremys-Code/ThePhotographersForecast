using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Services
{
    public static class GetTimeOfDay
    {
        public static string TimeOfDay(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 3):
                    return "Night";
                case int h when (3 <= h && h < 6):
                    return "Early Morning";
                case int h when (6 <= h && h < 12):
                    return "Morning";
                case int h when (12 <= h && h < 18):
                    return "Afternoon";
                case int h when (18 <= h && h < 22):
                    return "Evening";
                case int h when (22 <= h && h < 24):
                    return "Night";
                default:
                    return string.Empty;
            }
        }
    }
}
