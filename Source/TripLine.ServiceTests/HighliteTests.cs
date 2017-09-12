using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tripline.WebConsumer;
using TripLine.Dtos;
using TripLine.Service;
using System.Linq;
using System.Diagnostics;

namespace TripLine.ServiceTests
{
    [TestClass()]
    public class HighliteTests
    {
        
        private readonly GoogleClient _googleClient;

        private readonly LocationRepo _locationRepo;
        private readonly PlaceRepo _placeRepo;

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
            _placeRepo = new PlaceRepo(TripLineConfig.TestPlaceRepoPath);
            

            _locationService = new LocationService(_googleClient, _locationRepo, _placeRepo);

            _pictureExifReader = new PictureExifInformationReader();
            _localFileFolder = new LocalFileFolders(_pictureExifReader);
            _photoStore = new PhotoStore(new PhotoRepo(forceNew:false), _localFileFolder, _locationService);

            _tripSmartBuilder = new TripSmartBuilder(_locationService, _photoStore, new DestinationBuilder(_locationService));

            _tripStore = new TripStore(_photoStore, _locationService, _tripSmartBuilder, new TripsRepo(forceNew:false));
            _highliteService = new HighliteService(_photoStore, _tripStore, _locationService);

            var tripCreationService = new TripCreationService(_tripStore, _photoStore, _locationService);

            var result = tripCreationService.Build();
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
        public void TripStoreTests_GetTrips_OK()
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

                    var photos = _photoStore.GetPhotosByTrip(trip.Id);

                    writer.WriteLine($"Total of {photos.Count} photos for this trip.");
                    
                    foreach (var photoGroup in photos.GroupBy(p => Path.GetDirectoryName(p.PhotoUrl)))
                    {
                        writer.WriteLine($"{photoGroup.Key}  has {photoGroup.Count()} photos.");

                        if (photoGroup.Count() >= 1)
                            writer.WriteLine(photoGroup.First().Serialize(true));
                        if (photoGroup.Count() >= 2)
                        {
                            writer.WriteLine("...");
                            writer.WriteLine(photoGroup.Last().Serialize(true));
                        }

                    }
                }
            }
        }


        [TestMethod()]
        public void PhotoStore_GetPhotos_WithPlaces_OK()
        {
            string baseDirectory = @"c:\TripLine\OutPhotos\";

            Directory.CreateDirectory(baseDirectory);

            var photosByPlaces = _photoStore.GetPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId);

            Debug.Assert(photosByPlaces.Count() >= 1);

            foreach (var group in photosByPlaces)
            {
                var place = _locationService.GetPlace(group.Key);

                string fpath = baseDirectory + group.Key + $" {place.PlaceName}" + ".txt";
                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine($"{place.PlaceName}  has {group.Count()} photos.");

                    if (group.Count() >= 1)
                        writer.WriteLine(group.First().Serialize(true));
                    if (group.Count() >= 2)
                    {
                        writer.WriteLine("...");
                        writer.WriteLine(group.Last().Serialize(true));
                    }
                }
            }
        }


        [TestMethod()]
        public void PhotoStore_GetPhotos_WithGpsPosition_OutputByLocationAndDate()
        {
            string baseDirectory = @"c:\TripLine\OutPhotos\";

            Directory.CreateDirectory(baseDirectory);

            var photos = _photoStore.GetPhotos().Where(p => p.PositionFromGps);

            var photosByLocation = photos.GroupBy(p => p.Location.DisplayName);

            foreach (var locationGroup in photosByLocation)
            {
                var photosByDate = locationGroup.GroupBy(p => p.Creation.ToShortDateString());

                foreach (var dateGroup in photosByDate)
                {
                    string fpath = $"{baseDirectory}{locationGroup.Key} on {dateGroup.Key} ({dateGroup.Count()})" + ".txt";
                    using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                    {
                        if (dateGroup.Any())
                            writer.WriteLine(dateGroup.First().Serialize(true));

                        if (dateGroup.Count() >= 2)
                            writer.WriteLine(dateGroup.Skip(1).First().Serialize(true));

                        if (dateGroup.Count() > 3)
                            writer.WriteLine("...");

                        if (dateGroup.Count() >= 3)
                            writer.WriteLine(dateGroup.Last().Serialize(true));
                    }
                }
            }
        }

    }
}