using TripLine.Toolbox.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TripLine.Dtos;

namespace Tripline.WebConsumer
{

    public enum GeoCodeParams
    {
        address,
        latlng,
        place_id,
        components,

        //-- optionnal
        language,
        result_type,
        location_type,
        radius,
        location

    }

   

    public enum LocationType
    {
        ROOFTOP, //restricts the results to addresses for which we have location information accurate down to street address precision.
        RANGE_INTERPOLATED, // restricts the results to those that reflect an approximation (usually on a road) interpolated between two precise points (such as intersections). An interpolated range generally indicates that rooftop geocodes are unavailable for a street address.
        GEOMETRIC_CENTER, //restricts the results to geometric centers of a location such as a polyline (for example, a street) or polygon (region).
        APPROXIMATE
    }

    public enum eAddressTypes
    {
        street_address,   //indicates a precise street address.
        route, //indicates a named route (such as "US 101").
        intersection, // indicates a major intersection, usually of two major roads.
        political, // indicates a political entity. Usually, this type indicates a polygon of some civil administration.
        country, // indicates the national political entity, and is typically the highest order type returned by the Geocoder.
        administrative_area_level_1, // indicates a first-order civil entity below the country level. Within the United States, these administrative levels are states. Not all nations exhibit these administrative levels. In most cases, administrative_area_level_1 short names will closely match ISO 3166-2 subdivisions and other widely circulated lists; however this is not guaranteed as our geocoding results are based on a variety of signals and location data.
        administrative_area_level_2, // indicates a second-order civil entity below the country level. Within the United States, these administrative levels are counties. Not all nations exhibit these administrative levels.
        administrative_area_level_3, // indicates a third-order civil entity below the country level. This type indicates a minor civil division. Not all nations exhibit these administrative levels.
        administrative_area_level_4, // indicates a fourth-order civil entity below the country level. This type indicates a minor civil division. Not all nations exhibit these administrative levels.
        administrative_area_level_5, // indicates a fifth-order civil entity below the country level. This type indicates a minor civil division. Not all nations exhibit these administrative levels.
        colloquial_area, // indicates a commonly-used alternative name for the entity.
        locality, // indicates an incorporated city or town political entity.
        ward, // indicates a specific type of Japanese locality, to facilitate distinction between multiple locality components within a Japanese address.
        sublocality, // indicates a first-order civil entity below a locality. For some locations may receive one of the additional types: sublocality_level_1 to sublocality_level_5. Each sublocality level is a civil entity. Larger numbers indicate a smaller geographic area.
        neighborhood, // indicates a named neighborhood
        premise, // indicates a named location, usually a building or collection of buildings with a common name
        subpremise, // indicates a first-order entity below a named location, usually a singular building within a collection of buildings with a common name
        postal_code,  // indicates a postal code as used to address postal mail within the country.
        natural_feature, // indicates a prominent natural feature.
        airport, // indicates an airport.
        park,  //

        // --In addition to the above, address components may include the types below
        floor, // indicates the floor of a building address.
        establishment, // typically indicates a place that has not yet been categorized.
        point_of_interest, // indicates a named point of interest.
        parking, // indicates a parking lot or parking structure.
        post_box, // indicates a specific postal box
        postal_town, // indicates a grouping of geographic areas, such as locality and sublocality, used for mailing addresses in some countries
        room, // indicates the room of a building address.
        street_number, // indicates the precise street number.
        bus_station,
        train_station,
        transit_station
    }

    enum ResultType
    {
        ROOFTOP,
        RANGE_INTERPOLATED,
        GEOMETRIC_CENTER,
        APPROXIMATE
    }

    public class GoogleDefinitions
    {
        //AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI
        //        https://maps.googleapis.com/maps/api/geocode//json?key=&address=1600+Amphitheatre+Parkway,+Mountain+View,+CA
        //https://maps.googleapis.com/maps/api/geocode//json?key=&address=6185+Boulevard+Taschereau+brossard+ca 
        //https://maps.googleapis.com/maps/api/geocode/json?key=&address=6185+Boulevard+Taschereau+brossard+ca 
        
        //        ref:  https://developers.google.com/maps/documentation/geocoding/intro#ReverseGeocoding

