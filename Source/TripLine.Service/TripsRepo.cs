using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class TripRepoContent
    {
        internal int NewId { get; set; } = 1;
        public int NewSessionId { get; set; } = 1;

        public List<Trip> Trips { get; set; } = new List<Trip>();
    }


    public class TripsRepo : FileRepo<TripRepoContent>
    {
        public TripsRepo():this(forceNew:false)
        {
            
        }
        public TripsRepo(bool forceNew=false) : base(TripLineConfig.TripRepoPath, forceNew)
        {
            Load();
        }


        public int GetNewId()
        {
            return Content.NewId++;
        }
    }
   
}