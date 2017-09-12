using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.Service
{

    public enum BuildStep
    {
        Idle,

        BuildingNewPhotos = 210,
        BuildingPhotoSessions,
        BuildingCandidates
    }

    public enum BuildTaskState
    {
        Idle,
        Stopped,
        Running,
        Stopping,
    }

 

    public class BuildState
    {
        public BuildTaskState TaskState { get; set; } = BuildTaskState.Idle;
        public BuildStep CurrentStep { get; set; }

        public int NumErrors { get; set; }


        public bool IsRunning => TaskState == BuildTaskState.Running;


    }

    public class TripCreationDetectResult
    {
        public int NumNewPhotos { get; set;  }
        public int NumNewTravelPhotos { get; set; }
        public int NumNewSessions { get; set; }
        public int NumNewDestinations { get; set; }

        public int NumNewTrips { get; set; }


        public int NumNotImportedPhoto { get; set; }

        public int NumNonTravelPhotos { get; set; }

        public int NumInvalidPhotos { get; set; }
        

        public int TotalRepoTravelPhotos { get; set; }

        public int TotalRepoInvalid { get; set; }

        public int TotalRepoUnconfirmed { get; set; }
        public int TotalRepoPhotos { get; set; }
  

        public BuildState State { get; set; } = new BuildState();


       
    }

    
    public class TripCreationService
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly TripStore _tripStore;
        private readonly LocationService _locationService;

        List<PhotoSession> _newSessions = new List<PhotoSession>();
        private List<TripCandidate> _newTripCandidates = new List<TripCandidate>();


        private BuildState _buildState = new BuildState();


        public TripCreationService(TripStore tripStore, PhotoStore photoStore, LocationService locationService)
        {
            _photoStore = photoStore;
            _locationService = locationService;
            _tripStore = tripStore;
        }

        public bool IsTripSession(PhotoSession session)
        {
            return true;
        }



        private TripCreationDetectResult _lastResult = new TripCreationDetectResult();


        public TripCreationDetectResult TripCreationDetectResult
        {
            get
            {
                if (_buildState.TaskState == BuildTaskState.Stopped || _buildState.TaskState == BuildTaskState.Idle)
                    return _lastResult;

                var result = new TripCreationDetectResult();

                result.NumNewPhotos = _photoStore.NumNewPhotosFiles;
                
                result.NumNewTravelPhotos = _photoStore.NewTravelPhotos.Count;

                result.NumNewSessions = _newSessions.Count;
                result.NumNewTrips = _newTripCandidates.Count;
                result.NumNewDestinations = _newTripCandidates.Sum(t => t.Destinations.Count);

                result.State = _buildState;

                _lastResult = result;

                return result;
            }
        }


        private void DetectionStopped()
        {
            _lastResult = TripCreationDetectResult;

            _lastResult.TotalRepoInvalid = _photoStore.TotalRepoInvalid;
            _lastResult.TotalRepoPhotos = _photoStore.TotalRepoPhotos;
            _lastResult.TotalRepoTravelPhotos = _photoStore.TotalRepoTravelPhotos;
            _lastResult.TotalRepoUnconfirmed = _photoStore.TotalRepoUnconfirmed;
            _lastResult.NumNotImportedPhoto = _photoStore.NumNotImportedPhoto;
            _lastResult.NumNonTravelPhotos = _photoStore.NumNonTravelPhotos;
            _lastResult.NumInvalidPhotos = _photoStore.NewInvalidPhotoCounts;

            _buildState.TaskState = BuildTaskState.Stopped;
            _buildState.CurrentStep = BuildStep.Idle;
        }

        public void Stop()
        {
            lock (_buildState)
            {
                if (_buildState.TaskState == BuildTaskState.Running &&
                    _buildState.CurrentStep == BuildStep.BuildingNewPhotos)
                {
                    _buildState.TaskState = BuildTaskState.Stopping;
                    return;
                }

                DetectionStopped();
            }
        }

        public bool PeakForNewTripPhotos()
        {
            _photoStore.CreateNewPhotos(stopOnFirstTravelPhoto:true);

            return (_photoStore.NewTravelPhotos.Any() );
        }

        public TripCreationDetectResult Build()
        {
            var result = BuildPhotos();
            result = BuildTrips();
            return result;
        }


        public TripCreationDetectResult BuildPhotos()
        {
            _buildState.TaskState = BuildTaskState.Running;

            _buildState.CurrentStep = BuildStep.BuildingNewPhotos;

            _photoStore.CreateNewPhotos();

            return UpdateLastResult();
        }

        // prerequisite  task state = running &&  CurrentStep == BuildStep.FileDetected
        public TripCreationDetectResult BuildTrips ()
        {
            if  ( _photoStore.NumNewPhotosFiles == 0)
            {
                DetectionStopped();
                return TripCreationDetectResult;
            }

            _buildState.TaskState = BuildTaskState.Running;
            _buildState.CurrentStep = BuildStep.BuildingPhotoSessions;

            _newSessions = _photoStore.CreateNewPhotoSessions(peakForNewPhotos: false);

            if (_buildState.TaskState != BuildTaskState.Running)
            {
                DetectionStopped();
                return TripCreationDetectResult;
            }

            _buildState.CurrentStep = BuildStep.BuildingCandidates;

            _newTripCandidates = _tripStore.DetectNewTrips(_newSessions).ToList();

            if (_buildState.TaskState != BuildTaskState.Stopping)
            {
                DetectionStopped();
                return TripCreationDetectResult;
            }

            return TripCreationDetectResult;
        }

        public TripCandidate GetTripCandidate (int index)
        {
            if ( index >= _newTripCandidates.Count)
                throw new ArgumentOutOfRangeException();

            return _newTripCandidates[(int) index];
        }


        public void AddAll()
        {
            while (_newTripCandidates.Count > 0)
            {
                var candidate = _newTripCandidates.First();
                _tripStore.AddNewTrip(candidate);

                _newTripCandidates.RemoveAt(0);

            }

            // reject any other
            RejectAll();

        }

        public void RejectAll()
        {
            _photoStore.RejectAllUnconfirmed();
        }



        private TripCreationDetectResult UpdateLastResult()
        {
            _lastResult.TotalRepoInvalid = _photoStore.TotalRepoInvalid;
            _lastResult.TotalRepoPhotos = _photoStore.TotalRepoPhotos;
            _lastResult.TotalRepoTravelPhotos = _photoStore.TotalRepoTravelPhotos;
            _lastResult.TotalRepoUnconfirmed = _photoStore.TotalRepoUnconfirmed;
            _lastResult.NumNotImportedPhoto = _photoStore.NumNotImportedPhoto;
            _lastResult.NumNonTravelPhotos = _photoStore.NumNonTravelPhotos;
            _lastResult.NumInvalidPhotos = _photoStore.NewInvalidPhotoCounts;
            return _lastResult;
        }


    }
}
