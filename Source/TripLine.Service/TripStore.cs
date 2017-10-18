using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper.Execution;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class TripStore
    {
        public void Remove(int tripId)
        {
            var trip = GetTrip(tripId);

            _tripRepo.Content.Trips.Remove(trip);
            _tripRepo.Save();
        }
    
        public List<TripsGroup> GetTripByCity()
        {
            List<TripsGroup> tripsByCity = new List<TripsGroup>();

            var res = _tripRepo.Content.Trips.GroupBy(t => t.Location.City).ToList();
            
            foreach (var grp in res)
            {
                var city = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByCity.Add(new TripsGroup(city, tripItems));
            }
            return tripsByCity;
        }

        public List<TripsGroup> GetTripByLocationName()
        {
            List<TripsGroup> tripsByLocation = new List<TripsGroup>();

            var res = _tripRepo.Content.Trips.Where( t => t.Location != null).GroupBy(t => t.Location.DisplayName).ToList();

            foreach (var grp in res)
            {
                var locationName = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByLocation.Add(new TripsGroup(locationName, tripItems));
            }
            return tripsByLocation;
        }



        public List<TripsGroup> GetTripByCountry()
        {
            List<TripsGroup> tripsByLocation = new List<TripsGroup>();

            var res = _tripRepo.Content.Trips.Where(t => t.Location != null 
                                                    && !string.IsNullOrEmpty(t.Location.Country) )
                                                    .GroupBy(t => t.Location.Country).ToList();

            foreach (var grp in res)
            {
                var groupName = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByLocation.Add(new TripsGroup(groupName, tripItems));
            }
            return tripsByLocation;
        }


        public List<TripsGroup> GetTripByYear()
        {
            List<TripsGroup> tripsByDate = new List<TripsGroup>();

            var res = _tripRepo.Content.Trips.Where(t => t.Location != null).GroupBy(t => t.Date.Year).ToList();

            foreach (var grp in res)
            {
                var groupName = grp.Key.ToString();

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByDate.Add(new TripsGroup(groupName , tripItems));
            }
            return tripsByDate;
        }


        public List<VisitedPlace> GetPlaces()
        {
            var photosByPlaces = _photoStore.GetPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId);
            var places = photosByPlaces.Select(g => GetPlace(g.Key));
            return places.ToList();
        }

        public List<VisitedPlace> GetPlacesWithMostPhotos()
        {
            List<PlacesGroup> groupOfPlaces = new List<PlacesGroup>();

            var photosByPlaces = _photoStore.GetPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId);

            photosByPlaces = photosByPlaces.OrderByDescending(l => l.Count());

            var places = photosByPlaces.Select(g => GetPlace(g.Key));
            return places.ToList();
        }
        

        private TripItem CreateTripItem (Trip  trip)
        {
            var titem = new TripItem(trip.Id);

            // get photos count...
            var photos = _photoStore.GetPhotosByTrip(trip.Id);
            titem.DisplayName = trip.DisplayName;
            titem.NumPictures = photos.Count;
            titem.CoverPhoto = photos.First();
            return titem;
        }


        private PlaceItem CreatePlaceItem(VisitedPlace place)
        {
            var titem = new PlaceItem(place.Id);

            // get photos count...
            var photos = _photoStore.GetPhotosByTrip(place.Id);

            titem.DisplayName = place.PlaceName;
            titem.NumPictures = photos.Count;
            titem.CoverPhoto = photos.First();

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

        public int GetPlacesCount() => _photoStore.GetTravelPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId).Count();
       
        public List<Trip> GetTrips(int maxCount=int.MaxValue) => _tripRepo.Content.Trips.Take(maxCount).ToList();

        public Trip GetTrip(int id) => _tripRepo.Content.Trips.FirstOrDefault(t => t.Id == id);

        public VisitedPlace GetPlace(int id) => _locationService.GetPlace(id);


        public Destination GetDestination(int destinationId) => GetDestinations().FirstOrDefault (d => d.Id == destinationId);

        public IEnumerable<Destination> GetDestinations() => _tripRepo.Content.Trips.SelectMany(t => t.Destinations);
        

        public Trip CreateTrip(TripCandidate tripCandidate)
        {

            Trip newTrip;
            try
            {
                newTrip = ServiceMapper.Map<Trip>(tripCandidate);

                newTrip.Id = _tripRepo.GetNewId();

                foreach (var sessionId in tripCandidate.PhotoSessions.Select(s => s.SessionId))
                {
                    int? destinationId = null;

                    var destination =
                        tripCandidate.Destinations.FirstOrDefault(d => d.Sessions.Any(s => s.SessionId == sessionId));

                    if (destination != null)
                        destinationId = destination.Id;

                    _photoStore.ConfirmPhotoSession(sessionId, newTrip.Id, destinationId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           

            return newTrip;
        }

        public void DumpTrip(int tripId, string prefix=" ")
        {
            var trip = GetTrip(tripId);
            
            trip.Dump(prefix);

            var photos = _photoStore.GetPhotosByTrip(trip.Id);

            photos.FirstOrDefault()?.Dump("first");
            photos.LastOrDefault()?.Dump("last");

        }


    }

}
