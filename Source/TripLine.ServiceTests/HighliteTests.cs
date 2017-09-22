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
            _locationRepo = new LocationRepo(TripLineConfig.LocationRepoPath);
            _placeRepo = new PlaceRepo(TripLineConfig.PlaceRepoPath);
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

        string _testsOut = @"c:\TripLine\TestsOut";

        [TestMethod()]
        public void HighliteTests_GetHighlites_OK()
        {
            string baseDirectory = $@"{_testsOut}\Hlite Overview\";
            
            Directory.CreateDirectory(baseDirectory);
        
            var hlites = _highliteService.GetHighlites();
                
            foreach (var hlite in hlites)
            {
                string fpath = baseDirectory + ValidFilename(hlite.DisplayName) + ".txt";
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
            string baseDirectory = $@"{_testsOut}\Hlite Trips\";

            Directory.CreateDirectory(baseDirectory);

            var hlites = _highliteService.GetHighlites(new HighliteSelectOptions( target:HighliteTarget.Trip));

            foreach (var hlite in hlites)
            {
                string fpath = baseDirectory + ValidFilename(hlite.DisplayName) + ".txt";
                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine(Path.GetFileNameWithoutExtension(fpath));
                    writer.WriteLine(hlite.Serialize(pretty: true));
                }
            }
        }


        [TestMethod()]
        public void HighliteTests_GetHighlites_Places_OK()
        {
            string baseDirectory = $@"{_testsOut}\Hlite Places\";

            Directory.CreateDirectory(baseDirectory);

            var hlites = _highliteService.GetHighlites(new HighliteSelectOptions(target: HighliteTarget.Place));

            foreach (var hlite in hlites)
            {
                string fpath = baseDirectory + ValidFilename(hlite.DisplayName) + ".txt";
                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine(Path.GetFileNameWithoutExtension(fpath));
                    writer.WriteLine(hlite.Serialize(pretty: true));
                }
            }
        }

        string ValidFilename(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }


        [TestMethod()]
        public void TripStoreTests_GetTrips_OK()
        {
            string baseDirectory = $@"{_testsOut}\Trips\";

            Directory.CreateDirectory(baseDirectory);

            var trips = _tripStore.GetTrips();

            foreach (var trip in trips)
            {
                string fpath = baseDirectory + $"{trip.Id}-" 
                    + ValidFilename(trip.GetDisplayName(withDate:true)) + ".txt";

                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                {
                    writer.WriteLine(trip.Serialize(pretty: true));

                    var photos = _photoStore.GetPhotosByTrip(trip.Id);

                    writer.WriteLine($"Total of {photos.Count} photos for this trip.");

                    foreach (var photo in photos)
                    {
                        if (photo == photos.First())
                            OutputPhoto(fpath, photo, 1);
                        else
                        if (photo == photos.LastOrDefault())
                            OutputPhoto(fpath, photo, photos.Count());

                    }
                }
            }
        }

        void OutputPhoto(string fpath, Photo photo, int idx)
        {
            fpath = fpath.Replace(".txt", "");
            fpath += $"_P{idx}-{photo.Id}" + ".txt";

            using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine(photo.Serialize(true));
            }
        }



        [TestMethod()]
        public void PhotoStore_GetPhotos_WithPlaces_OK()
        {
            string baseDirectory = $@"{_testsOut}\PhotosWithPlaces\";

            Directory.CreateDirectory(baseDirectory);

            var photosByPlaces = _photoStore.GetTravelPhotos().Where(p => p.PlaceId != 0).GroupBy(p => p.PlaceId);

            Debug.Assert(photosByPlaces.Count() >= 1);

            foreach (var group in photosByPlaces)
            {
                var place = _locationService.GetPlace(group.Key);

                string fpath = baseDirectory + group.Key + $" {ValidFilename(place.PlaceName)}" + ".txt";
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
            string baseDirectory = $@"{_testsOut}\PhotosByDate\";

            Directory.CreateDirectory(baseDirectory);

            var photos = _photoStore.GetTravelPhotos();

            var photosByLocation = photos.GroupBy(p => p.Location.DisplayName);

            foreach (var locationGroup in photosByLocation)
            {
                var photosByDate = locationGroup.GroupBy(p => p.Creation.ToShortDateString());

                foreach (var dateGroup in photosByDate)
                {
                    var name = ValidFilename($"{locationGroup.Key} on {dateGroup.Key} ({dateGroup.Count()})");
                    string fpath = $"{baseDirectory}{name}" + ".txt";
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