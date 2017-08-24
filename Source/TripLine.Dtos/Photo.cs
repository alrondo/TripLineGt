using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TripLine.Dtos
{


    public enum PhotoTag
    {
        Pinned,
        Trip,
        Destination,
        Place,
        Fav,
        Cover

    }


    public class Photo : DtoBase<Photo>
    {
        public bool IsValid => DateHelper.IsValidDate(Creation) && Location != null;


        public int Id { get; set; }
        public string DisplayName { get; set; } = "";

        public string PhotoUrl { get; set; } = "";
        public string FileKey { get; set; } = "";

        public DateTime Creation
        {
            get
            {
                return DateHelper.LowestDate(ExifDate, FileDate);
            }
        }

        public DateTime FileDate { get; private set; } = DateTime.MinValue;

        public DateTime ExifDate { get; set; } = DateTime.MinValue;
        
        public int SessionId { get; set; } = 0;
        public int TripId { get; set; } = 0;
        public int DestId { get; set; } = 0;
        public int PlaceId { get; set; } = 0;

        public GeoPosition Position { get; set; } = new GeoPosition();

        public Location    Location { get; set; }

        private string Tags { get; set; } = "";

        public bool Unclassified => TripId == 0;

        public bool Excluded { get; set; } = false;

        public bool IsTravel => ! Unclassified  && ! Excluded;

        public bool IsTravelCandidate => Unclassified && !Excluded;

        public bool HasTag(PhotoTag tag)
        {
            return (Tags.Contains(tag.ToString()));
        }

        public string FileInfoContent { get; set; }

        public string DebugInfo { get; set; }


        public void AddTag(PhotoTag tag)
        {
            if (!HasTag(tag))
            {
                Tags += "," + tag.ToString();
            }
        }

        private Photo()
        {
            
        }

        public static Photo NewPhoto(int Id, string url, string fileKey, DateTime fileDate)
        {
            var photo = new Photo()
            {
                Id = Id,
                PhotoUrl = url,
                FileKey = fileKey,
                FileDate = fileDate,
            };

            return photo;
        }

        public void Dump( string prefix=" ")
        {
            Debug.WriteLine($">{prefix} Photo {DisplayName} Creation {Creation}");
            Debug.WriteLine($"    " + JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }

    }
}
