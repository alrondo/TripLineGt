using Microsoft.VisualStudio.TestTools.UnitTesting;
using TripLine.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tripline.WebConsumer;
using TripLine.Dtos;

namespace TripLine.ServiceTests
{
    [TestClass()]
    public class TripSmartBuilderTests
    {
        private readonly GoogleClient _googleClient;

        private readonly LocationRepo _locationRepo;

        private readonly LocationService _locationService;
        private readonly PhotoStore _photoStore;

        private readonly PictureExifInformationReader _pictureExifReader;

        private readonly LocalFileFolders _localFileFolder;

        private readonly TripSmartBuilder _tripSmartBuilder;

        private readonly HighliteService _highliteService;
        private readonly TripStore _tripStore;

        private bool _forceNew = false;

        public TripSmartBuilderTests()
        {
            _googleClient = new GoogleClient();
            _locationRepo = new LocationRepo(TripLineConfig.TestLocationRepoPath);
            _locationService = new LocationService(_googleClient, _locationRepo);

            _pictureExifReader = new PictureExifInformationReader();
            _localFileFolder = new LocalFileFolders(_pictureExifReader);
            _photoStore = new PhotoStore(new PhotoRepo(forceNew: _forceNew), _localFileFolder, _locationService);

            _tripSmartBuilder = new TripSmartBuilder(_locationService, _photoStore, new DestinationBuilder(_locationService));
            _tripStore = new TripStore(_photoStore, _locationService, _tripSmartBuilder, new TripsRepo(forceNew: _forceNew));
            _highliteService = new HighliteService(_photoStore, _tripStore, _locationService);


        }



        [TestMethod()]
        public void TripSmartBuilder_GetNewPhotos_OK()
        {

            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\NewPhotos.txt", FileMode.Create, FileAccess.Write)))
            {
                var photos = _photoStore.PeakForNewPhotos();

                streamWriter.WriteLine($"Got {photos.Count} photos");

                foreach (var photo in photos)
                {
                    streamWriter.WriteLine(photo.Serialize());
                }
            }
        }

