using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class DestinationBuilder
    {
        LocationService _locationService;


        public class MergeConfiguration
        {
            public double ScoreRatio { get; set; }
            public int DistanceMi { get; set; }

            public MergeConfiguration(double scoreRatio, int distanceMi)
            { ScoreRatio = scoreRatio; DistanceMi = distanceMi; }
        }

      
        private List<MergeConfiguration> _mergeConfigurations = new List<MergeConfiguration>()
        {
            new MergeConfiguration( 25.0, 15),
            new MergeConfiguration( 50.0, 40),
            new MergeConfiguration(70.0, 20)

        };

        private MergeConfiguration CurrentMergeConfiguration
        {
            get { return _mergeConfigurations[_mergeLevel]; }
        }


        private List<PhotoSession> Sessions { get; set; } = new List<PhotoSession>();

        private List<Location> Locations { get { return Sessions.Select(s => s.Location).ToList(); } }   // remove duplicate
        private bool HasChild(PhotoSession session)
        {
            return Locations.Any(l => session.Location.ParentId == l.Id);
        }

        private List<PhotoSession> SessionCandidates { get; set; } = new List<PhotoSession>();

        public List<DestinationCandidate> Candidates { get; set; } = new List<DestinationCandidate>();

        public List<PhotoSession> NonCandidates() => Sessions.Where(s => SessionCandidates.Any(c => c.SessionId == s.SessionId)).ToList();


        private int _mergeLevel { get; set; } = 1;

        public DestinationBuilder(LocationService locationService)
        {
            _locationService = locationService;
            _mergeLevel = _mergeConfigurations.Count / 2;
            _locationService = locationService;
        }

     
        public int Count() => Sessions.Count;



        private PhotoSession GetParentSession(PhotoSession session) => Sessions.Where(
               // todo: Check edge cases
               s => s.SessionId != session.SessionId && s.Location.Id == s.Location.ParentId).First();

        private IEnumerable<Location> GetLocations() => Sessions.Select(s => s.Location);

        public void Build(List<PhotoSession> photoSessions)
        {
            Candidates.Clear();
            Sessions = photoSessions;
            ComputeSessionsScores();

            var rankedSessions = Sessions.OrderByDescending(s => s.Score).ToList();

            while (rankedSessions.Any())
            {
                var mainSession = rankedSessions.First();
                var nearbySessions = rankedSessions
                    .Skip(1)
                    .Where(s => IsNear(mainSession.Location, s.Location)).ToList();

                var newCandidate = new DestinationCandidate(mainSession);

                if (nearbySessions.Count() > 0)
                {
                    newCandidate.Add(nearbySessions.ToList());
                }
    

                Candidates.Add(newCandidate);
                rankedSessions = rankedSessions
                                        .Skip(1)
                                        .Where(s => !nearbySessions.Contains(s)).ToList();
            }

            // Merge();
            Candidates = Candidates.OrderByDescending(l => l.TotalScore).ToList();
        }



        private bool IsNear(Location location1, Location location2)
        {
            return _locationService.IsWithinDistance(location1, location2, CurrentMergeConfiguration.DistanceMi);
        }

        private void ComputeSessionsScores()
        {
            int totalSessionPhotos = Sessions.Sum(c => c.NumPhotos);
            var totalElapsed = Sessions.Last().ToDate - Sessions.First().FromDate;
            float totalPopularity = Sessions.Sum(s => s.Location.Popularity);

            foreach (var session in Sessions)
            {
                double ratioPhotos = (session.NumPhotos) / totalSessionPhotos;
                double ratioHours = (session.Duration.TotalHours) / totalElapsed.TotalHours;

                double ratioPopularity = (session.Location.Popularity) / totalPopularity;

                var score = (ratioPhotos * 30) + (ratioHours * 30) + (ratioPopularity * 40);

                session.Score = (int)score;
            }
        }


        private bool StepMergeLevel(bool up)
        {
            if (up && CurrentMergeConfiguration == _mergeConfigurations.Last())
                return false;
            else
            if (CurrentMergeConfiguration == _mergeConfigurations.First())
                return false;

            _mergeLevel = up ? _mergeLevel + 1 : _mergeLevel - 1;

            return true;
        }



    }

}
