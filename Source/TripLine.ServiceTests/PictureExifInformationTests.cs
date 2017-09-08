using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TripLine.Service;

using System.IO;
using System.Linq;
using TripLine.Dtos;
using Tripline.WebConsumer;

namespace TripLine.ServiceTests
{
    [TestClass()]
    public class PictureExifInformationTests
    {
        private readonly PictureExifInformationReader _cut;

        private readonly GoogleClient _googleClient;
        private readonly LocalFileFolders _localFileFolder;
        private readonly LocationRepo _locationRepo;
        private readonly PlaceRepo _placeRepo;


        private readonly LocationService _locationService;


        public PictureExifInformationTests()
        {
            _cut = new PictureExifInformationReader();
            _googleClient = new GoogleClient();
            _locationRepo = new LocationRepo(TripLineConfig.TestLocationRepoPath, forceNew: true);
            _placeRepo = new PlaceRepo(TripLineConfig.TestPlaceRepoPath);

            _locationService = new LocationService(_googleClient, _locationRepo, _placeRepo);


            _localFileFolder = new LocalFileFolders(_cut);
        }

        [TestMethod()]
        public void GetFolderFiles_WriteToFile()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\FolderFiles.txt", FileMode.Create, FileAccess.Write)))
            {

                _localFileFolder.Load();

                List<FileExtendedInfo> noInfoFiles = new List<FileExtendedInfo>();

                List<string> geoPositions = new List<string>();

                foreach (var finfo in _localFileFolder.GetFiles())
                {
                    streamWriter.WriteLine(finfo.Serialize());
                }
            }
        }

        [TestMethod()]
        public void GetExifInformation_ImportantTags_WriteToFile()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\PictureExifImportantTags.txt", FileMode.Create, FileAccess.Write)))
            {

                _localFileFolder.Load();

                List<FileExtendedInfo> noInfoFiles = new List<FileExtendedInfo>();

                List<GeoPosition> geoPositions = new List<GeoPosition>();


                string directory = "", lastDirectory = "";

                foreach (var file in _localFileFolder.GetFiles())
                {
                    var inf = _cut.GetExifInformation(file.FilePath);

                    if (inf.GPS_Latitude != null || inf.DateTime != null)
                    {
                        directory = Path.GetDirectoryName(file.FilePath);
                        if (directory != lastDirectory)
                        {
                            streamWriter.WriteLine($"Directory {directory}");

                            streamWriter.Write($"info found for {file.FilePath}");
                            streamWriter.WriteLine(inf.Serialize());
                        }

                        if (inf.GPS_Latitude != null)
                        {
                            geoPositions.Add(new GeoPosition(inf.GPS_Latitude.Value,inf.GPS_Longitude.Value));
                        }
                    }
                    else
                    {
                        noInfoFiles.Add(file);
                    }

                    lastDirectory = directory;
                }


                lastDirectory = "";

                streamWriter.WriteLine("---  ");
                streamWriter.WriteLine("---  No info found for:");
                foreach (var file in noInfoFiles)
                {
                    directory = Path.GetDirectoryName(file.FilePath);
                    if (directory != lastDirectory)
                        streamWriter.WriteLine($"FilePath {directory}");

                    streamWriter.WriteLine(
                        $"File date {DisplayFormater.GetDate(file.LastWriteDateTimeUtc)} path={file.FilePath} ");

                    lastDirectory = directory;
                }

                streamWriter.WriteLine("---  ");
                streamWriter.WriteLine("---  All Positions:");

                foreach (var position in geoPositions)
                {
                    var dname = "";

                    var loc = _locationService.GetLocation(position);


                    if (loc != null)
                        dname = loc.DisplayName;
                    streamWriter.WriteLine($"{position}  {dname}");
                }
            }
        }



        [TestMethod()]
        public void WriteAllInfomations_OutputDebugStringsToFile()
        {
            using (
                var streamWriter =
                    new StreamWriter(File.Open(@"c:\TripLine\PictureAllExifAllTags.txt", FileMode.Create, FileAccess.Write))
            )
            {

                _localFileFolder.Load();

                foreach (var file in _localFileFolder.GetFiles())
                {
                    _cut.WriteAllInfomations(file.FilePath, streamWriter);
                }

                _cut.WritePresences(streamWriter);


                _cut.WritePresences(streamWriter);
                
            }
        }
    }
}