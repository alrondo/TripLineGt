using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service
{
   


    public class LocalFileFolders
    {
        private const string PhotoFilter = "*.jpg";
        private readonly PictureExifInformationReader  _exifReader;
        private readonly LocalFileRepo _localFileRepo;

        public List<FileExtendedInfo> ExtendedFileInfos { get; set; } = new List<FileExtendedInfo>();


        public List<FileExtendedInfo> GetNewFiles(DateTime fromTime)
        {
            return ExtendedFileInfos.Where(f => f.DetectedTime > fromTime).ToList();
        }

        public List<FileExtendedInfo> GetFiles()
        {
            return ExtendedFileInfos.ToList();
        }

        private string _pictureFolder = "";



        public LocalFileFolders ( PictureExifInformationReader exifReader, LocalFileRepo localFileRepo=null )
        {
            _pictureFolder = TripLineConfig.PictureFolderPath;
            _exifReader = exifReader;

            _localFileRepo = localFileRepo ?? new LocalFileRepo();
            Load(PhotoFilter);
        }

        public void Load(string filter = PhotoFilter)
        {
            //Debug.Assert(string.IsNullOrWhiteSpace(_basePath));

            _localFileRepo.Load();

            ExtendedFileInfos.Clear();

            //replace returns a string and does not modify it so this will keep the original location in tact
            //removing C:\\ so zip does not unarchive to root of c drive, this can be modified because in later cases it should be
            string[] fileEntries = Directory.GetFiles(_pictureFolder, filter, SearchOption.AllDirectories);

            IEnumerable<FileInfo> fileInfos = fileEntries.Select(f => new FileInfo(f));

            foreach (var fileinfo in fileInfos)
            {
                ExtendedFileInfos.Add(CreateFileExtendedInfo(fileinfo, DateTime.Now));
            }
        }
        

        public FileExtendedInfo CreateFileExtendedInfo(FileInfo fileInfo, DateTime detectedTime)
        {
            var key = GetFileKey(fileInfo);
            var existingInfo = _localFileRepo.GetFileInfo(key);

            var exifInfo = _exifReader.GetExifInformation(fileInfo.FullName);

           return existingInfo != null
                ? existingInfo
                : new FileExtendedInfo(fileInfo, key, exifInfo, detectedTime);

        }


        public static string GetFileKey(FileInfo fileInfo)
        {
            return (fileInfo.Name.RemoveDiacritics() + ":" + fileInfo.Length);  //                             + fileInfo?.Directory?.CreationTimeUtc.GetHashCode());
        }


        public string GetPath(string fileKey)
        {
            return ExtendedFileInfos?.FirstOrDefault(x => x.FileKey == fileKey).FilePath;
        }


        public void Save()
        {
            using (var outFile = File.Create(TripLineConfig.FileInfoRepoPath))
            {
                var serializedRepo = this.SerializeToJsonBytes();
                outFile.Write(serializedRepo.ToArray(), 0, serializedRepo.Length);

            }
        }

        public byte[] SerializeToJsonBytes(bool addNewLine = true)
        {
            var theString = JsonConvert.SerializeObject(this);
            if (addNewLine) theString += Environment.NewLine;

            return Encoding.ASCII.GetBytes(theString);
        }
    }
    
}
