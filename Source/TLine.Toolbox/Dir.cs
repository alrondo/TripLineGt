
using System.Collections;

namespace TripLine.Toolbox
{
    using System.Collections.Generic;
    using System.IO;

    public class Dir
    {
        private string _path;

        public Dir(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Will create the directory if it does not exist. 
        /// </summary>
        /// <returns>The path of the directory</returns>
        public string CreateIfNotExist()
        {
            var path = GetPath();
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            return _path;
        }

        public string EnsureExistsAndIsEmpty()
        {
            var path = GetPath();
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive:true);
            }
            Directory.CreateDirectory(path);

            return _path;
        }

        public string GetPath()
        {
            if (Path.GetExtension(_path) == "")
                return Path.GetFullPath(_path);
            return Path.GetDirectoryName(_path);
        }

        public bool Exist()
        {
            return Directory.Exists(GetPath());

        }

        public IEnumerable<string> Files(string filter)
        {
            return Directory.GetFiles(GetPath(), filter);
        }
    }
}
