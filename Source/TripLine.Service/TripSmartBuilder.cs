using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;

namespace TripLine.Service
{


    public class TripSmartBuilder
    {
        private readonly LocationService _locationService;
        private readonly PhotoStore _photoStore;

        private const int FarTrîpIdleHours = 6*24;
        private const int NearTrîpIdleHours = 3 * 24;

        private int MaxIdleHours
        {
            get { return FarTrip ? FarTrîpIdleHours : NearTrîpIdleHours; }
            
        }

        public  List<TripCandidate> BuildedTrips { get; set; }  = new List<TripCandidate>();


        private DestinationBuilder DestinationBuilder { get; set; } = null;
        
        public TripCandidate  CurrentTripCandidate { get; private set; }

        public bool FarTrip { get; set; } = false;

        public List<PhotoSession> ExcludedPhotoSessions { get; set; } = new List<PhotoSession>();

        public TripSmartBuilder(LocationService locationService, PhotoStore photoStore, DestinationBuilder destinationBuilder)
        {
            _locationService = locationService;
            _photoStore = photoStore;
            DestinationBuilder = destinationBuilder;
        }


        private void TakePhotoSessionsForOneTrip(ref List<PhotoSession> sessions)
        {
            int numTaken = 0;
            foreach (var session in sessions.TakeWhile(s => IsSessionDateWithinTrip( s )) )
            {
                if (session.Location.Excluded)
                    ExcludedPhotoSessions.Add(session);
                else
                {
                    CurrentTripCandidate.PhotoSessions.Add(session);

                    CurrentTripCandidate.InitializeDatesFromPhotoSession();

                    FarTrip = _locationService.GetHomeMi(session.Location.Id) > FarTrîpIdleHours;
                }
                numTaken += 1;

            }
            CurrentTripCandidate.InitializeDatesFromPhotoSession();

            sessions = sessions.Skip(numTaken).ToList();
        }


        public bool InitializeNewTripCandidate(ref List<PhotoSession> photoSessions)
        {
            CurrentTripCandidate = new TripCandidate();

            TakePhotoSessionsForOneTrip(ref photoSessions);

            if (!CurrentTripCandidate.PhotoSessions.Any())
                return false;

            

            return true;
        }

        public void Build(List<PhotoSession> photoSessions)
        {
            BuildedTrips = new List<TripCandidate>();

            if (!photoSessions.Any())
                return;

            while (InitializeNewTripCandidate(ref photoSessions) )
            {   
                DestinationBuilder.Build(CurrentTripCandidate.PhotoSessions);

                foreach (var destCandidate in DestinationBuilder.Candidates)
                {
                    CurrentTripCandidate.Destinations.Add(destCandidate);
                }

                InitializeTripCandidate(CurrentTripCandidate);
                
                BuildedTrips.Add(CurrentTripCandidate);
            }

        }

        //private bool IsLocationExcluded(PhotoSession session) => !_locationService.IsWithinDistanceOfAndExcludedLocation(session.Location, ExclusionDistanceMi);
       
        private bool IsSessionDateWithinTrip( PhotoSession session)
        {
            if (!CurrentTripCandidate.PhotoSessions.Any() && session.FromDate.Year > 1980)
                return true;

            return session.FromDate.Subtract(CurrentTripCandidate.ToDate).TotalHours < MaxIdleHours;
        }


        void InitializeTripCandidate(TripCandidate tripCandidate)
        {
            tripCandidate.Location = DetectLocation(tripCandidate);
            tripCandidate.DisplayName = tripCandidate.Location.DisplayName;
        }


        private Location DetectLocation( TripCandidate tripCandidate)
        {
            var destinationLocations = tripCandidate.Destinations.Select(d => d.Location);

            if (destinationLocations.Count() == 1)
                // single location (trip location is same a s destination
                return destinationLocations.First(); ;


            Location tripLocation = null;

            foreach (var loc in destinationLocations)
            {
                if (tripLocation == null)
                {
                    tripLocation = new Location { Country = loc.Country, City = loc.City, State = loc.State };
                    continue;
                }

                if (loc.Country != tripLocation.Country)
                    tripLocation.Country = null;

                if (loc.State != tripLocation.State)
                    tripLocation.State = null;

                if (loc.City != tripLocation.City)
                    tripLocation.City = null;
            }

            Debug.Assert(tripLocation.Country != null );

            var resolvedLocation = _locationService.GetLocation(tripLocation.DisplayName);

            Debug.Assert(resolvedLocation != null);

            return resolvedLocation;
        }


    }
}
