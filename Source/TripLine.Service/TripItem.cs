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
}