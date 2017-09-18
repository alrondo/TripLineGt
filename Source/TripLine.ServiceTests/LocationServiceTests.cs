using Microsoft.VisualStudio.TestTools.UnitTesting;
using TripLine.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Tripline.WebConsumer;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service.Tests
{
    [TestClass()]
    public class LocationServiceTests
    {
        private readonly LocalFileFolders _localFileFolder;

        private readonly GoogleClient _googleClient;

        private readonly LocationRepo _locationRepo;
        private readonly PlaceRepo _placeRepo;
        private readonly LocationService _locationService;

        private readonly PictureExifInformationReader _exifReader;

        private readonly PhotoStore _photoStore;

        public LocationServiceTests()
        {
            _exifReader = new PictureExifInformationReader();

            _googleClient = new GoogleClient();
            _locationRepo = new LocationRepo(TripLineConfig.LocationRepoPath, forceNew:false);
            _placeRepo = new PlaceRepo(TripLineConfig.PlaceRepoPath);

            _locationService = new LocationService(_googleClient, _locationRepo, _placeRepo);

            _localFileFolder = new LocalFileFolders(_exifReader);
            _photoStore = new PhotoStore(new PhotoRepo(forceNew: false), _localFileFolder, _locationService);

        }

        List<string> _allGoodTests = new List<string>()
        {
            // were bads now good
        };

        List<string> _allBadTests = new List<string>()
        {
            "Chine+Chine+Croissiere+fleuve+Yangtzé+Chongquing+(embarquement)",
            "Chine+Chine+Croissiere+fleuve+Yangtzé+En+bateau",
"Chine+Chine+Croissiere+fleuve+Yangtzé+Excusion+a+Fengdu+(palais+pour+expier+le+mal)",
"Chine+Chine+Croissiere+fleuve+Yangtzé+Mini+croissiere+georges",
"Chine+Chine+Croissiere+fleuve+Yangtzé+Vol+Pekin+Chongquing",
"Chine+Chine+Pekin+Cite+interdite+(emperreur+Mings+et+Qings,+reservé+au+fonctionnaire)"
        };

        List<string> _correctedTest = new List<string>()
        {
            "Chine+Chine+Croissiere+fleuve+Yangtzé+Chongquing+(embarquement)",
            "Chine+Chine+Croissiere+fleuve+Yangtzé+En+bateau",
"Chine+Chine+Croissiere+fleuve+Yangtzé+Excusion+a+Fengdu+(palais+pour+expier+le+mal)",
"Chine+Chine+Croissiere+fleuve+Yangtzé+Mini+croissiere+georges",
"Chine+Chine+Croissiere+fleuve+Yangtzé+Vol+Pekin+Chongquing",
"Chine+Chine+Pekin+Cite+interdite+(emperreur+Mings+et+Qings,+reservé+au+fonctionnaire)"
        };

        [TestMethod()]
        public void TestLocationRepo_CheckRepo()
        {
            var locationRepo = new LocationRepo( TripLineConfig.LocationRepoPath+"2", forceNew:true);

            Assert.IsTrue(locationRepo.Content.Locations.Count == 0);

            locationRepo.Add(new Location()
            {
                City = "Montreal"
            });


            locationRepo.Save();

            Assert.IsTrue(locationRepo.Content.Locations.Count == 1);

            locationRepo.ClearContent();

            Assert.IsTrue(locationRepo.Content.Locations.Count == 0);

            // reload to see iuf they are really saved
            locationRepo.Reload();

            // Check 
            Assert.IsTrue(locationRepo.Content.Locations.Count ==1);

            Assert.IsTrue(locationRepo.Content.Locations[0].City == "Montreal");


        }

        [TestMethod()]
        public void GetLocationTest_CheckRepo()
        {
            int ok = 0;
            List<string> failed = new List<string>();


            foreach (var address in _correctedTest)
            {
                var location = _locationService.GetLocation(address);

                if (location == null || !location.Position.IsValid())
                {
                    failed.Add(address);
                }
                else
                {
                    ok += 1;
                }
            }
            
            Assert.IsTrue(failed.Count <=  3);


            int savedCount = _locationRepo.Content.Locations.Count;

            // Location not saved yet
            Assert.IsTrue(savedCount >= ok);


            _locationService.SaveLocations();

            _locationRepo.ClearContent();

            Assert.IsTrue(_locationRepo.Content.Locations.Count == 0);

            // reload to see iuf they are really saved
            _locationRepo.Reload();

            // Check 
            Assert.IsTrue(_locationRepo.Content.Locations.Count == savedCount);
        }

        [TestMethod()]
        public void Get_AllLocations_FromRepo()
        {

            string baseDirectory = @"c:\TripLine\Locations\";

            Directory.CreateDirectory(baseDirectory);

            var locations = _locationService.GetAllLocations();

            foreach (var loc in locations)
            {
                string fpath = baseDirectory + loc.DisplayName + $"({loc.Id})" + ".txt";
                using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write))) 
                {
                    loc.Serialize(writer);

                    var photos = _photoStore.GetPhotosAtLocation(loc.Id);

                    writer.WriteLine($"Total of {photos.Count} photos at this location.");


                    foreach (var photoGroup in photos.GroupBy(p =>  Path.GetDirectoryName(p.PhotoUrl)))
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
        public void Get_LocationNearbyPlaces()
        {

            string baseDirectory = @"c:\TripLine\Locations\";

            Directory.CreateDirectory(baseDirectory);

            var locations = _locationService.GetAllLocations();

            foreach (var loc in locations)
            {
                var dir = baseDirectory + loc.DisplayName + @"\";

                Directory.CreateDirectory(dir);

                var places = _locationService.GetPlaces(loc);

                foreach (var place in places)
                {
                    string fpath = baseDirectory + place.PlaceName + ".txt";

                    using (var writer = new StreamWriter(File.Open(fpath, FileMode.Create, FileAccess.Write)))
                    {
                        loc.Serialize(writer);
                    }
                }
            }
        }


        [TestMethod()]
        public void GetLocation_ByAddress_MultipleSyntax_GetSameResult()
        {
            List<string> addresses = new List<string>()
            {
                @"Montreal",
                @"Montreal, qc",
                @"Montreal, qc, ca",
                @"Montreal, qc, canada",
                @"Montreal canada",
                @" montreal québec",
                @"Montréal ",
                @"Montreal canada"
            };

            int numAlike = 0;

            var locationRef = _locationService.GetLocation(@"Montreal, qc, ca");

            foreach (var address in addresses)
            {
                var location = _locationService.GetLocation(address);

                Assert.IsTrue(location != null && location.Position.IsValid());

                if (_locationService.IsWithinDistance(location, locationRef, 20)  && location.City.ContainsEx("montreal"))
                {
                    numAlike += 1;
                }
            }

            Assert.AreEqual(numAlike, addresses.Count);
        }



        List<GeoPosition> _positions = new List<GeoPosition>()
        {
           new GeoPosition(74.036765555555547, 46.109614444444446),
            new GeoPosition(46.11054,74.01581),
new GeoPosition(46.11054,74.01581),
new GeoPosition(46.1096144444444,74.0367655555555),
new GeoPosition(46.1096144444444,74.0367655555555),
new GeoPosition(46.1087163888889,74.0366211111111),
new GeoPosition(46.1094347222222,74.0369133333333),
new GeoPosition(46.1094691666667,74.0369230555555),
new GeoPosition(46.1095225,74.0371175),
new GeoPosition(46.1047888888889,74.0569752777778),
new GeoPosition(46.1046411111111,74.0571266666667),
new GeoPosition(45.4608808333333,74.0950988888889),
new GeoPosition(45.4608808333333,74.0950988888889),
new GeoPosition(45.4609138888889,74.0950541666667),
new GeoPosition(45.4894697222222,73.39995),
new GeoPosition(45.488905,73.3951566666667),
new GeoPosition(45.4846516666667,73.4032225),
new GeoPosition(45.4846516666667,73.4032225),
new GeoPosition(45.4862427777778,73.3989927777778),
new GeoPosition(45.4884108333333,73.3971475),
new GeoPosition(45.4893888888889,73.3945241666667),
new GeoPosition(45.4879308333333,73.4001077777778),
new GeoPosition(45.46728,73.5429297222222),
new GeoPosition(45.46728,73.5429297222222),
new GeoPosition(45.46728,73.5429297222222),
new GeoPosition(45.46728,73.5429297222222),
new GeoPosition(45.46728,73.5429297222222),
new GeoPosition(45.4895197222222,73.4017697222222),
new GeoPosition(45.4895197222222,73.4017697222222),
new GeoPosition(45.4895197222222,73.4017697222222),
new GeoPosition(45.4858408333333,73.5648863888889),
new GeoPosition(47.0829616666667,70.8978588888889),
new GeoPosition(47.0829616666667,70.8978588888889),
new GeoPosition(47.0829616666667,70.8978588888889),
new GeoPosition(47.0829616666667,70.8978588888889),
new GeoPosition(47.082305,70.9199008333333),
new GeoPosition(47.0822644444444,70.9199083333333),
new GeoPosition(47.0822655555556,70.91991),
new GeoPosition(47.0822833333333,70.9199388888889),
new GeoPosition(47.0822833333333,70.9199388888889),
new GeoPosition(47.0824363888889,70.9200975),
new GeoPosition(47.0869119444444,70.9304477777778),
new GeoPosition(47.0865552777778,70.9311811111111),
new GeoPosition(47.0865180555556,70.9310719444445),
new GeoPosition(47.0865180555556,70.9310719444445),
new GeoPosition(47.1170208333333,70.8595044444444),
new GeoPosition(47.1170208333333,70.8595044444444),
new GeoPosition(45.9701908333333,74.11781),
new GeoPosition(45.4871033333333,73.5537161111111),
new GeoPosition(45.4871033333333,73.5537161111111),
new GeoPosition(45.4910419444444,73.3981783333333),
new GeoPosition(46.1154197222222,74.0435838888889),
new GeoPosition(46.1154197222222,74.0435838888889),
new GeoPosition(46.1154197222222,74.0435838888889),
new GeoPosition(46.1154511111111,74.0435913888889),
new GeoPosition(46.1154511111111,74.0435913888889),
new GeoPosition(46.1154511111111,74.0435913888889),
new GeoPosition(46.1154916666667,74.0434358333333),
new GeoPosition(45.5222519444444,73.5055258333333),
new GeoPosition(45.5210122222222,73.5033483333333),
new GeoPosition(45.5210122222222,73.5033483333333),
new GeoPosition(45.5210122222222,73.5033483333333),
new GeoPosition(45.5339463888889,73.4621727777778),
new GeoPosition(45.4568847222222,73.4437638888889),


        };


        [TestMethod()]
        public void GetLocations_With_Position()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\LocationByPosition.txt", FileMode.Create, FileAccess.Write)))
            {
                foreach (var position in _positions)
                {
                    var location = _locationService.GetLocation(position);

                    var message = (location == null)
                        ? "No location result from google: "
                        : (location.City == null && location.State == null && location.Country == null  )
                            ? $"Week {location.DisplayName}"
                            : $"Good {location.DisplayName} ";

                    streamWriter.WriteLine($"{position.GetDisplay()} ->: {message}  ");

                    if (location != null)
                        streamWriter.WriteLine( location.GetShortDisplay());

                }
            }
        }

        [TestMethod()]
        public void GetPlace_ForLocationInRepo()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\PlaceForLocationByPosition.txt", FileMode.Create, FileAccess.Write)))
            {

                var locations = _locationService.GetAllLocations();

                foreach (var location in locations)
                {
                    var place = _locationService.GetNearbyPlace(location.Position, location.Id);

                    if (place == null)
                    {
                        streamWriter.WriteLine( $"No place found at {location.GetShortDisplay()}");
                        continue;
                    }
                    
                    streamWriter.WriteLine($"{place.PlaceName} AT {location.DisplayName} : {location.Position.GetDisplay()}");
                }
            }
        }


        [TestMethod()]
        public void GetLocations_FromFilesExifGPS()
        {
            _localFileFolder.Load();

            List<FileExtendedInfo> noInfoFiles = new List<FileExtendedInfo>();

            List<GeoPosition> geoPositions = new List<GeoPosition>();


            //string directory = "", lastDirectory = "";
            List<string>  invalidExifPositionFile = new List<string>();

            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\FileGpsLocation.txt", FileMode.Create, FileAccess.Write))
            )
            {
                var totalFiles = _localFileFolder.GetFiles().Count();

                foreach (var file in _localFileFolder.GetFiles())
                {
                    Location location;

                    var inf = _exifReader.GetExifInformation(file.FilePath);

                    if (inf == null)
                    {
                        streamWriter.WriteLine($"{file.FilePath} EXIF not found");
                        continue;
                    }

                    if (!inf.GetPosition().IsValid())
                    {
                        streamWriter.WriteLine($"{file.FilePath}  NO GPS  ");
                        invalidExifPositionFile.Add(file.FilePath);
                        continue;
                    }

                    location = _locationService.GetLocation(inf.GetPosition());

                    var message = (location == null)
                        ? "No location result from google: "
                        : (location.City == null && location.State == null && location.Country == null)
                            ? $"Week {location.DisplayName}"
                            : $"Good {location.DisplayName} ";

                    streamWriter.WriteLine($"{file.FilePath} {inf.GetPosition().GetDisplay()} ->: {message}  ");

                    if (location != null)
                        streamWriter.WriteLine(location.GetShortDisplay());


                }

                streamWriter.WriteLine($"Invalif EXIF GPS {invalidExifPositionFile.Count} of {totalFiles} ");
                foreach (var file in invalidExifPositionFile)
                {
                    streamWriter.WriteLine($"file : {file}  NO GPS  ");
                    //_exifReader.WriteAllInfomations(file, streamWriter);
                }

            }

        }



        [TestMethod()]
        public void GetHomeLocation_IsMontreal()
        {
            var location = _locationService.GetHome();

            Assert.IsTrue(location.City.ContainsEx("montreal") == true);
            Assert.IsTrue(location.Country.ContainsEx("canada") == true);
            // todo: soon  Assert.IsTrue(location.State.ContainsEx("quebec") == true);
            Assert.IsTrue(location.Excluded  == true);

            Assert.IsTrue(location.Position.Latitude != 0);
            Assert.IsTrue(location.Position.Longitude != 0);

            Assert.IsTrue( ((long) location.Position.Latitude) == 45 );
            Assert.IsTrue(((long) location.Position.Longitude) == -73);
        }

        private static double MontrealLatitude = 45.5016890;
        private static double MontrealLongitude = -73.5672560;


        [TestMethod()]
        public void GetLocation_MontrealGeoPosition_Return_CityProvince()
        {
            GeoPosition position = new GeoPosition(MontrealLatitude, MontrealLongitude);
            
            var location = _locationService.GetLocation(position);
            Assert.IsTrue(location.Id > 0);

      
            Assert.IsTrue(location.City.ContainsEx("montreal") == true);
            Assert.IsTrue(location.Country.ContainsEx("canada") == true);
            // todo: soon  Assert.IsTrue(location.State.ContainsEx("quebec") == true);
        }


        [TestMethod()]
        public void GetLocation_ByAddress_ShouldReturnCompleteLocation()
        {
            List<string> addresses = new List<string>()
            {
                @"Chine 2013\Wuhan"
            };


            foreach (var address in addresses)
            {
                var location = _locationService.GetLocation(address);

                Assert.IsTrue(!string.IsNullOrEmpty(location.Country));
                Assert.IsTrue(location.Position.IsValid());
            }
        }


        [TestMethod()]
        public void GetLocations_With_Position_Twice_GetCachedLocation()
        {
            GeoPosition position = new GeoPosition(46.1047888888889, -74.0569752777778);

            var location = _locationService.GetLocation(position);

            Assert.IsTrue(location.Id > 0);

            var location2 = _locationService.GetLocation(position);
            Assert.IsTrue(location2.Id == location.Id);
        }

        [TestMethod()]
        public void GetLocation_ByAddress_Twice_ShouldReturnCachedLocation()
        {
            string address = @"Chine 2013\Wuhan";

            var location = _locationService.GetLocation(address);
            Assert.IsTrue(location.Id > 0);

            var location2 = _locationService.GetLocation(address);
            Assert.IsTrue(location2.Id ==  location.Id);

        }


        // 45.5016890

        [TestMethod()]
        public void GetHomeKm_FromHome_SouldBe0()
        {
            var km = _locationService.GetHomeMi(1);
            Assert.IsTrue(km == 0);
        }



        [TestMethod()]
        public void GetHomeKm_FromQuebecCity_SouldBe0()
        {
            var quebec = _locationService.GetLocation("Quebec city");
            var montreal = _locationService.GetLocation("Montreal city");

            var km  = _locationService.GetDistanceMi(quebec, montreal);
            Assert.IsTrue(km > 200 && km <= 300);
        }


    }
}