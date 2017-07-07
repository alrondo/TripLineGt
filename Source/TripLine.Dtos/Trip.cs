using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TripLine.Dtos
{
    //public interface ITripComponent: IDateRange
    //{
    //    public int Id { get; set; }


    //}
    public class Trip: TripComponent
    {
        public string Summary { get; set; }

        public bool Active { get; set; }
        public bool Closed { get; set; }

        public string PhotoFolder { get; set; }

        //public IDateRange Date { get; set; }

        public Trip()
        {
        }

        public Trip(int tripId, DateTime startDate)
        {
            Id = tripId;
            DisplayName += " " + tripId;

            //Date.FromDate = startDate;

        }
        //public List<Visit> Visit { get; set; } 

        public List<Destination> Destinations { get; set; }
    }


    

}




