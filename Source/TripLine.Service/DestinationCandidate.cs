using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class DestinationCandidate : TripComponent
    {
        public static int NewId { get; set; } = 1;
        
        public IReadOnlyCollection<PhotoSession> Sessions => PhotoSessions;

        public List<PhotoSession> PhotoSessions { get; set; } = new List<PhotoSession>();

        public int TotalPhotos => PhotoSessions.Count() > 0 ? PhotoSessions.Sum(s => s.NumPhotos) : 0;
        public int TotalScore { get; set; } = 0;
        //              _sessions.Count() > 0 ? (_sessions.Sum(s => s.Score))  : 0;
        public PhotoSession MainPhotoSession => Sessions.FirstOrDefault();


        public int? MergedTo { get; set; } = null;

        public string ElapsedFrom => $"{FromDate.ToString(DtoDefs.DateFormatter)}, {Duration.TotalDays,2} days";
        public string Describe => $"{Id }: {ElapsedFrom}  Destination {DisplayName} => {TotalPhotos,3} photos - {TotalScore,3}% ";


        public DestinationCandidate(PhotoSession mainPhotoSession)
        {
            Id = NewId++;

            PhotoSessions= new List<PhotoSession>();
            PhotoSessions.Add(mainPhotoSession);

            FromDate = new DateTime(mainPhotoSession.FromDate.Ticks);
            ToDate = new DateTime(mainPhotoSession.ToDate.Ticks);

            TotalScore = mainPhotoSession.Score;
        }

        public void Add(PhotoSession session)
        {
            PhotoSessions.Add(session);

            TotalScore += session.Score;
        }

        public void Add (List<PhotoSession> sessions)
        {
            var rankedSessions = sessions.OrderByDescending(s => s.Score).ToList();

            int total = rankedSessions.Sum(s => s.Score);

            PhotoSessions.AddRange(rankedSessions);

            TotalScore += total;
        }



    }
}
