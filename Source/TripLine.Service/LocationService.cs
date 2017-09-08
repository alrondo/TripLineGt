using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tripline.WebConsumer;
using TripLine.Dtos;

using System.Diagnostics;
using static System.FormattableString;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service
{
    

    public class LocationService
    {
        private const int ExclusionDistanceMi = 75;

        private readonly GoogleClient _googleClient;
        private readonly LocationRepo _locationRepo;
        private readonly PlaceRepo _placeRepo;

        private IEnumerable<Location> ExcludedLocation => _locationRepo.Locations.Where(l => l.Excluded == true);

        private bool Connected => true;
        
        public IEnumerable<Location> GetLocations() => _locationRepo.Locations.Where(l =>! ExcludedLocation.Contains(l));
        
        public IEnumerable<Location> GetAllLocations() => _locationRepo.Locations;

        public LocationService(GoogleClient googleClient, LocationRepo locationRepo, PlaceRepo placeRepo )
        {
            _googleClient = googleClient;
            _locationRepo = locationRepo;
            _placeRepo = placeRepo;

            InitHomeLocation();
        }

        public IEnumerable<VisitedPlace> GetPlaces(Location loc) => _placeRepo.VisitedPlaces.Where(p => p.LocationId == loc.Id);

        public VisitedPlace GetPlace(int placeId) => _placeRepo.VisitedPlaces.SingleOrDefault(p => p.Id == placeId);


        void InitHomeLocation(string homeAddress = "Montreal, Quebec, Canada, downtown")
        {
            // todo: better 

            if (_locationRepo.GetHomeLocation() != null)
                return;

            var location = GetLocation(homeAddress);
            Debug.Assert(location != null);

            location.Excluded = true;
            
            _locationRepo.Content.HomeLocationId = location.Id;
            _locationRepo.Add(location);

            _locationRepo.Save();
        }





        public Location GetHome() => _locationRepo.GetHomeLocation();


        public bool IsExcludedLocation(Location location)
        {
            return ExcludedLocation.Any(l => l.Id == location.Id || l.DisplayName == location.DisplayName);
        }

        public Location GetLocation(int id) => _locationRepo.Locations.FirstOrDefault(l => l.Id == id);
        public Location GetLocation(GeoPosition geoPosition)
        {
        
            var location = _locationRepo.GetLocation(geoPosition.Latitude, geoPosition.Longitude);

            if (location != null)
                return location;

            if (!Connected)
                return null;

            var response = _googleClient.GetReverseGeocoding(geoPosition.Latitude, geoPosition.Longitude);
            
            if (response.IsOk)
            {
                location = CreateLocation(string.Empty, geoPosition, response);
                return location;
            }
            else
            {
                Debug.WriteLine($"Could not find location by name for {geoPosition.GetDisplay()} ");
                //_unknownAddresses[address] = 1;
            }


            return null;
        }


        public VisitedPlace GetNearbyPlace(Location location)
        {
            VisitedPlace place = null;
            var response = _googleClient.GetNearbyPlaces(location.Position.Latitude, location.Position.Longitude);

            if (response.IsOk)
            {
                var result = response.results?.FirstOrDefault(r => r.types.Any(t => t == "point_of_interest"));

                if (result == null)
                    return null;

                place = new VisitedPlace()
                {
                    Id = VisitedPlace.NewPlaceId++,
                    LocationId = location.Id,
                    PlaceName = result.name,
                    Types = result.types
                };

                _placeRepo.VisitedPlaces.Add(place);
                _placeRepo.Save();

                Debug.WriteLine($"Found new place {place.PlaceName} at {location.Position.GetDisplay()} :: {place.Types} ");
                return place;
            }
            else
            {
                Debug.WriteLine($"Could not find location by name for {location.Position.GetDisplay()} ");
            }

            return null;
        }

        private List<Location> _newLocations = new List<Location>();

        public List<Location> GetNewLocations() => _newLocations;
        
        public Dictionary<string, int> _unknownAddresses = new Dictionary<string, int>();


        public string GetSearchPath ( string relativeUrl)
        {
            relativeUrl = Regex.Replace(relativeUrl, @"[\d-]", " ");

            List<string> names = relativeUrl.Split(new char[] { '\\', '+', ',', ' ' }, 
                                          StringSplitOptions.RemoveEmptyEntries).ToList();
            string addressLine = string.Join("+", names);

            addressLine = addressLine.ToLower().Replace("croisiere", "+");
            addressLine = addressLine.ToLower().Replace("campagne", "+");
            addressLine = addressLine.ToLower().Replace("bateau", "+");
            addressLine = addressLine.Replace("-", "+");
            addressLine = addressLine.ToLower().Replace("é", "e");
            addressLine = addressLine.ToLower().Replace("vol", "+");
            addressLine = addressLine.Replace("+++", "+").Replace("++", "+");
            addressLine = addressLine.Replace("+++", "+").Replace("++", "+");
            addressLine = addressLine.Replace("-", "+").Replace("++", "+");

            names = addressLine.Split(new char[] { '\\', '+', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            addressLine = string.Join("+", names);

            return addressLine;

        }


        public Location GetLocation(string relativeUrl)
        {

           var addressLine = GetSearchPath(relativeUrl);

           var location = _locationRepo.GetLocationBySearchedAddress(addressLine);

            if (location != null)
                return location;
            
            if (_unknownAddresses.Keys.Contains(addressLine))
            {
                _unknownAddresses[addressLine] += 1;
                return null;
            }

            if (Connected)
            {
                var response = _googleClient.GetGeocoding(addressLine);

                if (response.IsOk)
                {
                    location = CreateLocation(addressLine, null, response);
                    return location;
                }
            }
           
            Debug.WriteLine($"Could not find location by name for {addressLine} ");
            _unknownAddresses[addressLine] = 1;

            return null;

        }



        private Location CreateLocation(string address, GeoPosition position, 
                                        GeocodingRequestResult response)
        {
            Location location;
            location = new Location(); 

            location.Id = _locationRepo.GetNewId();

            location.SearchedAddress = address.RemoveDiacritics();
            location.SearchedPosition = position;
            location.Position = new GeoPosition(response.results.First().Latitude, response.results.First().Longitude);

            location.Country = response.GetCountry();
            location.City = response.GetCity();
            location.State = response.GetState();

            location.DisplayName = BuildDisplayName(location.City, location.State, location.Country);

            var exitingLocation = _locationRepo.GetLocation(location.DisplayName);

            if (exitingLocation != null)
                return exitingLocation;
            
            location.Excluded = IsExcludedLocation(location);

            Debug.WriteLine($"New location for {address} {location.Id} {location.DisplayName} ");

            _newLocations.Add(location);
            _locationRepo.Add(location);

            SaveLocations();
            return location;
        }

        public string BuildDisplayName(string city, string state, string country)
        {
            List<string> parts = new List<string>();
            if (!string.IsNullOrEmpty(city))
                parts.Add(city);
            if (!string.IsNullOrEmpty(state))
                parts.Add(state);
            if (!string.IsNullOrEmpty(country))
                parts.Add(country);

            var dname = string.Join(",", parts);

            return dname.RemoveDiacritics();
        }


        public bool IsWithinDistance(Location location1, IEnumerable<Location> locations, long distanceMi)
        {
            return (locations.Any(l => GetDistanceMi(location1, l) < distanceMi));
        }

        public bool IsWithinDistance(Location location1, Location location2, long distanceMi)
        {
            return (GetDistanceMi(location1, location2) < distanceMi);
        }

        public bool IsWithinDistanceOfExclusion(Location location, int distanceMi=ExclusionDistanceMi)
        {
            return (ExcludedLocation.Any(excludeLoc => IsWithinDistance(location, excludeLoc, distanceMi)));
        }


        public long GetDistanceMi(Location location1, Location location2)
        {
            if (location1 == null || location2 == null)
                return 1000;

            var sCoord = new GeoCoordinate(location1.Position.Latitude,  location1.Position.Longitude);
            var eCoord = new GeoCoordinate(location2.Position.Latitude,  location2.Position.Longitude);

            var distance = sCoord.GetDistanceTo(eCoord) / 1000;

            return (long) distance;
        }


        public long GetHomeMi(int locationId)
        {
            var location = _locationRepo.GetLocation(locationId);
            var home = _locationRepo.GetHomeLocation();

            Debug.Assert(home != null);

            return GetDistanceMi(location, home);

        }


        public void SaveLocations()
        {
            //_locationRepo.Content.Locations = _locationRepo.Content.Locations.Take(1).ToList();

            _locationRepo.Save();
        }


        public Location DetectParentLocation(IEnumerable<Location> childLocations)
        {
            if (childLocations.Count() == 1)
                // single location (trip location is same a s destination
                return childLocations.First(); ;

            Location tripLocation = null;

            foreach (var loc in childLocations)
            {
                if (tripLocation == null)
                {
                    tripLocation = new Location { Country = loc.Country, City = loc.City, State = loc.State };
                    continue;
                }

                if (loc.Country != tripLocation.Country && tripLocation.Country!= null)
                    tripLocation.Country = null;

                if (loc.State != tripLocation.State  && tripLocation.State != null)
                    tripLocation.State = null;

                if (loc.City != tripLocation.City && tripLocation.City != null)
                    tripLocation.City = null;
            }
            

            if (tripLocation?.Country == null)
            {
                // todo :  something about concatanating the country...
                //var countries = childLocations.Where(l => l.Country != null).Select(s => s.Country);
                tripLocation = childLocations.First(l => l.Country != null);
            }

            Debug.Assert( tripLocation != null && tripLocation.Country != null);

            var locationName = BuildDisplayName(tripLocation.City, tripLocation.State, tripLocation.Country);

            var resolvedLocation = GetLocation(locationName);

            Debug.Assert(resolvedLocation != null);

            return resolvedLocation;
        }

    }
}
