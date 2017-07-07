using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tripline.WebConsumer;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class LocationService: FileRepo<Location>
    {
        private GoogleClient _googleClient = new GoogleClient();

        public LocationService() : base(TripLineConfig.PhotoRepoPath)
        {
        }

        public Location GetLocation(float latitude, float longitude)
        {
            var response = _googleClient.GetReverseGeocoding(latitude, longitude);


            return new Location();
            

        }

        public Location GetLocation(string relativeUrl)
        {
            string[] names = relativeUrl.Split(new char[] { '\\' }).ToArray();
 
            Location loc = new Location();

            //_googleClient.
            //loc.Country = names[0];
            //loc.City = (names.Length >= 2) ? names[1] : string.Empty;
            //loc.State = (names.Length >= 2) ? names[1] : string.Empty;

            loc.Name = loc.City + (loc.Country.Length > 0 ? "," + loc.Country : "");
            return loc;
        }


        public bool IsInRange(Location location1, IEnumerable<Location> locations, long distanceKm)
        {
            return (locations.Any(l => GetDistanceKm(location1, l) < distanceKm));
        }

        public bool IsInRange(Location location1, Location location2, long distanceKm)
        {
            return (GetDistanceKm(location1, location2) < distanceKm);
        }


        public long GetDistanceKm(Location location1, Location location2)
        {
            if (location1.Equals(location2))
                return 0;
            else
            if (location1.Country == location2.Country && location1.State == location2.State)
                return 20;
            else
                return 100;
        }

    }
}
