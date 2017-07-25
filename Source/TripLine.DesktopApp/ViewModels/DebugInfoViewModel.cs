using System.Reflection;
using log4net;
using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.DesktopApp.ViewModels
{
    public class DebugInfoViewModel : BaseViewModel
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TripStore _tripStore;
        private readonly PhotoStore _photoStore;
        private readonly MainViewModel _mainViewModel;
        private readonly LocationService _locationService;

        private Photo _photo;
        private Location _location;
        private Trip _trip;
        private Destination _destination;
        
        public Photo Photo => _photo;        
        public Location Location { get { return _location; } }
        public Destination Destination =>_destination;

        // todo: public PhotoSession PhotoSession { get; }

        public Trip Trip => _trip;


        public DebugInfoViewModel(TripStore tripStore,  PhotoStore photoStore, LocationService locationService) : base("Highlite")
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
            _mainViewModel = MainViewModel.Instance;
            _locationService = locationService;
        }
        
        public string TripLid 
        {
            get { return _photo.TripId  + ":" + _photo.DestId + ":" + _photo.PlaceId; }
        }

        public string PhotoLid
        {
            get { return _photo.SessionId + ":" + _photo.Id; }
        }

        public void Load( Photo photo )
        {
            _photo = photo;

            OnPropertyChanged(nameof(Photo));

            _location = _locationService.GetLocation(_photo.Location.Id);

            OnPropertyChanged(nameof(Location));

            _destination = _tripStore.GetDestination(photo.DestId);
        }
    }
}