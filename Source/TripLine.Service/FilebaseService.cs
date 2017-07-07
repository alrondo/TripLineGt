using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TripLine.Dtos;


namespace TripLine.Service
{
    public class FilebaseService
    {
        private TripsRepo _previousRepo = null;

        private TripsRepo _tripsRepo = null;

        public const string FileName = "TripLineRepo.txt";


        private string _fileName = FileName;
        
        public FilebaseService()
        {
            Load();
        }


        private void Load(FileMode mode = FileMode.OpenOrCreate)
        {         
            using (var fileStream = File.Open(_fileName, mode))
            {
                var newRepo = TripsRepo.CreateFromStream(fileStream);

                if (_tripsRepo != null &&  newRepo.LastModification != _tripsRepo?.LastModification)
                    // keep old repo
                    _previousRepo = _tripsRepo;

                _tripsRepo = newRepo;
            }
        }


        private void Save()
        {
            using (var outFile = File.Create(FileName))
            {
                var serializedRepo = _tripsRepo.SerializeToJsonBytes();
                outFile.Write(serializedRepo.ToArray(), 0, serializedRepo.Length);

            }
        }

        public IEnumerable<Trip> GetTrips ()
        {
            return _tripsRepo.Trips;
        }

        public Trip GetTripByTitle(string title)
        {
            return _tripsRepo.Trips.First(t => t.Title.IsEqualNonCase(title));
        }

        //public Trip CreateTrip ( )
        //{
            //return _tripsRepo.Trips.First(t => t.Title.IsEqualNonCase(title));
        //}

    }


    public static class StringExtensions
    {
        public static bool  IsEqualNonCase ( this String string1, String string2)
        {
            return string1.Equals(string2, StringComparison.CurrentCultureIgnoreCase);
        }
    }


    public static class BinaryReaderExtensions
    {
        public static string ReadLineUntilDelimiter(this BinaryReader reader, char delimeter)
        {
            var chars = new List<char>();
            while (reader.PeekChar() >= 0)
            {
                char c = (char)reader.ReadChar();

                if (c == delimeter)
                {
                    break;
                }

                chars.Add(c);
            }

            return new String(chars.ToArray());
        }
    }
}
