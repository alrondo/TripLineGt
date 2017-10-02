using TripLine.Dtos;

namespace TripLine.Service
{
    public class PlaceItem
    {
        public string DisplayName { get; set; }
        public Photo CoverPhoto { get; set; }
        public int PlaceId { get; set; }

        public int NumPictures { get; set; }

        public long Weight { get; set; }

        public PlaceItem(int id)
        {
            PlaceId = id;
        }
    }
}