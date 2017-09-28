﻿using Newtonsoft.Json;
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

    public class TripItem
    {
        public string DisplayName { get; set; }
        public  Photo CoverPhoto { get; set; }

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

    public class TripByDateGroup
    {
        public int Year;

        public List<TripItem> Items { get; set; }


        public TripByDateGroup(int year, List<TripItem> items)
        {
            Year = year;
            Items = items;
        }
    }


    public class TripStore
    {
        public void Remove(int tripId)
        {
            var trip = GetTrip(tripId);

            _tripRepo.Content.Trips.Remove(trip);
            _tripRepo.Save();
        }
    
        public List<TripByLocationGroup> GetTripByCity()
        {
            List<TripByLocationGroup> tripsByCity = new List<TripByLocationGroup>();

            var res = _tripRepo.Content.Trips.GroupBy(t => t.Location.City).ToList();
            
            foreach (var grp in res)
            {
                var city = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByCity.Add(new TripByLocationGroup(city, tripItems));
            }
            return tripsByCity;
        }

        public List<TripByLocationGroup> GetTripByLocationName()
        {
            List<TripByLocationGroup> tripsByLocation = new List<TripByLocationGroup>();

            var res = _tripRepo.Content.Trips.Where( t => t.Location != null).GroupBy(t => t.Location.DisplayName).ToList();

            foreach (var grp in res)
            {
                var locationName = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByLocation.Add(new TripByLocationGroup(locationName, tripItems));
            }
            return tripsByLocation;
        }



        public List<TripByLocationGroup> GetTripByCountry()
        {
            List<TripByLocationGroup> tripsByLocation = new List<TripByLocationGroup>();

            var res = _tripRepo.Content.Trips.Where(t => t.Location != null && !string.IsNullOrEmpty(t.Location.Country) ).GroupBy(t => t.Location.Country).ToList();

            foreach (var grp in res)
            {
                var locationName = grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByLocation.Add(new TripByLocationGroup(locationName, tripItems));
            }
            return tripsByLocation;
        }


        public List<TripByDateGroup> GetTripByYear()
        {
            List<TripByDateGroup> tripsByDate = new List<TripByDateGroup>();

            var res = _tripRepo.Content.Trips.Where(t => t.Location != null).GroupBy(t => t.Date.Year).ToList();

            foreach (var grp in res)
            {
                int  year =  grp.Key;

                var tripItems = grp.Select(i => CreateTripItem(i)).ToList();
                tripsByDate.Add(new TripByDateGroup(year , tripItems));
            }
            return tripsByDate;
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
