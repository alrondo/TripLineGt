using System.Collections.Generic;
using System.Linq;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class PlaceRepoContent
    {
        public PlaceRepoContent() { }

        public int NewId { get; set; } = 1;

        public int HomeLocationId { get; set; } = 0;

        public List<VisitedPlace> VisitedPlaces { get; set; } = new List<VisitedPlace>();
    }

    public class PlaceRepo : FileRepo<PlaceRepoContent>
    {
        public PlaceRepo() : this(TripLineConfig.PlaceRepoPath, forceNew: false)
        {
        }

        public PlaceRepo(string path, bool forceNew = false) : base(path, forceNew)
        {
            base.Load();
        }

        public List<VisitedPlace> VisitedPlaces
        {
            get { return base.Content.VisitedPlaces; }
        }


        public VisitedPlace GetPlace(int id)
        {
            return VisitedPlaces.FirstOrDefault(l => l.Id == id);
        }

        
        public void Add(VisitedPlace place)
        {
            VisitedPlaces.Add(place);
        }

        public int GetNewId()
        {
            return Content.NewId++;
        }
    }
}