using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tripline.WebConsumer;
using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.ServiceTests
{
    [TestClass()]
    public class TripCreationServiceTests
    {

        private readonly GoogleClient _googleClient;

        private readonly LocationRepo _locationRepo;
        private readonly PlaceRepo _placeRepo;
        private readonly LocationService _locationService;
        private readonly PictureExifInformationReader _pictureExifReader;

        private readonly LocalFileFolders _localFileFolder;

        private readonly TripSmartBuilder _tripSmartBuilder;

        private PhotoStore _photoStore;

        private TripCreationService _tripCreationService;

        TripStore _tripStore;

        public TripCreationServiceTests()
        {
            _googleClient = new GoogleClient();
            _locationRepo = new LocationRepo(TripLineConfig.TestLocationRepoPath, forceNew: true);
            _placeRepo = new PlaceRepo(TripLineConfig.TestPlaceRepoPath);

            _locationService = new LocationService(_googleClient, _locationRepo, _placeRepo);

            _pictureExifReader = new PictureExifInformationReader();
            _localFileFolder = new LocalFileFolders(_pictureExifReader);
            _photoStore = new PhotoStore(new PhotoRepo(), _localFileFolder, _locationService);

            _tripSmartBuilder = new TripSmartBuilder(_locationService, _photoStore, new DestinationBuilder(_locationService));
            _tripStore = new TripStore(_photoStore, _locationService, _tripSmartBuilder, new TripsRepo());
        }


        [TestMethod()]
        public void DetectNewFiles_Twice_NewPhotosOnFirstCall()
        {
            _photoStore = new PhotoStore(new PhotoRepo(forceNew:true), _localFileFolder, _locationService);

            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result = _tripCreationService.TripCreationDetectResult;
            Assert.IsTrue(result.NumNewPhotos == 0);

            Assert.IsTrue(result.State.TaskState == BuildTaskState.Idle);

            result = _tripCreationService.BuildPhotos();

            Assert.IsTrue(result.NumNewPhotos > 0);
            Assert.IsTrue(result.State.TaskState == BuildTaskState.Running);

            _tripCreationService.Stop();
            _tripCreationService.RejectAll();

            result = _tripCreationService.TripCreationDetectResult;

            Assert.IsTrue(result.State.TaskState == BuildTaskState.Stopped);
            Assert.IsTrue(result.NumNewPhotos > 0);

            // second time
            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result2 = _tripCreationService.BuildPhotos();

            Assert.IsTrue(result2.NumNewPhotos == 0);
        }



        [TestMethod()]
        public void DetectNewTrips_GotNewTrips_OK()
        {
            _photoStore = new PhotoStore(new PhotoRepo(forceNew: true), _localFileFolder, _locationService);

            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);


            var result = _tripCreationService.Build();

            Assert.IsTrue(result.NumNewPhotos > 0);
            Assert.IsTrue(result.NumNewTrips > 0);
            Assert.IsTrue(result.NumNewDestinations > 0);
            Assert.IsTrue(result.NumNewTravelPhotos > 0);
            
            Assert.IsTrue(result.State.TaskState == BuildTaskState.Stopped);

            _tripCreationService.Stop();
            _tripCreationService.RejectAll();         
        }



        [TestMethod()]
        public void DetectNewFiles_AfterNewTripsHaveBeenRejected_GotNone()
        {
            _photoStore = new PhotoStore(new PhotoRepo(forceNew: true), _localFileFolder, _locationService);

            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result = _tripCreationService.Build();

            Assert.IsTrue(result.NumNewPhotos > 0);
            Assert.IsTrue(result.NumNewTrips > 0);
            Assert.IsTrue(result.NumNewDestinations > 0);
            Assert.IsTrue(result.NumNewTravelPhotos > 0);

            Assert.IsTrue(result.State.TaskState == BuildTaskState.Stopped);

            _tripCreationService.Stop();
            _tripCreationService.RejectAll();

            // second time
            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result2 = _tripCreationService.BuildPhotos();

            Assert.IsTrue(result2.NumNewPhotos == 0);

        }

        [TestMethod()]
        public void DetectNewFiles_AfterNewTripsHaveAccepted_GotNone()
        {
            _photoStore = new PhotoStore(new PhotoRepo(forceNew: true), _localFileFolder, _locationService);

            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result = _tripCreationService.Build();

            Assert.IsTrue(result.NumNewPhotos > 0);
            Assert.IsTrue(result.NumNewTrips > 0);
            Assert.IsTrue(result.NumNewDestinations > 0);
            Assert.IsTrue(result.NumNewTravelPhotos > 0);

            Assert.IsTrue(result.State.TaskState == BuildTaskState.Stopped);

            _tripCreationService.Stop();
            _tripCreationService.AddAll();

            // second time
            _tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result2 = _tripCreationService.Build();

            Assert.IsTrue(result2.NumNewPhotos == 0);

        }

    }
}