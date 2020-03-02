using System;
using System.Globalization;

namespace Zen.Pebble.FlexibleData.DateTime
{
    public static class Extensions
    {
        public static System.DateTime? ToCalendar(this System.DateTime sourceDateTime, Calendar targetCalendar)
        {
            try
            {
                var referenceDateTime = new System.DateTime(
                    targetCalendar.GetYear(sourceDateTime),
                    targetCalendar.GetMonth(sourceDateTime),
                    targetCalendar.GetDayOfMonth(sourceDateTime),
                    targetCalendar.GetHour(sourceDateTime),
                    targetCalendar.GetMinute(sourceDateTime),
                    targetCalendar.GetSecond(sourceDateTime));
                return referenceDateTime;
            }
            catch (Exception e) { return null; }
        }
        public static System.DateTime? FromCalendar(this System.DateTime sourceDateTime, Calendar targetCalendar)
        {
            try
            {
                var referenceDateTime = targetCalendar.ToDateTime(
                    sourceDateTime.Year,
                    sourceDateTime.Month,
                    sourceDateTime.Day,
                    sourceDateTime.Hour,
                    sourceDateTime.Minute,
                    sourceDateTime.Second,
                    sourceDateTime.Millisecond
                );
                return referenceDateTime;
            }
            catch (Exception e) { return null; }
        }
    }
}