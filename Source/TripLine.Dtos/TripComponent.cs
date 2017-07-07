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
    }
}