using System.Collections.Generic;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class TripAlbum : Album
    {
        public Trip  Trip { get; private set; }
        
        public TripAlbum(Trip trip, List<AlbumSection> sections) : base ( trip.DisplayName, sections )
        {
            Trip = trip;
        }       
    }

    public class PlaceAlbum : Album
    {
        public VisitedPlace Place { get; private set; }

        public PlaceAlbum(VisitedPlace place, List<AlbumSection> sections) : base(place.PlaceName, sections)
        {
            Place = place;
        }
    }
}