using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class AlbumOption
    {
        private int? TripId { get; set; }
        private int? LocationId { get; set; }
    }

    public enum AlbumType
    {
        Trip,
        Place,
        Location
    }

    public class ComponentId
    {
        int? TripId         { get; set; }
        int? DestinationId  { get; set; }
        int? PlaceId { get; set; }
    }


    public class AlbumService
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly TripStore _tripStore;
        private readonly LocationService _locationService;


        public AlbumService(PhotoStore photoStore, TripStore tripStore, LocationService locationService)
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
            _locationService = locationService;
        }

        public Album GetTripAlbum(int tripId)
        {
            var sections = BuildAlbumSectionFromTrip(tripId);

            var trip = _tripStore.GetTrip(tripId);

            var tripAlbum = new TripAlbum(trip, sections);

            return new Album(tripAlbum.DisplayName, sections);
        }


        public Album GetLocationAlbum(int locationId)
        {
            var location = _locationService.GetLocation(locationId);
            var sections = BuildFromLocation(locationId);

            return new Album(location.DisplayName, sections);
        }
        

        private List<AlbumSection> BuildAlbumSectionFromTrip(int tripId)
        {
            try
            {
                var trip = _tripStore.GetTrip(tripId);
                return new List<AlbumSection>(CreateSections(trip));

            }
            catch
            {
                return new List<AlbumSection>();
            }
        }

        private List<AlbumSection> BuildFromLocation(int locationId)
        {
            var location = _locationService.GetLocation(locationId);
            return new List<AlbumSection>(CreateSections(location));
        }
        
        private List<AlbumSection> CreateSections(Trip trip)
        {
            var list = new List<AlbumSection>();

            foreach (var dest in trip.Destinations)
                list.Add(CreateSection(dest));

            return list;
        }

        private List<AlbumSection> CreateSections(Location location)
        {
            var list = new List<AlbumSection>();
            list.Add(CreateSection(location));
            return list;
        }

        private AlbumSection CreateSection(Destination destination)
        {
            AlbumSection vmodel = AutoMapper.Mapper.Map<AlbumSection>(destination);

            var photos = _photoStore.GetPhotos().Where(p => p.DestId == destination.Id).ToList();

            vmodel.Items = photos.Select(p => CreateAlbumItem(p)).ToList();

            return vmodel;
        }

        private AlbumSection CreateSection(Location location)
        {
            AlbumSection vmodel = AutoMapper.Mapper.Map<AlbumSection>(location);

            var photos = _photoStore.GetPhotosAtLocation(location.Id).ToList();
            vmodel.Items = photos.Select(p => CreateAlbumItem(p)).ToList();
            return vmodel;
        }
        private AlbumItem CreateAlbumItem(Photo photo)
        {
            AlbumItem vmodel = AutoMapper.Mapper.Map<AlbumItem>(photo);

            vmodel.PhotoId = photo.Id;

            return vmodel;
        }
    }
}