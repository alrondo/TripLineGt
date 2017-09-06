using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TripLine.Dtos
{
    //public interface ITripComponent: IDateRange
    //{
    //    public int Id { get; set; }


    //}
    public class Trip : TripComponent
    {
        public string Summary { get; set; }

        public bool Active { get; set; }
        public bool Closed { get; set; }

        public string PhotoFolder { get; set; }

        public List<PhotoSession> PhotoSessions { get; set; } = new List<PhotoSession>();

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

        public IEnumerable<Destination> Destinations { get; set; }

        public void Dump(string prefix = " ")
        {
            Debug.WriteLine($">{prefix} Trip {DisplayName}  {base.FromDate} {base.Duration} ");
            Debug.WriteLine($"    " + JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }

    }
}



