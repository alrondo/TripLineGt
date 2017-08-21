
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace TripLine.Dtos
{
    
    public class Location
    {
        public bool   Excluded { get; set; }

        public int    Id { get; set; }
        public int    ParentId { get; set; }
        public float Popularity { get; set; } = (float)0.1;

        public float PopularityRatio() =>  Popularity > 0 ? Popularity / 100 : 0;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ShortName { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GeoPosition Position { get; set; } = null;
        

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SearchedAddress { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GeoPosition SearchedPosition { get; set; } = null;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (! (obj is Location) )
                return false;

            return Id == (obj as Location).Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public string GetShortDisplay() => $"{Country},{State},{City}";


        public void Dump(string prefix = " ")
        {
            Debug.WriteLine($">{prefix} Location {DisplayName}");
            Debug.WriteLine($"    " + JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }
    }

}

//[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
//public GeoPosition NE_Position { get; set; } = null;

//[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
//public GeoPosition SW_Position { get; set; } = null;