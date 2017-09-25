using TripLine.Dtos;

namespace TripLine.Service
{
    public class AlbumItem
    {
        public string DefaultImg { get; set; } = "pack://application:,,,/Resources/hawai.jpg";

        public string Thumbnail => PhotoUrl;
        public string PhotoUrl { get; set; }

        public string DisplayName { get; set; } = "abc";
        public string Description { get; set; }


        public int PhotoId { get; set; }

        public int TargetId { get; set; }

        public HighliteTarget Target { get; set; }
    }
}