        string EX_RevGeoCoding = "https://maps.googleapis.com/maps/api/geocode/json?latlng=40.714224,-73.961452";
        //                        https://maps.googleapis.com/maps/api/geocode/Json/?key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI&address=Chine+2013+Chine+campagne+Grand+Barrage

    }

    public class  GLocation
    {
        public float lat { get; set; }
        public float lng { get; set; }


        //public string lat { get; set; }

        //public string lng { get; set; }


        //public float LatValue => 1;
        //public float LongValue => 1;
    }

    public class GViewport
    {
        public GLocation northeast { get; set; } = new GLocation();
        public GLocation southwest { get; set; } = new GLocation();
    }

    public class  GGeometry
    {
        public GLocation location { get; set; } = new GLocation();

        public string location_type { get; set; } = string.Empty;

        public GViewport viewport { get; set; } = new GViewport();
    }

    public class GLocationBondary
    {
        public GLocation northeast  { get; set; } = new GLocation();
        public GLocation southwest { get; set; } = new GLocation();
    }

    public class Address_Component
    {
        public string long_name { get; set; }  = string.Empty;
        public string short_name { get; set; }  = string.Empty;
        public string[] types { get; set; } = new string[0];
    }


    public class GResult
    {
        public List<Address_Component> address_components { get; set; }

        public string formatted_address { get; set; }

        public GGeometry geometry { get; set; } = new GGeometry();

        public string place_id { get; set; } = string.Empty;

        public string[] types { get; set; } = new string[0];


        public Address_Component ComponentOfType(eAddressTypes type)
            => address_components.Where(a => a.types.Any(t => t.ToString().EqualLt(type.ToString()))).FirstOrDefault();



        public float Latitude
        {
            get { return (geometry.viewport.northeast.lat + geometry.viewport.southwest.lat) / 2; }

        }

        public float Longitude
        {
            get
            {
                return (geometry.viewport.northeast.lng + geometry.viewport.southwest.lng) / 2;
            }
          
        }

        public string GetCity()
        {
            var address = ComponentOfType(eAddressTypes.locality);

            return address?.short_name.RemoveDiacritics() ?? null as string;
        }

        public string GetCountry()
        {
            var address = ComponentOfType(eAddressTypes.country);

            return address?.long_name.RemoveDiacritics() ?? null as string;
        }

        public string GetState()
        {
            var address = ComponentOfType(eAddressTypes.administrative_area_level_1); ;

            var name = (GetCity() != null && GetCountry() != null)
                ? (address?.short_name.RemoveDiacritics() ?? address?.long_name.RemoveDiacritics())
                : (address?.long_name.RemoveDiacritics() ?? address?.short_name.RemoveDiacritics());

            return name;
        }

    }

    public class GeocodingRequestResult
    {
        public List<GResult> results { get; set; } =  new List<GResult>();
        public string status { get; set; } = "NOT_FOUND";

        private GResult ResultOfType(eAddressTypes type)
            => results.FirstOrDefault(a => a.types.Any(t => t.ToString().EqualLt(type.ToString())));

        private GResult BestResult
            => ResultOfType(eAddressTypes.locality) 
            ?? ResultOfType(eAddressTypes.street_address) 
            ?? results.FirstOrDefault();

        public bool IsOk => status.EqualLt ("ok");


        public string GetCity()
        {
            var address = BestResult.GetCity();

            return address ?? null as string; 
        }

        public string GetCountry()
        {
            var address = BestResult.GetCountry();

            return address ?? null as string;
        }

        public string GetState()
        {
            var address = BestResult.GetState();

            return address ?? null as string;
        }



    }


    public class GPhoto
    {
        public int height { get; set; }
        public int width { get; set; }

        public List<string> html_attributions { get; set; }

        public string photo_reference { get; set; } = string.Empty;

    }

    public class GPlacesResult
    {
        public List<string> html_attributions { get; set; }

        public List<GPhoto> photos { get; set; }

        public GGeometry geometry { get; set; } = new GGeometry();


        public string icon { get; set; } = string.Empty;

