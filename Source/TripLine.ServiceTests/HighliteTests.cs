using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tripline.WebConsumer;
using TripLine.Dtos;
using TripLine.Service;

namespace TripLine.ServiceTests
{
    [TestClass()]
    public class HighliteTests
    {
        
        private readonly GoogleClient _googleClient;

        private readonly LocationRepo _locationRepo;

        private readonly LocationService _locationService;
        private readonly PhotoStore _photoStore;

        private readonly PictureExifInformationReader _pictureExifReader;

        private readonly LocalFileFolders _localFileFolder;

        private readonly TripSmartBuilder _tripSmartBuilder;

        private readonly HighliteService _highliteService;

        TripStore _tripStore;

        public HighliteTests()
        {
            ServiceBootStrapper.Configure();

            _googleClient = new GoogleClient();
            _locationRepo = new LocationRepo(TripLineConfig.TestLocationRepoPath);
            _locationService = new LocationService(_googleClient, _locationRepo);

            _pictureExifReader = new PictureExifInformationReader();
            _localFileFolder = new LocalFileFolders(_pictureExifReader);
            _photoStore = new PhotoStore(new PhotoRepo(forceNew:false), _localFileFolder, _locationService);

            _tripSmartBuilder = new TripSmartBuilder(_locationService, _photoStore, new DestinationBuilder(_locationService));

            _tripStore = new TripStore(_photoStore, _locationService, _tripSmartBuilder, new TripsRepo(forceNew:false));
            _highliteService = new HighliteService(_photoStore, _tripStore, _locationService);

            var tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result = tripCreationService.DetectTripsFromNewPhotos();
            if (result.NumNewTrips > 0)
                tripCreationService.AddAll();
        }

        [TestMethod()]
        public void HighliteTests_GetHighlites_OK()
        {
            string baseDirectory = @"c:\TripLine\Highlites\";
            
            Directory.CreateDirectory(baseDirectory);
        
            var hlites = _highliteService.GetHighlites();
                
            foreach (var hlite in hlites)
            {
                string fpath = baseDirectory + "Overview - " + hlite.DisplayName + ".txt";
                using ( var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine(Path.GetFileNameWithoutExtension(fpath));
                    writer.WriteLine(hlite.Serialize(pretty: true));
                }
            }
        }

        [TestMethod()]
        public void HighliteTests_GetHighlites_Trips_OK()
        {
            string baseDirectory = @"c:\TripLine\Highlites\";

            Directory.CreateDirectory(baseDirectory);

            var hlites = _highliteService.GetHighlites(new HighliteSelectOptions( target:HighliteTarget.Trip));

            foreach (var hlite in hlites)
            {
                string fpath = baseDirectory + "Trip - " + hlite.DisplayName + ".txt";
                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine(Path.GetFileNameWithoutExtension(fpath));
                    writer.WriteLine(hlite.Serialize(pretty: true));
                }
            }
        }

        [TestMethod()]
        public void TripStoreTests_GetHighlites_Trips_OK()
        {
            string baseDirectory = @"c:\TripLine\Trips\";

            Directory.CreateDirectory(baseDirectory);

            var trips = _tripStore.GetTrips();

            foreach (var trip in trips)
            {
                string fpath = baseDirectory + $"{trip.Id} - " + trip.DisplayName + ".txt";
                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine(trip.Serialize(pretty: true));
                }
            }
        }

    }
}