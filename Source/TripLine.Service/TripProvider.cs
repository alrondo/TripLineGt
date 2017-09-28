using System;
using System.Collections.Generic;
using System.Linq;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class TripFilter
    {
        public string Name { get; set; }


        private Func<Trip, bool> IsMatch { get; }

        //Func<VisitedPlace, bool> filter = type != null
        //    ? (Func<IEnumeable<Trip>)  delegate (VisitedPlace p) { return p.Types.Contains(type); }
        //    : null;
        //public List<Trip> Trips { get; set; }

        public TripFilter(string Name, Func<Trip, bool> isMatch)
        {
            IsMatch = isMatch;
        }


        public static Func<Trip, bool> WithManyDestination = delegate (Trip t)
        {
            return t.Destinations?.Count() > 1;
        };

        public static Func<Trip, bool> WeekendType = delegate (Trip t)
        {
            return t.Duration.TotalDays <= 3;
        };

    }

    public class TripProvider
    {
        private Func<Trip, int> groupByDestinationCount = delegate(Trip t) { return t.Destinations.Count(); };
        
        public IEnumerable<IEnumerable<Trip>> GetTripsBy(List<Trip> trips)
        {
            var groups = trips.GroupBy(groupByDestinationCount).ToList();

            foreach (var group in groups)
            {
                yield return group;
            }



        }
    }
}