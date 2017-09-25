using System.Collections.Generic;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class Album: DtoBase<Album>
    {
        public string DisplayName { get; set; }

        public List<AlbumSection> Sections { get; set; }
        
        public Album(string displayName, List<AlbumSection> sections)
        {
            DisplayName = displayName;
            Sections = sections;
        }

    }
}