        [TestMethod()]
        public void TripSmartBuilder_GetPhotoLocations_OK()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\PhotoLocations.txt", FileMode.Create,
                        FileAccess.Write)))
            {
                DisplayPhotoLocations(streamWriter, "In collection (by position)", _photoStore.GetPhotos(), true, false);
                DisplayPhotoLocations(streamWriter, "In collection (by adddress)", _photoStore.GetPhotos(), false, true);
            }

            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\NewPhotoLocations.txt", FileMode.Create, FileAccess.Write)))
            {
                var photos = _photoStore.PeakForNewPhotos();


                DisplayPhotoLocations(streamWriter,  "New travel photos (found by position)", photos, true, false);


                DisplayPhotoLocations(streamWriter, "New travel photos (found by address)", photos, false, true);


                DisplayPhotoLocations(streamWriter, "New none travel photos", _photoStore.NewImportPhotos.Where(p => !p.IsTravel).ToList(), true, true );
            }
        }

        private void DisplayPhotoLocations(StreamWriter streamWriter,  string titleline,  List<Photo> photos, bool showFoundByPosition, bool showFoundByAddress,  bool onlyFirstOfFolder=false )
        {
            int lastId = 0;

            List<string> folders =  new List<string>();

            streamWriter.WriteLine(titleline + $"{photos.Count}");
            foreach (var photo in photos.Where(l => l.IsValid).OrderBy(l2 => l2.Id))
            {
                string path = Path.GetDirectoryName(photo.PhotoUrl);

                if (!folders.Contains(path))
                {
                    folders.Add(path);
                    streamWriter.WriteLine($"In {path}");
                }

                if (onlyFirstOfFolder==false ||   photo.Location.Id != lastId)
                {
                    Location loc = photo.Location;

                    var position = photo.Position;

                    var posDisplay  = (position != null) ? $" POS {position.GetDisplay()}" : "No position";

                    var foundByLatLong = loc.SearchedPosition != null;

                    if (showFoundByPosition && !foundByLatLong)
                        continue;


                    if (showFoundByAddress && foundByLatLong)
                        continue;

                    string spos = loc.SearchedPosition != null ? loc.SearchedPosition.GetDisplay() : " None ";
                    var line = $"{loc.DisplayName} pos={posDisplay}  { loc.Id} {photo.PhotoUrl} ";
                    var line2 = $"foundByLatLong={foundByLatLong}, spos={spos} ";  


                    if (foundByLatLong &&  loc.SearchedPosition.GetDisplay() != posDisplay)
                        line += $" SPOS !! ==  {loc.SearchedPosition.GetDisplay()}";

                    streamWriter.WriteLine(line);
                    streamWriter.WriteLine(line2);

                    lastId = loc.Id;
                }

               
            }

            folders.Clear();

            streamWriter.WriteLine(" ");

            streamWriter.WriteLine(titleline + " INVALID");

            string lastLine = "";
            foreach (var photo in photos.Where(p => ! p.IsValid))
            {
                string path = Path.GetDirectoryName(photo.PhotoUrl);

                if (!folders.Contains(path))
                {
                    folders.Add(path);
                    streamWriter.WriteLine($"In {path}");
                }


                Location loc = photo.Location;

                var line = $"{photo.PhotoUrl} ";
                var line2 = "";

                var position = photo.Position;

                if (position != null)
                    line = $" POS {position.GetDisplay()}";

                if (photo.Location == null)
                    line += " NO LOC";
                else 
                {
                    var posDisplay = (position != null) ? $" POS {position.GetDisplay()}" : "No position";
                    string spos = loc.SearchedPosition != null ? loc.SearchedPosition.GetDisplay() : " None ";

                    var foundByLatLong = loc.SearchedPosition != null;

                    line += $"{loc.DisplayName} pos={posDisplay} {loc.Id}  ";
                    line2 = $"We have Found By LatLong ={foundByLatLong}, spos={spos} ";
                    
                }

                if (photo.Creation.Date.Year < 1990)
                    line += $" {photo.Creation}";

                if (line != lastLine)
                {
                    streamWriter.WriteLine( line);
                    streamWriter.WriteLine( line2);
                }
            }
        }


        [TestMethod()]
        public void TripSmartBuilder_AddNewDetectedPhotos_OK()
        {
            var expectedPhotos = _photoStore.PeakForNewPhotos();

            _photoStore.CreatePhotoFromNewFiles();

            Assert.IsTrue(_photoStore.NumNewPhotosFiles == expectedPhotos.Count);
            Assert.IsTrue(_photoStore.NewTravelPhotos.Count == expectedPhotos.Count(p => p.Excluded == false));

            var previous = _photoStore.NewTravelPhotos.First();
            foreach (var photo in _photoStore.NewTravelPhotos)
            {
                Assert.IsTrue(photo.Creation.Year > 1980, "Invalid creation date");

                Assert.IsTrue(photo.Creation >= previous.Creation, "not ordered by creation");

               //todo:  Assert.IsFalse(expectedPhotos.Any(e => e.Id != photo.Id && e.FileKey == photo.FileKey), "duplicate key");
            }
        }



        [TestMethod()]
        public void TripSmartBuilder_GetNewSessions_OK()
        {
            int previousCount = _photoStore.NewTravelPhotos.Count;

            var sessions = _photoStore.GetNewSessions();

            Assert.IsTrue(sessions.Count > 0);


            foreach (var session in sessions)
            {
                ValidateSession(session);
            }
        }


        [TestMethod()]
        public void TripSmartBuilder_GetNewSessions_ToFile()
        {

            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\NewSessions.txt", FileMode.Create, FileAccess.Write)))
            {
                var sessions = _photoStore.GetNewSessions();

             
                streamWriter.WriteLine($"Got {sessions.Count} sessions");
                foreach (var session in sessions.OrderBy(s => s.Location.Excluded))
                {
                    string isexcluded = session.Location.Excluded ? "X" : ">";
                    streamWriter.WriteLine($" {isexcluded} {session.Describe}");

                    var sessionPhotos = _photoStore.GetSessionPhoto(session.SessionId);

                    //Assert.IsTrue(session.Location.Excluded || first != null);
                    if (sessionPhotos.Any())
                    {
                        var first = sessionPhotos.First();
                        var last  = sessionPhotos.Last();
                        streamWriter.WriteLine($"  P01) {first.FileDate} {first.PhotoUrl} -> ( {first.ExifDate} )");

                        if (last != first)
                        {
                            streamWriter.WriteLine($"  P{sessionPhotos.Count,2}) {last.FileDate} {last.PhotoUrl} -> ( {last.ExifDate} )");
                        }
                    }
                    else
                    {
                        if (! session.Location.Excluded )
                            streamWriter.WriteLine($"   ERR:: Missing photo(s)!!!");
                    }
                }
            }

        }


        [TestMethod()]
        public void TripSmartBuilder_GetSessionByTrips()
        {
            int previousCount = _photoStore.NewTravelPhotos.Count;

            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\GetSessionByTrips.txt", FileMode.Create, FileAccess.Write)))
            {
                var sessions = _photoStore.GetNewSessions();

                streamWriter.WriteLine($"Got {sessions.Count}");

                //TripCandidate lastTrip = null;

                while (_tripSmartBuilder.InitializeNewTripCandidate(ref sessions))
                {
                    //if(lastTrip != null)
                    //    Assert.IsTrue(_tripSmartBuilder.CurrentTripCandidate.FromDate > lastTrip.FromDate );

                    var trip = _tripSmartBuilder.CurrentTripCandidate;

                    streamWriter.WriteLine($"Trip {trip.Describe}");

                    Assert.IsTrue(trip.PhotoSessions.Any());

                    foreach (var session in trip.PhotoSessions)
                    {
                        ValidateSession(session);

                        streamWriter.WriteLine($"   {session.Describe}");
                    }
                }

                
            }
        }

        [TestMethod()]
        public void TripSmartBuilder_BuildTripCandidates_WriteFile()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\BuildTripCandidates.txt", FileMode.Create, FileAccess.Write)))
            {
                var sessions = _photoStore.GetNewSessions();

                _tripSmartBuilder.Build(sessions);

                Assert.IsTrue(_tripSmartBuilder.BuildedTrips.Any());

                TripCandidate lastTrip = null;

                streamWriter.WriteLine($"Got {_tripSmartBuilder.BuildedTrips.Count} new trips");

                foreach (var trip in _tripSmartBuilder.BuildedTrips)
                {
                    //if(lastTrip != null)
                    //   Assert.IsTrue(trip.FromDate > lastTrip.ToDate );

                    ValidateTrip(trip);

                    streamWriter.WriteLine($"   {trip.Describe}");

                    lastTrip = trip;
                }
            }
        }


        [TestMethod()]
        public void TripSmartBuilder_BuildTripCandidatesSecondTime_ShouldBeEmpty()
        {
        }


        private void ValidateSession(PhotoSession session)
        {
            Assert.IsTrue(session.NumPhotos > 0);
       //todo:  Assert.IsTrue(session.FromDate.Year > DtoDefs.MinSupportedYear);
            Assert.IsTrue(session.ToDate >= session.FromDate);
        }


        private void ValidateTrip(TripCandidate trip)
        {
            Assert.IsTrue(trip.PhotoSessions.Any());
           
        }
    }
}