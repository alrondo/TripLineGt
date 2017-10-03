using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service
{

    public class HighliteService 
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly TripStore _tripStore;
        private readonly LocationService _locationService;
        private readonly RandomPhotoProvider _randomPhotoProvider;

        public HighliteService(PhotoStore photoStore, TripStore tripStore, 
            LocationService locationService) 
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
            _locationService = locationService;
            _randomPhotoProvider = new RandomPhotoProvider();
        }

        private HighliteSelectOptions _selectOptions = null;
        
        public List<HighliteTopic> GetHighlites(HighliteSelectOptions selectOptions=null)
        {
            _selectOptions = selectOptions ?? new HighliteSelectOptions();

            if (! _selectOptions.Target.HasValue)
            {
                return GetRandomHighlites();
            }

            List<HighliteTopic> topics = null;

            switch (_selectOptions.Target.Value)
            {
                case HighliteTarget.Trip:
                    topics = CreateTopicsForAllTrips();
                    break;

                case HighliteTarget.Place:
                    topics = CreateTopicsForAllPlaces();
                    break;

                case HighliteTarget.Location:
                    topics = CreateTopicsForAllLocations();
                    break;

                default:
                    throw new NotImplementedException();
            }

            return topics;
        }


        private List<HighliteTopic> GetRandomHighlites()
        {
            var tripByLocationName = _tripStore.GetTripByLocationName();

            var topic1 = CreateTopicForTripsGroup("Visited locations", tripByLocationName);

            var tripByLocationCountry = _tripStore.GetTripByCountry();

            var topic2 = CreateTopicForTripsGroup("Visited country", tripByLocationCountry);

            var trips = _tripStore.GetTrips();

            var latestTrips = trips.OrderByDescending(t => t.FromDate).Take(3);

            var oldestTrips = trips.OrderBy(t => t.FromDate).Take(3);

            var topic3 = CreateTopicForTrips("Latest trips", latestTrips);

            var topic4 = CreateTopicForTrips("Long time ago", oldestTrips);

            var topic5 = CreateTopicWithMostPhtographedPlace();

            var topics = new List<HighliteTopic>()
            {
                topic1,
                topic2,
                topic3,
                topic4,
                topic5
            };

            return topics;
        }

        private List<HighliteTopic> CreateTopicsForAllTrips()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var trips = _tripStore.GetTrips();

            foreach (var trip in trips)
            {
                var topic = CreateTopicForTrip($"Trip to {trip.GetDisplayName(withDate:true)}",  trip);

                topics.Add(topic);
            }

            return topics;
        }

        private List<HighliteTopic> CreateTopicsForAllPlaces()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var places = _tripStore.GetPlaces();

            foreach (var trip in places)
            {
                var items = places.Select(pl => DoCreateHighliteItem(pl.Id, PickPlacePhoto(pl.Id), CountPlacePhoto(pl.Id),
                    HighliteTarget.Place, string.Empty));

                var topic = new HighliteTopic("Most photographed places", items.ToList());

                topics.Add(topic);
            }

            return topics;
        }

        private HighliteTopic CreateTopicWithMostPhtographedPlace()
        {
            var places = _tripStore.GetPlaces();

            places = places.OrderByDescending(p => p.NumPhotos).ToList();

            places = places.Take(5).ToList();

            var items = places.Select(pl => DoCreateHighliteItem(pl.Id, PickPlacePhoto(pl.Id), CountPlacePhoto(pl.Id),
                HighliteTarget.Place, string.Empty));

            
            var topic = new HighliteTopic("Most photographed places", items.ToList());

            return topic;
        }

    
        private List<HighliteTopic> CreateTopicsForAllLocations()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var locations = _locationService.GetLocations();

            foreach (var location in locations)
            {
                var topic = CreateTopicForLocation($"{location.DisplayName}", location);

                topics.Add(topic);
            }

            return topics;
        }

        private HighliteTopic CreateTopicForTripsGroup(string topicName, IEnumerable<TripsGroup> tripByLocationGroup, int count=5)
        {
            var tripItems = tripByLocationGroup.Select(g => g.Items.First());

            var items = tripItems.Select(x => CreateHighliteItem(x));
            
            return new HighliteTopic(topicName, items.ToList());
        }

        
        private HighliteTopic CreateTopicForTrips(string topicName, IEnumerable<Trip> trips)
        {
            var items = trips.Select(x => CreateHighliteItem(x));

            return new HighliteTopic(topicName, items.ToList());
        }    

      
        private HighliteTopic CreateTopicForTrip(string topicName, Trip trip)
        {
            var photos =    _photoStore.GetPhotosByTrip(trip.Id);
            var photosByDate = photos.GroupBy(p => GetDayNumber(trip, p));

            var highliteItems = photosByDate.Select(g => DoCreateHighliteItem(
                trip.Id,
                PickPhoto(g),
                g.Count(),
                HighliteTarget.Trip,
                $"Day {g.Key} {g.First().Location.City ?? @"N/A"} ")).ToList();

            // on photo per day
           return new HighliteTopic(topicName, highliteItems);
        }

        private HighliteTopic CreateTopicForLocation(string topicName, Location location)
        {
            var photos = _photoStore.GetPhotosAtLocation(location.Id);
            var photosByDate = photos.GroupBy(p => p.Creation.ToShortDateString());

            var highliteItems = photosByDate.Select(g => DoCreateHighliteItem(
                location.Id,
                PickPhoto(g),
                g.Count(),
                HighliteTarget.Photos,
                $"{g.Key}")).ToList();
            
            return new HighliteTopic(topicName, highliteItems);
        }

        private IHighliteItem CreateHighliteItem(TripItem titem)
            => DoCreateHighliteItem(titem.TripId, titem.CoverPhoto, titem.NumPictures, HighliteTarget.Trip, titem.DisplayName);

        private IHighliteItem CreateHighliteItem(PlaceItem titem)
            => DoCreateHighliteItem(titem.PlaceId, titem.CoverPhoto, titem.NumPictures, HighliteTarget.Trip, titem.DisplayName);

        private IHighliteItem CreateHighliteItem(Trip trip, Photo photo, int count)
            => DoCreateHighliteItem(trip.Id, photo, count, HighliteTarget.Trip, trip.DisplayName);


        private IHighliteItem CreateHighliteItem(Trip trip)
        {
            var photos = _photoStore.GetPhotosByTrip(trip.Id);
            var photo = PickPhoto(photos);

            return CreateHighliteItem(trip, photo, photos.Count);
        }

        private IHighliteItem DoCreateHighliteItem(int targetId, Photo photo, int count, HighliteTarget target, string title)
        {
            Debug.Assert(photo.Location != null);

            var item = new HighliteItem()
            {
                TargetId = targetId,
                DisplayName = title,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl,
                Description = target.GetDescription()
            };
            return item;
        }


        Photo PickPhoto(IEnumerable<Photo> photos)
        {
            var photo = _randomPhotoProvider.GetRandomPhotos(photos.ToList(), 1).FirstOrDefault();
            return photo ?? photos.First();
        }

        Photo PickPlacePhoto(int placeId) => PickPhoto(_photoStore.GetPhotosByPlace(placeId));

        int CountPlacePhoto(int placeId) => _photoStore.GetPhotosByPlace(placeId).Count;


        int GetDayNumber(Trip trip, Photo photo)
        {
            var diff = photo.Creation - trip.FromDate;

            return (int)diff.TotalDays;
        }
    }

  
}
