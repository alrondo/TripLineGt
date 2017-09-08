using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TripLine.Dtos;
using TripLine.Toolbox.Extensions;

namespace TripLine.Service
{

    public class FileExtendedInfo : DtoBase<FileExtendedInfo>
    {
        public PictureExifInformation ExifInfo { get; set; }

        public string Name { get; private set; } =  string.Empty;

        public string FilePath { get; private set; } = string.Empty;
        public string RelativePath { get; private set; } = string.Empty;

        public string FileKey { get; set; } = "";

        public DateTime Creation { get; private set; }


        public string ParentFolder { get; set; } = string.Empty;


        public DateTime LastWriteDateTimeUtc { get; private set; }

        public DateTime LastWriteTime { get; private set; }

        public DateTime LastAccessTimeUtc { get; set; }

        public DateTime DetectedTime { get; private set; }

        public bool ContentChanged { get; set; }


        public FileExtendedInfo(FileInfo fileInfo, string fileKey, PictureExifInformation exifInfoInfo, DateTime detectedTime)
        {
            Name = fileInfo.Name.RemoveDiacritics();
            ParentFolder = fileInfo?.Directory?.Name;
            Creation = fileInfo.CreationTimeUtc;
            FileKey = fileKey;
            LastWriteDateTimeUtc = fileInfo.LastWriteTimeUtc;
            ContentChanged = true;
            FilePath = fileInfo.FullName;
            RelativePath = MakeRelativePath(TripLineConfig.PictureFolderPath, fileInfo.Directory.FullName);

            LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            LastWriteTime = fileInfo.LastWriteTime;
            DetectedTime = detectedTime;

            ExifInfo = exifInfoInfo;
        }


        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");


            fromPath += "\\";

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

    }
    
}

