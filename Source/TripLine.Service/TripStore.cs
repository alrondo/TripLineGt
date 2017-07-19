using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class TripItem
    {
        public int TripId { get; set; }

        public int NumPictures { get; set; }

        public long Weight { get; set; }

        public TripItem(int id)
        {
            TripId = id;
        }
    }

    public class TripByLocationGroup
    {
        public string GroupName { get; set; }

        public List<TripItem> Items { get; set; }

        public TripByLocationGroup(string groupName, List<TripItem> items)
        {
            GroupName = groupName;
            Items = items;
        }
    }

    public class TripByYearGroup
    {
        public int Year { get; set; }

        public List<TripItem> Items { get; set; }
    }


    public class TripStore
    {
        public List<TripByYearGroup> GetTripByYear()
        {
            throw new NotImplementedException();
        }

        public List<TripByLocationGroup> GetTripByCity()
        {
            List<TripByLocationGroup> listByYear = new List<TripByLocationGroup>();

            var res = _tripRepo.Content.Trips.GroupBy(t => t.Location.City).ToList();
            
            foreach (var grp in res)
            {
                var city = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i.Id)).ToList();
                listByYear.Add(new TripByLocationGroup(city, tripItems));
            }
            return listByYear;
        }


        private TripItem CreateTripItem (int tripId)
        {
            var titem = new TripItem(tripId);

            // get photos count...

            return titem;
        }
        private readonly LocationService _locationService;
        private readonly PhotoStore _photoStore;
        private readonly TripSmartBuilder _smartBuilder;

        private readonly TripsRepo _tripRepo;


        public TripStore(PhotoStore photoStore, LocationService locationService, TripSmartBuilder smartBuilder, TripsRepo tripRepo)
        {
            _photoStore = photoStore;
            _locationService = locationService;
            _smartBuilder = smartBuilder;
            _tripRepo = tripRepo;
        }

        public IEnumerable<TripCandidate> DetectNewTrips(List<PhotoSession> photoSessions)
        {
            _smartBuilder.Build(photoSessions);
            return _smartBuilder.BuildedTrips;
        }

                
        public void AddNewTrip(TripCandidate tripCandidate)
        {
            Trip trip= CreateTrip(tripCandidate);
            _tripRepo.Content.Trips.Add(trip);

            _tripRepo.Save();
        }


        public List<Trip> GetTrips(int maxCount) => _tripRepo.Content.Trips.Take(maxCount).ToList();

        public Trip GetTrip(int id) => _tripRepo.Content.Trips.First(t => t.Id == id);

        public List<Destination> GetDestinations(int maxCount)
        {
            List<Destination> destinations = new List<Destination>();

            foreach (var trip in _tripRepo.Content.Trips)
            {
                if (trip.Destinations.Count <= 1)
                    continue;

                destinations.AddRange(trip.Destinations.Take(maxCount));

                if (destinations.Count > maxCount)
                    break;
            }

            return destinations;
        }

        public Trip CreateTrip(TripCandidate tripCandidate)
        {
            Trip trip = ServiceMapper.Map<Trip>(tripCandidate);

            trip.Id = _tripRepo.Content.NewId++;

            foreach (var sessionId in tripCandidate.PhotoSessions.Select(s => s.SessionId))
            {
                int? destinationId = null;

                var destination =
                    tripCandidate.Destinations.FirstOrDefault(d => d.Sessions.Any(s => s.SessionId == sessionId));

                if (destination != null)
                    destinationId = destination.Id;

                _photoStore.ConfirmPhotoSession(sessionId, trip.Id, destinationId);
            }

            return trip;
        }


    }

}
