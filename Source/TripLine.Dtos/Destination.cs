using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Dtos
{
    public enum eDestinationType
    {
        Continent,
        SubContinent,
        Country,
        Province,
        Region,
        NaturalPark,
        City,
        AttractionPark
    }


    public class Destination : TripComponent
    {
        public eDestinationType DestinationType { get; set; }

        public List<int> PhotoSessionIds { get; set; }

        public List<int> PhotoSessions { get; set; }


        //public string Summary { get; set; }

        //public int Popularity { get; set; }

        //public string Type { get; set; }
        //public string Icon { get; set; }

        //public string Lodging { get; set; }


    }

}
