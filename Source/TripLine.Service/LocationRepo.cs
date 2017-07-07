using System.Collections.Generic;
using System.IO;
using System.Linq;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service
{

    public class LocationRepoContent
    {
        public LocationRepoContent() { }

        public int NewId { get; set; } = 1;

        public int HomeLocationId { get; set; } = 0;

        public List<Location> Locations { get; set; } = new List<Location>();

    }

    public class LocationRepo : FileRepo<LocationRepoContent>
    {
        public LocationRepo() : this(TripLineConfig.LocationRepoPath, forceNew:false)
        {
        }

        public LocationRepo(string path, bool forceNew=false) : base(path, forceNew)
        {
            base.Load();
        }
       

        public List<Location> Locations
        {
            get { return base.Content.Locations; }
        }

        public Location GetLocation(int id)
        {
            return Locations.FirstOrDefault(l => l.Id == id);
        }

        public Location GetHomeLocation() => GetLocation(Content.HomeLocationId);


        public Location GetLocation(float latitude, float longitude)
        {
            var location = Locations.FirstOrDefault(l => l.SearchedPosition != null && l.SearchedPosition.IsAlike(latitude, longitude, 5));
            if (location != null)
                return location;

            return Locations.FirstOrDefault(l => l.Position != null && l.Position.IsAlike(latitude, longitude)  );
        }

        public Location GetLocationBySearchedAddress(string searchedAddress)
        {
            if (string.IsNullOrEmpty(searchedAddress))
                return null;
            return Locations.FirstOrDefault(l => l.SearchedAddress.EqualLt(searchedAddress));
        }

        public Location GetLocationBySearchedPosition(GeoPosition position)
        {

            if (position ==null)
                return null;

            return Locations.FirstOrDefault(l => l.SearchedPosition.IsAlike(position.Latitude, position.Longitude));
        }

        public void Add(Location location)
        {
            Locations.Add(location);
        }

        public int GetNewId()
        {
            return Content.NewId++;
        }
    }
}