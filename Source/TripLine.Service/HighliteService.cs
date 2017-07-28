﻿using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service
{
    public class HighliteSelectOptions
    {

        public bool Random { get; set; } = true;

        public int?  MaxNumberOfTarget { get; set; }

        public HighliteTarget? Target { get; set; } = null;

        //order by & and other stuff


        public HighliteSelectOptions(HighliteTarget target)
        {
            Target = target;
        }

        public HighliteSelectOptions()
        {
            Target = null;
        }

    }


    public enum TitleSource
    {
        Default,
        TripName,
        LocationName,
        PhotoName,
        FileName
    }

    public class HighliteService 
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly TripStore _tripStore;


        public HighliteService(PhotoStore photoStore, TripStore tripStore) 
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
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

                default:
                    throw new NotImplementedException();
            }

            return topics;
        }


        private List<HighliteTopic> GetTripHighlites()
        {
            List<HighliteTopic> topics = new List<HighliteTopic>();

            var trips = _tripStore.GetTrips(15);

            foreach (var trip in trips)
            {
                var topic = CreateHighliteTopicViewModelForTrip($"Trip to {trip.DisplayName}",  trip);

                topics.Add(topic);
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

            var topic3 = CreateHighliteTopicViewModel("Recent trips", _tripStore.GetTrips(15));

            var topic5 = DoCreateHighliteTopicViewModel("Random locations", GetRandomPhotos(photos.ToList()),  HighliteTarget.Location,  TitleSource.LocationName);

            var topics = new List<HighliteTopic>()
            {
                topic1,
                topic2,
                topic3,
                topic5
            };

            return topics;
        }


        private static List<Photo> GetRandomPhotos(List<Photo> photos, int numPhotoWanted=5)
        {
            List<Photo> randomPhotos = new List<Photo>();

            if (photos.Count == 0)
                return randomPhotos;

            int modulo = Math.Max(1, photos.Count / numPhotoWanted);

            while (modulo < 3 && numPhotoWanted > 6)
            {   // try getting less photos so we really have a kind of random
                numPhotoWanted = numPhotoWanted / 2;
                modulo = Math.Max(1, photos.Count / numPhotoWanted);
            }

            IList<int> numbers = (Enumerable.Range(0, photos.Count - 1)).ToList();
            
            numbers.Shuffle();

            int photoIndex = numbers.First();  // random start

            while (randomPhotos.Count() < numPhotoWanted)
            {
                randomPhotos.Add(photos[photoIndex]);
                photoIndex += modulo;

                if (photoIndex >= photos.Count)
                    photoIndex = 0;
            }

            return randomPhotos;
        }


        private HighliteTopic CreateHighliteTopicViewModel(string topicName, List<TripByLocationGroup> tripByLocationGroup, int count=5)
        {
            List<TripItem> tripItems = tripByLocationGroup.Select(g => g.Items.First()).ToList();
            
            var topic = new HighliteTopic()
            {
                DisplayName = topicName,
                Items = tripItems.Select(x => CreateHighliteItem(
                    x.TripId,
                    x.DisplayName,
                    x.CoverPhoto,
                    x.NumPictures,
                    HighliteTarget.Trip)).ToList()
            };

            return topic;

        }

        
        private HighliteTopic CreateHighliteTopicViewModel(string topicName, List<Trip> trips)
        {

            var topic = new HighliteTopic()
            {
                DisplayName = topicName,
                Items = trips.Select(x => CreateHighliteItem(
                    x,
                    _photoStore.GetPhotos().First(), 5,
                    HighliteTarget.Trip)).ToList()
            };

            return topic;
        }

        private HighliteTopic CreateHighliteTopicViewModel(string topicName, List<Destination> trips)
        {
            if (trips==null)
            {

            }

            var topic = new HighliteTopic()
            {
                DisplayName = topicName,

                Items = trips.Select(x => CreateHighliteItem(
                    x,
                    _photoStore.GetPhotos().First(), 5,
                    HighliteTarget.Destination)).ToList()
            };

            return topic;
        }


        int GetDayNumber( Trip trip,  Photo photo)
        {
            var diff = photo.Creation - trip.FromDate;

            return (int) diff.TotalDays;
        }




        private HighliteTopic CreateHighliteTopicViewModelForTrip(string topicName, Trip trip)
        {

            var photos = _photoStore.GetPhotosByTrip(trip.Id);

            var photosByDate = photos.GroupBy(p => GetDayNumber(trip, p));

            var highliteItems = photosByDate.Select(g => DoCreateHighliteItem(
                g.First().Id,
                g.First(),
                g.Count(),
                HighliteTarget.Trip,
                $"Day {g.Key} {g.First().Location.City ?? @"N/A"} ")).ToList();

            // on photo per day
            var topic = new HighliteTopic()
            {
                DisplayName = topicName,
                Items = highliteItems
            };
            return topic;
        }


        // doer
        private HighliteTopic DoCreateHighliteTopicViewModel(string topicName, List<Photo> photos,   HighliteTarget target, TitleSource itemTitleSource)
        {
            
            var topic = new HighliteTopic()
            {
                DisplayName = topicName,

                Items = photos.Select(p => CreateHighliteItem(
                    p,
                    _photoStore.GetPhotosAtLocation(p.Location.Id)?.Count ?? 0,
                    target, itemTitleSource)).ToList()
            };

            return topic;
        }


        private IHighliteItem CreateHighliteItem(Photo photo, int count, HighliteTarget target, TitleSource titleSource)
        {
            Debug.Assert(photo.Location != null);

            var displayName = target == HighliteTarget.Location ? photo.Location.DisplayName + $" {count} photos" : photo.DisplayName;


            var item = new HighliteItem()
            {
                Id = (target == HighliteTarget.Location) ? photo.Location.Id : photo.Id,
                DisplayName = displayName,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl
            };
            return item;
        }

        private IHighliteItem CreateHighliteItem(int tripId,  string displayName, Photo photo, int count, HighliteTarget target)
        {
            Debug.Assert(photo.Location != null);

            var item = new HighliteItem()
            {
                Id = tripId,
                DisplayName = displayName,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl
            };
            return item;
        }


        private IHighliteItem CreateHighliteItem(Trip  trip, Photo photo, int count, HighliteTarget target)
        {
            Debug.Assert(photo.Location != null);

            var item = new HighliteItem()
            {
                Id = trip.Id,
                DisplayName = trip.DisplayName,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl
            };
            return item;
        }

        private IHighliteItem CreateHighliteItem(Destination destination, Photo photo, int count, HighliteTarget target)
        {
            Debug.Assert(photo.Location != null);

            var item = new HighliteItem()
            {
                Id = destination.Id,
                DisplayName = destination.DisplayName,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl
            };
            return item;
        }

        private IHighliteItem DoCreateHighliteItem(int id, Photo photo, int count, HighliteTarget target, string title)
        {
            Debug.Assert(photo.Location != null);

            var item = new HighliteItem()
            {
                Id = id,
                DisplayName = title,
                Target = target,
                PhotoUrl = photo.PhotoUrl,
                Thumbnail = photo.PhotoUrl
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
