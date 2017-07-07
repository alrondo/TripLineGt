using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class FileRepo<T>
    {
        public string FilePath { get; set; } 

        public T Content { get; private set; }

        private bool _loaded = false;

        public FileRepo(string filePath) : this (filePath, forceNew:false)
        {

        }

        public FileRepo(string filePath, bool forceNew )
        {
            if (forceNew && File.Exists(filePath))
                File.Delete(filePath);

            var dir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FilePath = filePath;
        }


        public void Load()
        {
            Debug.Assert(_loaded == false);
            DoLoad();
        }

        public void Reload()
        {
            DoLoad();
        }

        public void ClearContent()
        {
            Content = Activator.CreateInstance<T>();

        }


        private void DoLoad()
        {
            var newContent = Activator.CreateInstance<T>();
            bool exist = File.Exists(FilePath);

            if (exist)
            {
                using (var fstream = File.OpenText(FilePath))
                {
                    var line = fstream.ReadToEnd();

                    if (line.Trim().Any())
                    {
                        try
                        {
                            var content = (T)JsonConvert.DeserializeObject(line, typeof(T));
                            exist = false;
                            newContent = content;

                            _loaded = true;
                        }
                        catch (Exception)
                        {
                            // will use default
                        }
                    }
                  
                }
            }
          

            Content = newContent;

        }

        


        public void Save()
        {
            using (var outFile = File.Create(FilePath))
            {
                var serializedRepo = this.SerializeToJsonBytes();
                outFile.Write(serializedRepo.ToArray(), 0, serializedRepo.Length);

            }
        }

        public byte[] SerializeToJsonBytes(bool addNewLine = true)
        {
            var theString = JsonConvert.SerializeObject(this.Content);
            if (addNewLine) theString += Environment.NewLine;

            return Encoding.ASCII.GetBytes(theString);
        }
    }

}
