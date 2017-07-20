using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class TripCandidate : TripComponent
    {
        public List<DestinationCandidate> Destinations { get; private set; } = new List<DestinationCandidate>();
        public List<PhotoSession> PhotoSessions { get; set; } = new List<PhotoSession>();
        public int TotalPhotos => PhotoSessions.Sum(s => s.NumPhotos);

        public void InitializeDatesFromPhotoSession()
        {
            if (PhotoSessions.Any())
            {
                FromDate = PhotoSessions.First().FromDate;
                ToDate = PhotoSessions.Last().ToDate;
            }
        }
       
        public string ElapsedFrom => $"{FromDate.ToString(DtoDefs.DateFormatter)}, {(int)Duration.TotalDays,2} days";
        public string Describe => $"{ElapsedFrom}  Trip {DisplayName,20} =>  {Destinations.Count,2} destination,  {PhotoSessions.Count,2} sessions, {TotalPhotos,3}, photos";


        
    }
}