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
                    return "Overnight";
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
        public static string TimesOfDayGreeting(int hour)
        {
            switch (hour)
            {
                case int h when (0 <= h && h < 3):
                    return "Good evening";
                case int h when (3 <= h && h < 12):
                    return "Good morning";
                case int h when (12 <= h && h < 18):
                    return "Good afternoon";
                case int h when (18 <= h && h < 24):
                    return "Good evening";
                default:
                    return string.Empty;
            }
        }


        //public string? IsTodayAHoliday()
        //{
        //    S
        //}
    }
}



//"Birthday of Martin Luther King, Jr."

//Third Monday in January

//Inauguration Day

//January 20, every 4 years following a presidential election

//"Washington's Birthday" 

//Also known as "Presidents Day"

//Third Monday in February

//"Memorial Day"

//Last Monday in May

//"Juneteenth National Independence Day"

//June 19

//"Independence Day"

//July 4

//"Labor Day"

//First Monday in September

//"Columbus Day"

//Second Monday in October

//"Veterans Day"

//November 11

//"Thanksgiving Day"

//Fourth Thursday in November

//"Christmas Day"

//December 25