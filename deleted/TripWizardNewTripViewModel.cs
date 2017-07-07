using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using log4net;
using TripLine.DesktopApp.Models;
using TripLine.Service;

namespace TripLine.DesktopApp.ViewModels
{
    public class TripWizardNewTripViewModel : BaseViewModel, IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly LocationService _locationService;

        private readonly TripCreationService _tripCreationService;
        

        public TripWizardNewTripViewModel(TripCreationService tripCreationService, PhotoStore photoStore,
            LocationService locationService) : base("TripWizard")
        {
            _tripCreationService = tripCreationService;
            _photoStore = photoStore;
            _locationService = locationService;
        }

        public void Dispose()     {        }

        ObservableCollection<DestinationCandidateModel> _destinationCandidates = new ObservableCollection<DestinationCandidateModel>();

        TripCandidateModel _tripCandidate;

        public TripCandidateModel TripCandidate
        {
            get
            {
                return _tripCandidate;
            }
            set
            {
                if (value == _tripCandidate)
                    return;

                _tripCandidate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DestinationCandidateModel> DestinationCandidates
        {
            get
            {
                return _destinationCandidates;
            }
            set
            {
                if (value == _destinationCandidates)
                    return;

                _destinationCandidates = value;
                OnPropertyChanged();
            }
        }


        public void Load (int tripIndex = 0)
        {
            var trip = _tripCreationService.GetTripCandidate(tripIndex);
            var candidates = trip.Destinations.Select(
                d => DestinationCandidateModel.CreateDestinationCandidateModel(d)).ToList();
            foreach (var cand in candidates)
                DestinationCandidates.Add(cand);
              
            TripCandidate = TripCandidateModel.CreateTripCandidateModel(trip);
        } 
    }
}