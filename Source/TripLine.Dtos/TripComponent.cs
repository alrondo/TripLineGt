using Newtonsoft.Json;
using System;

namespace TripLine.Dtos
{
    public class TripComponent : ITripComponent
    {
        // IDateRange
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public TimeSpan Duration => (ToDate - FromDate);

        public int Id { get; set; }

        public Location Location { get; set; }

        public string DisplayName { get; set; } = string.Empty;


        public DateTime Date => GetMostRelevantDate(FromDate, ToDate);


        private DateTime GetMostRelevantDate(DateTime from, DateTime to)
        {
 
            if (@from.Year == to.Year && @from.Month != to.Month)
                return new DateTime(@from.Year, @from.Month, @from.Day);

            DateTime theDateToPick;

            if (from.Year != to.Year)
            {
                int daysInYear = DateTime.IsLeapYear(from.Year) ? 366 : 365;
                int daysLeftInYear = daysInYear - from.DayOfYear; // Result is in range 0-365.

                theDateToPick = (daysLeftInYear > to.DayOfYear) ? from : to;
            }
            else
            {
                int daysInMounth = DateTime.DaysInMonth(from.Year, from.Month);
                int daysLeftInMounth = daysInMounth - from.Day;

                theDateToPick = (daysLeftInMounth > to.Day) ? from : to;
            }
            return new DateTime( theDateToPick.Year, theDateToPick.Month, 1); 
        }

        public string Serialize(bool pretty = false)
        {
            Formatting formatting = pretty ? Formatting.Indented : Formatting.None;

            string str = JsonConvert.SerializeObject(this, formatting);

            if (!pretty)
            {
                str = str.Replace(@"},", (@"}," + Environment.NewLine));
                str = str.Replace(@"[{,", (Environment.NewLine + @"[{"));
            }

            return str;
        }

    }
}