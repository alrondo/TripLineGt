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


        public HighliteService(PhotoStore photoStore, TripStore tripStore, LocationService locationService) 
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
            _locationService = locationService;
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
                    topics = GetTripHighlites();
                    break;

                case HighliteTarget.Place:
                    topics = GetPlaceHighlites();
                    break;

                case HighliteTarget.Location:
                    topics = GetLocationHighlites();
                    break;


                default:
                    throw new NotImplementedException();
            }

            return topics;
        }

        private List<HighliteTopic> GetRandomHighlites()
        {
            List<string> choseLocationNames = new List<string>();

            var photos = _photoStore.GetPhotos().Where(p => p.Location != null).ToList();


            var tripByLocationName = _tripStore.GetTripByLocationName();

            var topic1 = CreateHighliteTopicViewModel("Visited locations", tripByLocationName);


            var tripByLocationCountry = _tripStore.GetTripByCountry();

            var topic2 = CreateHighliteTopicViewModel("Visited country", tripByLocationCountry);

            var trips = _tripStore.GetTrips();

            var latestTrips = trips.OrderByDescending(t => t.FromDate).Take(3);

            var oldestTrips = trips.OrderBy(t => t.FromDate).Take(3);

            var topic3 = CreateHighliteTopicViewModel("Latest trips", latestTrips);

            var topic4 = CreateHighliteTopicViewModel("Long time ago", oldestTrips);

            var topic5 = GetMostPhotographPlaceHighlite("Most photograph place", type: null);

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



        private List<HighliteTopic> GetTripHighlites()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var trips = _tripStore.GetTrips();

            foreach (var trip in trips)
            {
                var topic = CreateHighliteTopicViewModelForTrip($"Trip to {trip.GetDisplayName(withDate:true)}",  trip);

                topics.Add(topic);
            }

            return topics;
        }



 

        private HighliteTopic GetMostPhotographPlaceHighlite( string topicName, string type=null)
        {
            Func<VisitedPlace, bool> filter = type != null
                ? (Func < VisitedPlace, bool> ) delegate (VisitedPlace p) { return p.Types.Contains(type); }
                : null;
            
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var photosByPlaces = _photoStore.GetPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId);
  
            VisitedPlace selPlace = null;

            IEnumerable<Photo> photos = new List<Photo>();

            foreach (var group in photosByPlaces)
            {
                var place = _locationService.GetPlace(group.Key);

                if (place == null)
                    continue;

                if (  filter != null && !filter(place))
                    continue;

                if (photos ==null || group.Count() > photos.Count() )
                {
                    selPlace = place;
                    photos = group;
                }     
                                           
            }
            var items = photos.Select(p => DoCreateHighliteItem(p.Id, p, 0, HighliteTarget.Place, string.Empty));
            var topic = new HighliteTopic(topicName, items.ToList());

            return topic;
        }

        private List<HighliteTopic> GetPlaceHighlites()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var photosByPlaces = _photoStore.GetPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId);

            Debug.Assert(photosByPlaces.Count() >= 1);         
            return GetPlaceHighlites (photosByPlaces);
        }

        private List<HighliteTopic> GetPlaceHighlites(IEnumerable<IGrouping<int, Photo>> photoGroups, 
            Func<VisitedPlace, bool> placeFilter=null)
        {
            if (placeFilter==null)
                placeFilter = delegate (VisitedPlace p) { return true; };
            List<HighliteTopic> topics = new List<HighliteTopic>();

            foreach (var group in photoGroups)
            {
                var place = _locationService.GetPlace(group.Key);

                if (place == null)
                    continue;

                if (!placeFilter(place))
                    continue;

                var photos = PickPhotos(group);

                if (!photos.Any())
                    continue;

                var items = photos.Select(p => DoCreateHighliteItem(p.Id, p, 0, HighliteTarget.Place, string.Empty));

                var itemsList = items.ToList();

                var topic = new HighliteTopic(place.PlaceName, items.ToList());
                topics.Add(topic);
            }

            return topics;
        }


        private IHighliteItem  CreatePhotoHighliteItem (Photo p) => DoCreateHighliteItem(p.Id, p, 0, HighliteTarget.Place, string.Empty);

        IEnumerable<Photo> PickPhotos(IEnumerable<Photo> photos) => GetRandomPhotos(photos.ToList());

        Photo PickPhoto(IEnumerable<Photo> photos)
        {
            var photo = GetRandomPhotos(photos.ToList(), 1).FirstOrDefault();
            return photo ?? photos.First();
        }

        private List<HighliteTopic> GetLocationHighlites()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var locations = _locationService.GetLocations();

            foreach (var location in locations)
            {
                var topic = CreateHighliteTopicViewModelForLocation($"{location.DisplayName}", location);

                topics.Add(topic);
            }

            return topics;
        }


        

        private Dictionary<int, int> _allPickedPhotos = new Dictionary<int, int>();

        private int GetPickedCount(Photo photo)
        {
            if (!_allPickedPhotos.ContainsKey(photo.Id))
                return 0;

            return _allPickedPhotos[photo.Id];
        }

        private void AddPickedCount(Photo photo)
        {
            int count = GetPickedCount(photo);

            _allPickedPhotos[photo.Id] = ++count;
        }

        
        private List<Photo> GetRandomPhotos(List<Photo> photos, int numPhotoWanted = 5)
        {
            var num2Pick = Math.Min(photos.Count, numPhotoWanted);

            List<Photo> picks = new List<Photo>();
            List<Photo> backupPics = new List<Photo>();

            var random = new Random( (int) ( DateTime.Now.Ticks) );

            for (var i=0; i< photos.Count; i++)
            {
                var selPhoto = photos[random.Next(0, photos.Count-1)];

                if (GetPickedCount(selPhoto) >= 1)
                {
                    if (GetPickedCount(selPhoto) == 1)
                        backupPics.Add(selPhoto);

                    continue;
                }

                AddPickedCount(selPhoto);
                picks.Add(selPhoto);

                if (picks.Count() >= num2Pick)
                    break;
            }

            return picks;
        }
        
        

        private HighliteTopic CreateHighliteTopicViewModel(string topicName, IEnumerable<TripByLocationGroup> tripByLocationGroup, int count=5)
        {
            var tripItems = tripByLocationGroup.Select(g => g.Items.First());

            var items = tripItems.Select(x => CreateHighliteItem(x));
            
            return new HighliteTopic(topicName, items.ToList());
        }

        
        private HighliteTopic CreateHighliteTopicViewModel(string topicName, IEnumerable<Trip> trips)
        {
            var items = trips.Select(x => CreateHighliteItem(x));

            return new HighliteTopic(topicName, items.ToList());
        }
        

        int GetDayNumber( Trip trip,  Photo photo)
        {
            var diff = photo.Creation - trip.FromDate;

            return (int) diff.TotalDays;
        }


        private HighliteTopic CreateHighliteTopicViewModelForTrip(string topicName, Trip trip)
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


        private HighliteTopic CreateHighliteTopicViewModelForLocation(string topicName, Location location)
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

        private IHighliteItem CreateHighliteItem(Photo photo, int count, HighliteTarget target, TitleSource titleSource)
        {
            Debug.Assert(photo.Location != null);

            var displayName = target == HighliteTarget.Location ? photo.Location.DisplayName + $" {count} photos" : photo.DisplayName;

            var item = new HighliteItem()
            {
                TargetId = (target == HighliteTarget.Location) ? photo.Location.Id : photo.Id,
                DisplayName = displayName,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl,
                Description = target.GetDescription()
            };
            return item;
        }

        private IHighliteItem CreateHighliteItem(int tripId,  string displayName, Photo photo, int count, HighliteTarget target)
        {
            Debug.Assert(photo.Location != null);

            var item = new HighliteItem()
            {
                TargetId = tripId,
                DisplayName = displayName,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl,
                Description = target.GetDescription()
            };
            return item;
        }


        private IHighliteItem CreateHighliteItem(TripItem titem)
            => DoCreateHighliteItem(titem.TripId, titem.CoverPhoto, titem.NumPictures, HighliteTarget.Trip, titem.DisplayName);

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
        
       


        private string GetItemTitle(TitleSource titleSource, Photo photo)
        {
            string notAvailable = @"N/A";
            string title;

            switch (titleSource)
            {
                case TitleSource.FileName:
                    return photo?.PhotoUrl ?? notAvailable;

                case TitleSource.LocationName:
                    return photo?.Location?.DisplayName ?? notAvailable;
                    
                case TitleSource.PhotoName:
                case TitleSource.Default:
                    return photo?.DisplayName ?? notAvailable;

                default:
                    return "Unkown-TSRC";

            }

         
        }

        public Photo GetPhoto(ITripComponent trip)
        {
            throw new NotImplementedException();
            
        }

    }

  
}
