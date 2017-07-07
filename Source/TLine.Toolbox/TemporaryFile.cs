using System;
using System.IO;

namespace TripLine.Toolbox
{
    public class TemporaryFile : IDisposable
    {
        private bool _disposed = false;

        private readonly string _filePath;

        public string FilePath { get { return _filePath; } }

        public TemporaryFile(string tempFileName)
        {
            _filePath = Path.Combine(Path.GetTempPath(), tempFileName);
        }

        public TemporaryFile()
            :this(Path.GetTempFileName())
        {
        }


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (File.Exists(_filePath))
                {
                    var fileInfo = new FileInfo(_filePath);
                    if (fileInfo.IsReadOnly)
                    {
                        fileInfo.IsReadOnly = false;
                    }

                    fileInfo.Delete();
                }
            }
        }
    }
}
