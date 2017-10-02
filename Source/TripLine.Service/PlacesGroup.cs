using System.Collections.Generic;

namespace TripLine.Service
{
    public class PlacesGroup
    {
        public string Title;

        public List<PlaceItem> Items { get; set; }


        public PlacesGroup(int year, List<PlaceItem> items)
        {
            Items = items;
        }
    }
}