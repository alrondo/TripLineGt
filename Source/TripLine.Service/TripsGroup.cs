using System.Collections.Generic;

namespace TripLine.Service
{
    public class TripsGroup
    {
        public string GroupName { get; set; }

        public List<TripItem> Items { get; set; }

        public TripsGroup(string groupName, List<TripItem> items)
        {
            GroupName = groupName;
            Items = items;
        }
    }
}