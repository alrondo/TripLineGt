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

        private IEnumerable<FileInfo> _fileInfos;


        private void SkipOlderFiles(DateTime fromTime)
        {
            int skipCount = 0;

            foreach (var fileinfo in _fileInfos)
            {
                var extInfo = ObtainFileExtendedInfo(fileinfo, DateTime.Now);

                if (extInfo.DetectedTime >= fromTime)
                    break;  // do not skip from here

                skipCount += 1;
            }

            _fileInfos = _fileInfos.Skip(skipCount);
        }

        public IEnumerable<FileExtendedInfo> GetFiles() => GetFiles(DateTime.MinValue, int.MaxValue);

        public IEnumerable<FileExtendedInfo> GetFiles(DateTime fromTime, int maxCount= int.MaxValue)
        {
            SkipOlderFiles(fromTime);

            List<FileExtendedInfo> extendedInfos = new List<FileExtendedInfo>();

            foreach (var fileinfo in _fileInfos.Take(maxCount))
            {
                var extInfo = ObtainFileExtendedInfo(fileinfo, DateTime.Now);

                yield return extInfo;
            }

            _fileInfos.Skip(maxCount);

            if (!_fileInfos.Any())
                _localFileRepo.Save();
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

            _fileInfos = fileEntries.Select(f => new FileInfo(f));

            
        }


        public FileExtendedInfo ObtainFileExtendedInfo(FileInfo fileInfo, DateTime detectedTime)
        {
            var key = GetFileKey(fileInfo);
            var existingInfo = _localFileRepo.GetFileInfo(key);

            var exifInfo = _exifReader.GetExifInformation(fileInfo.FullName);

            if (existingInfo != null)
                return existingInfo;

            var newFInfo = new FileExtendedInfo(fileInfo, key, exifInfo, detectedTime);
            _localFileRepo.Add(newFInfo);
            return newFInfo;
        }


        public static string GetFileKey(FileInfo fileInfo)
        {
            return (fileInfo.Name.RemoveDiacritics() + ":" + fileInfo.Length);  //                             + fileInfo?.Directory?.CreationTimeUtc.GetHashCode());
        }


        public string GetPath(string fileKey)
        {
            return ExtendedFileInfos?.FirstOrDefault(x => x.FileKey == fileKey).FilePath;
        }

        
    }
    
}
