using System.Collections.Generic;

namespace TripLine.Dtos
{
    public interface IHighliteItem
    {
        HighliteTarget Target { get; set; }

        int Id { get; set; }

        string DisplayName { get; set; }


        string Description { get; set; }
        string PhotoUrl { get; set; }
        string Thumbnail { get; set; }
    }

    public interface IHighliteTopic 
    {
        string DisplayName { get; set; }
        
    }
}