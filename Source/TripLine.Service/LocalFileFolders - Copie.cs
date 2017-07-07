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

namespace TripLine.Service
{


    public class LocalFileFolders
    {
        private const string PhotoFilter = "*.jpg";

        
        private IEnumerable<FileExtendedInfo> _previousExtendedInfo = new List<FileExtendedInfo>();

        public List<FileExtendedInfo> ExtendedFileInfos { get; set; } = new List<FileExtendedInfo>();


        public List<FileExtendedInfo> GetNewFiles()
        {
            return ExtendedFileInfos.Where(f => f.NewFile).ToList();
        }

    
        private string _pictureFolder = "";


        public LocalFileFolders ()
        //public void Initialize(string basePath, IEnumerable<FileExtendedInfo> previousExtendedInfo = null)
        {
            _pictureFolder = TripLineConfig.PictureFolderPath;
            _previousExtendedInfo = new List<FileExtendedInfo>();

            Load(PhotoFilter);

        }

        public void Load(string filter = PhotoFilter)
        {
            //Debug.Assert(string.IsNullOrWhiteSpace(_basePath));

            if (ExtendedFileInfos.Count() > 0)
            {
                _previousExtendedInfo = ExtendedFileInfos;
            }

            ExtendedFileInfos.Clear();

            //replace returns a string and does not modify it so this will keep the original location in tact
            //removing C:\\ so zip does not unarchive to root of c drive, this can be modified because in later cases it should be
            string[] fileEntries = Directory.GetFiles(_pictureFolder, filter, SearchOption.AllDirectories);

            IEnumerable<FileInfo> fileInfos = fileEntries.Select(f => new FileInfo(f));

            foreach (var fileinfo in fileInfos)
            {
                ExtendedFileInfos.Add(CreateFileExtendedInfo(fileinfo));
            }
        }

        public FileExtendedInfo CreateFileExtendedInfo(FileInfo fileInfo)
        {
            var key = GetFileKey(fileInfo);
            var previous = _previousExtendedInfo.FirstOrDefault(f => f.FileKey == key);

            bool newFile = previous == null;
            bool hasChanged = previous?.LastWriteDateTimeUtc == fileInfo.LastWriteTimeUtc;

            if (previous != null && hasChanged == false)
            {
                previous.ContentChanged = false;
                // haven't changed since lasty time
                return previous;
            }
            else
                return new FileExtendedInfo(fileInfo, key, newFile);
        }


        public static string GetFileKey(FileInfo fileInfo)
        {
            return (fileInfo.Name + ":" + fileInfo?.Directory?.CreationTimeUtc.GetHashCode());
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
