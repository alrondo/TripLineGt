using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TLine.Toolbox.Extensions;

using TLine.WPF.MVVM;

using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.DesktopApp.ViewModels
{
   
    public enum DestinationType
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


    public class CandidateModel
    {
        string   Name { get; set; }
        int      NumPhotos { get; set; }

        string   Where { get; set; }
        string   When { get; set; }

        DateTime StartDate { get; set; }
        int      NumDays { get; set; }

        // int  ImportanceVsSibbling
        // int  ImportanceInTrip
        // int  ImportanceInLibrary
        // bool SelectedForCreation
    }


    public class DestinationCandidateModel: CandidateModel
    {
        int             ParentTripId    { get; set; }
        int             ParentDestinationId { get; set; }
        DestinationType DestinationType { get; set; }
        int             DestinatonId    { get; set; }
        List<int>       ChildDestionIds { get; set; }
        List<int>       SessionIds      { get; set; }
    }

    public class TripCandidateModel : CandidateModel
    {
        int             TripId          { get; set; }
    }

    public class TripWizardViewModel : BaseViewModel, IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly PhotoStore _photoStore;
        //private readonly LocationService _locationService;

        //private readonly TripCreationService _tripCreationService;

        public TripWizardViewModel (TripCreationService tripCreationService) : base("TripWizard")
        //)
        //, PhotoStore photoStore,    LocationService locationService)         {
        //_tripCreationService = tripCreationService;
        ////_photoStore = photoStore;
        //_locationService = locationService;
        { 
            _newPhotoCount = 2;  // _tripCreationService.DetectNewPhotos(out _newPhotoSessionCount);
        }

        private int _newPhotoSessionCount = 0;

        public int NewPhotoSessionCount
        {
            get
            {
                return _newPhotoSessionCount;
            }
            set
            {
                if(value != _newPhotoSessionCount)
                {
                    _newPhotoSessionCount = value;
                    OnPropertyChanged(nameof(NewPhotoSessionCount));
                }
            }
        }

        private int _newPhotoCount = 0;

        public int NewPhotoCount
        {
            get
            {
                return _newPhotoCount;
            }
            set
            {
                if (value != _newPhotoCount)
                {
                    _newPhotoCount = value;
                    OnPropertyChanged(nameof(NewPhotoCount));
                }
            }
        }


        private int _newDestinationCandidateCount = 0;


        public int NewDestinationCandidateCount
        {
            get
            {
                return _newDestinationCandidateCount;
            }
            set
            {
                if (value != _newDestinationCandidateCount)
                {
                    _newDestinationCandidateCount = value;
                    OnPropertyChanged(nameof(NewDestinationCandidateCount));
                }
            }
        }




        private int _newTripCandidateCount = 0;


        public int NewTripCandidateCount
        {
            get
            {
                return _newTripCandidateCount;
            }
            set
            {
                if (value != _newDestinationCandidateCount)
                {
                    _newTripCandidateCount = value;
                    OnPropertyChanged(nameof(NewTripCandidateCount));
                }
            }
        }

        
        private List<TripCandidateModel> _tripCandidates = new List<TripCandidateModel>();

        private TripCandidateModel _selectedTripCandidate = null;
        
        public TripCandidateModel SelectedTripCandidate
        {
            get { return _selectedTripCandidate; }
            set
            {
                _selectedTripCandidate = value;
                OnPropertyChanged(nameof(SelectedTripCandidate));
            }
        }
        
        private List<TripCandidateModel> TripCandidates
        {
            get
            {
                return _tripCandidates;
            }
            set
            {
                if (value != _tripCandidates)
                {
                    _tripCandidates = value;
                    OnPropertyChanged(nameof(TripCandidates));
                }
            }
        }

        public void Dispose()
        {

        }

    }
}
