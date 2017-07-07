using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tripline.WebConsumer;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class LocationService
    {
        private readonly GoogleClient _googleClient;

        private readonly LocationRepo _locationRepo;

        public LocationService(GoogleClient googleClient, LocationRepo locationRepo )
        {
            _googleClient = googleClient;
            _locationRepo = locationRepo;
        }

        //public Location GetLocation(float latitude, float longitude)
        //{
        //    var location = _locationRepo.GetLocation(latitude, longitude);

        //    if (location != null)
        //        return location;

        //    var response = _googleClient.GetReverseGeocoding(latitude, longitude);

        //    location = new Location();  // _goo

        //    location.Id = _locationRepo.GetNewId();

        //    _locationRepo.Content.Locations.Add(location);
        //    return location;

        //}

        private List<Location> _newLocations = new List<Location>();

        public List<Location> GetNewLocations() => _newLocations;


        public Location GetLocation(string relativeUrl)
        {
            List<string> names = relativeUrl.Split(new char[] { '\\' }).ToList();
            string address = string.Join("+", names);
            address = Regex.Replace(address, @"[\d-]", string.Empty);
            address = address.Replace(" ", "+").Replace("-", "+");
            address = address.Replace("++", "+").Replace("++", "+");

            var location = _locationRepo.GetLocationBySearchedAddress(address);

            if (location != null)
                return location;

            var response = _googleClient.GetGeocoding(address);

            if (response.IsOk)
            {
                location = new Location();  // _goo

                location.Id = _locationRepo.GetNewId();

                location.SearchedAddress = address;

                location.Country = response.GetCountry();
                location.City = response.GetCity();

                location.DisplayName = BuildDisplayName(response.BestResult.GetCity(),
                    response.BestResult.GetState(),
                    response.BestResult.GetCountry());

                _newLocations.Add(location);
                location.ProviderContent = JsonConvert.SerializeObject(response);

                return location;
            }
            return null;

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

            return string.Join(",", parts);
        }


        public bool IsInRange(Location location1, IEnumerable<Location> locations, long distanceKm)
        {
            return (locations.Any(l => GetDistanceKm(location1, l) < distanceKm));
        }

        public bool IsInRange(Location location1, Location location2, long distanceKm)
        {
            return (GetDistanceKm(location1, location2) < distanceKm);
        }


        public long GetDistanceKm(Location location1, Location location2)
        {
            if (location1.Equals(location2))
                return 0;
            else
            if (location1.Country == location2.Country && location1.State == location2.State)
                return 20;
            else
                return 100;
        }

    }
}
