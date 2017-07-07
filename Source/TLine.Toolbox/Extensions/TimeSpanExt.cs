namespace TripLine.Toolbox.Extensions
{
    using System;

    public static class TimeSpanExt
    {
        public static TimeSpan Secs(this int time)
        {
            return TimeSpan.FromSeconds(time);
        }

        public static TimeSpan Mins(this int time)
        {
            return TimeSpan.FromMinutes(time);
        }

        public static TimeSpan Hours(this int time)
        {
            return TimeSpan.FromHours(time);
        }

        public static TimeSpan Days(this int time)
        {
            return TimeSpan.FromDays(time);
        }

        public static TimeSpan Milli(this int time)
        {
            return TimeSpan.FromMilliseconds(time);
        }
    }
}