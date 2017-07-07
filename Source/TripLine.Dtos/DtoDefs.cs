using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Dtos
{
    
    public static class DtoDefs
    {
        public const string DateFormatter = "dd MMM yy";

        public const int MinSupportedYear = 1995;
    }

    public interface IDateRange
    {
        DateTime FromDate { get; set; }
        DateTime ToDate { get; set; }
    }

    public static class DisplayFormater
    {
        public static string GetDate(DateTime dateTime) => dateTime.ToString(DtoDefs.DateFormatter);
    }


    public static class DateHelper
    {
        public static bool IsValidDate(DateTime dt) => dt.Year > DtoDefs.MinSupportedYear;

        public static DateTime HighestDate(DateTime dt1, DateTime dt2)
        {
            if (IsValidDate(dt1) != IsValidDate(dt2))
                return IsValidDate(dt1) ? dt1 : dt2;  // valid one

            return dt1 > dt2 ? dt1 : dt2;
        }
        
        public static DateTime LowestDate(DateTime dt1, DateTime dt2)
        {
        
            if (IsValidDate(dt1) != IsValidDate(dt2))
                return IsValidDate(dt1) ? dt1 : dt2;  // valid one

            return (dt1 < dt2) ? dt1 : dt2;  // lower one
        }
    }

}
