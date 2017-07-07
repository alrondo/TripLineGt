using System;
using System.ComponentModel;
using Newtonsoft.Json;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class PictureExifInformation :DtoBase<PictureExifInformation>
    {
        public PictureExifInformation()
        {
            
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? GPS_Longitude { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? GPS_Latitude { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LatitudeRef { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LongitudeRef { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateTime { get; set; }


        public GeoPosition GetPosition()
        {
            if (GPS_Longitude.HasValue  && GPS_Latitude.HasValue)
                return new GeoPosition(GPS_Latitude.Value, GPS_Longitude.Value);
            else
            {
                return new GeoPosition();
            }
        }
        
    }
}