using System.Collections.Generic;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class LocationAlbum : Album
    {
        public Location Location { get; private set; }

        //todo  visits...

        public LocationAlbum(Location location, List<AlbumSection> sections) : base(location.DisplayName, sections)
        {
            Location = location;
        }
    }
}