using System.Collections.Generic;
using System.Linq;
using TripLine.Dtos;

namespace TripLine.Service
{
    public class FileInfoRepoContent
    {
        public FileInfoRepoContent() { }

        public int NewId { get; set; } = 1;

        public List<FileExtendedInfo> FileInfos { get; set; } = new List<FileExtendedInfo>();
    }

    public class LocalFileRepo : FileRepo<FileInfoRepoContent>
    {
        public LocalFileRepo() : this(TripLineConfig.FileInfoRepoPath, forceNew: false)
        {
        }

        public LocalFileRepo(string path, bool forceNew = false) : base(path, forceNew)
        {
            base.Load();
        }

        public List<FileExtendedInfo> FileInfos
        {
            get { return base.Content.FileInfos; }
        }

        public FileExtendedInfo GetFileInfo(string key)
        {
            return FileInfos.FirstOrDefault(i =>  i.FileKey == key);
        }


        public bool Contains(string key)
        {
            return FileInfos.FirstOrDefault(i => i.FileKey == key) != null;
        }

        public void Add(FileExtendedInfo info)
        {
            FileInfos.Add(info);
        }

        public int GetNewId()
        {
            return Content.NewId++;
        }
    }
}