        public string id { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public string place_id { get; set; } = string.Empty;

        public string scope { get; set; } = string.Empty;

        public string[] types { get; set; } = new string[0];

        public string vicinity { get; set; } = string.Empty;

        //public Address_Component ComponentOfType(eAddressTypes type)
        //    => address_components.Where(a => a.types.Any(t => t.ToString().EqualLt(type.ToString()))).FirstOrDefault();


        public float Latitude
        {
            get { return (geometry.viewport.northeast.lat + geometry.viewport.southwest.lat) / 2; }
        }

        public float Longitude
        {
            get
            {
                return (geometry.viewport.northeast.lng + geometry.viewport.southwest.lng) / 2;
            }
        }

    }

    public class PlacesRequestResult
    {
        public string next_page_token { get; set; }


        public List<GPlacesResult> results { get; set; } = new List<GPlacesResult>();

        public string status { get; set; } = "NOT_FOUND";

        private GPlacesResult ResultOfType(eAddressTypes type)
            => results.FirstOrDefault(a => a.types.Any(t => t.ToString().EqualLt(type.ToString())));

        private GPlacesResult BestResult
            => ResultOfType(eAddressTypes.locality)
               ?? ResultOfType(eAddressTypes.street_address)
               ?? results.FirstOrDefault();

        public bool IsOk => status.EqualLt("ok");

        public IEnumerable<GPlacesResult> GetOtherResults(GPlacesResult selectResult) => results?.Where(r => r != selectResult);
         
        public string Serialize() => JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
    }





    public class GoogleClient : RestConsumer
    {
        const string key = "AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI";
        static readonly string hostName = "https://maps.googleapis.com";
        static readonly string postfix = "maps/api";

        static readonly string nearbyplaces = $"place/nearbysearch/json?key={key}";
        static readonly string geocode = $"geocode/json?key={key}";

       

        // string ex1 = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI&location=-33.8670,151.1957&radius=500&types=food&name=cruise";
        // "nearbysearch/json&key=" + key;
        string nearby = @"nearbysearch/json?location=-33.8670,151.1957&radius=500&types=food&name=cruise&key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI";

        string ex1 = @"place/nearbysearch/json?location=-33.8670,151.1957&radius=500&types=food&name=cruise&key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI";

        string ex2 = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?&key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI&location=-33.8670,151.1957&radius=500";

        string ex3 = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI&location=-33.8670,151.1957&radius=100&type=locallity";

        private string nextPageTokenParameter = "next_page_token";



        public string types = "food;bank;";

        public GoogleClient() : base(hostName, postfix)
        {
            string key = "AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI"; //now this give you your air distance.
            
        }

        public double GetAirDistance(long lat1, long long1, long lat2, long long2)
        {
            //now this give you your air distance.
            var distance = Math.Sqrt((lat2 - lat1) ^ 2 + (long2 - long1) ^ 2);
            return distance;
        }


        public GeocodingRequestResult GetReverseGeocoding(float latitude, float longitude)
        {
            string latlngValue = latitude.ToString("F", CultureInfo.InvariantCulture) + "," +
                                 longitude.ToString("F", CultureInfo.InvariantCulture);

            string url = geocode + $"&{GeoCodeParams.latlng}={latlngValue}";

            var res = base.ExecuteGetRequest<GeocodingRequestResult>(url);

            return res;
        }

        public GeocodingRequestResult GetGeocoding(string address)
        {
            string url = geocode + $"&{GeoCodeParams.address}={address}";

            var res = ExecuteGetRequest<GeocodingRequestResult>(url);

            return res;
        }


        public GeocodingRequestResult GetGeocodingByComponent(string locality)
        {
            string url = geocode + $"&{GeoCodeParams.components}={locality}";

            var res = ExecuteGetRequest<GeocodingRequestResult>(url);

            return res;
        }


        public PlacesRequestResult GetNearbyPlaces(float latitude, float longitude, int radius=500)
        {
            string latlngValue = latitude.ToString("F", CultureInfo.InvariantCulture) + "," +
                                 longitude.ToString("F", CultureInfo.InvariantCulture);

            string url = nearbyplaces + $"&{GeoCodeParams.location}={latlngValue}&{GeoCodeParams.radius}={radius}";

            var res = base.ExecuteGetRequest<PlacesRequestResult>(url);
            return res;
        }
    }
}
