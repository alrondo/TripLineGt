using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExifLib;

namespace TripLine.Service
{
    public class PictureExifInformationReader
    {
        public List<string> InfoMessages = new List<string>();

        public List<ExifTags> TagsFound = new List<ExifTags>();
        public List<ExifTags> TagsNotFound = new List<ExifTags>();

        private ExifTags[] InterstingsTagDouble = new ExifTags[]
        {
            ExifTags.DateTime,
            ExifTags.DateTimeDigitized,

            ExifTags.GPSAltitude,
            ExifTags.GPSAreaInformation,
            ExifTags.GPSDateStamp,
            ExifTags.GPSDestDistance,
            ExifTags.GPSLongitude,
            ExifTags.GPSLatitude,
            ExifTags.GPSDOP,
            ExifTags.GPSMapDatum,
            ExifTags.GPSTimestamp,

            ExifTags.GPSDestLatitude,
            ExifTags.GPSDestLatitudeRef
        };




        public PictureExifInformation GetExifInformation(string filepath)
        {
            var inf = new PictureExifInformation();

            try
            {
                using (ExifReader reader = new ExifReader(filepath))
                {
                    // Extract the tag data using the ExifTags enumeration
                    InfoMessages.Clear();

                    InfoMessages.Add($"Info from picture {filepath}");
                    bool notFound;

                    inf.DateTime = Get<DateTime>(reader, ExifTags.DateTime, ExifTags.DateTimeDigitized, out notFound);
                    if (notFound)
                        inf.DateTime = null;

                    inf.GPS_Latitude = Get24BitPosition(reader, ExifTags.GPSLatitude, ExifTags.GPSDestLatitude,
                        out notFound);
                    if (notFound)
                        inf.GPS_Latitude = null;

                    inf.GPS_Longitude = Get24BitPosition(reader, ExifTags.GPSLongitude, ExifTags.GPSDestLongitude,
                        out notFound);
                    if (notFound)
                        inf.GPS_Longitude = null;

                    inf.LatitudeRef = Get<string>(reader, ExifTags.GPSLatitudeRef, out notFound);
                    if (notFound)
                        inf.LatitudeRef = "N";

                    inf.LongitudeRef = Get<string>(reader, ExifTags.GPSLongitudeRef, out notFound);
                    if (notFound)
                        inf.LongitudeRef = "W";


                    if (inf.GPS_Latitude != null)
                        inf.GPS_Latitude = FixLatitude(inf.LatitudeRef, inf.GPS_Latitude.Value);

                    if (inf.GPS_Longitude != null)
                        inf.GPS_Longitude = FixLongitude(inf.LongitudeRef, inf.GPS_Longitude.Value);
                }
            }
            catch (Exception ex)
            {
                InfoMessages.Add($"Exception while GetExifInformation {filepath} {ex.Message}");
            }


            return inf;
        }


        double FixLatitude(string refPosition, double value)
        {
            bool expectPositive = refPosition.ToLower().StartsWith("n");

            return ForceProperSign(expectPositive, value);
        }


        double FixLongitude(string refPosition, double value)
        {
            bool expectPositive = refPosition.ToLower().StartsWith("e");

            return ForceProperSign(expectPositive, value);
        }

        double ForceProperSign(bool positive, double value)
        {
            if ((value > 0 && !positive) || (value < 0 && positive))
                return -value;  //reverse sign

            return value;
        }

        public List<object> Infos = new List<object>();


        Dictionary <ExifTags, int> _countTagPresence = new Dictionary<ExifTags, int>();

        public Dictionary<ExifTags, int> TagsPrensence => _countTagPresence;



        public void WriteAllInfomations(string filepath, StreamWriter writer)
        {
            try
            {
                // Instantiate the reader
                using (ExifReader reader = new ExifReader(filepath))
                {
                    // Extract the tag data using the ExifTags enumeration
                    InfoMessages.Clear();

                    InfoMessages.Add($"Info from picture {filepath}");

                    ExifTags[] allTags = Enum.GetValues(typeof(ExifTags)).Cast<ExifTags>().ToArray();

                    foreach (var tag in allTags)
                    {
                        bool notFound;
                        var obj = GetObject(reader, tag, out notFound);

                        if (obj != null  && notFound != true)
                        {
                            int numFoundOfThisTag = _countTagPresence.ContainsKey(tag) ? _countTagPresence[tag] : 0;

                            _countTagPresence[tag] = ++numFoundOfThisTag;

                            TagsFound.Add(tag);
                            InfoMessages.Add($"{tag}={obj} is {obj.GetType()}");
                        }
                        else
                            TagsNotFound.Add(tag);
                    }

                }
            }
            catch (Exception ex)
            {
                InfoMessages.Add($"Exception while GetExifInformation {filepath} {ex.Message}");
            }

            finally 
            {
                if (writer!=null)
                    InfoMessages.ForEach(m => writer.WriteLine(m));
            }
            
        }


        public void WritePresences(StreamWriter writer)
        {
            writer.WriteLine("Presense TagName ->  Count");
            foreach (var present in TagsPrensence.ToList())
            {
                writer.WriteLine(          present.Key + "->  " + present.Value);
            }

        }


        private object GetObject(ExifReader reader, ExifTags tag, out bool notFound)
        {
            var tagname = tag.ToString().ToLower();


            double db24 = Get24BitPosition(reader, tag, out notFound);
            if (!notFound)
                return db24;

            DateTime dt = Get<DateTime>(reader, tag, out notFound);
            if (!notFound)
                return dt;

            long lg = Get<long>(reader, tag, out notFound);

            if (!notFound)
                return lg;

            double db = Get<double>(reader, tag, out notFound);

            if (!notFound)
                return db;

            int num = Get<int>(reader, tag, out notFound);

            if (!notFound)
                return num;

                var str = GetString(reader, tag, out notFound);

            if (!notFound)
                return str;

            return null;
        }


        double Get24BitPosition(ExifReader reader, ExifTags tag, out bool notFound)
        {
            double position = default(double);

            notFound = true;
            try
            {
                Double[] positionValues;

                if (reader.GetTagValue<Double[]>(tag, out positionValues))
                    position = positionValues[0] + positionValues[1] / 60 + positionValues[2] / 3600;

                notFound = double.IsNaN(position) || (Math.Abs(position) < 0.1);
            }
            catch
            {
            }

            return position;
        }


        double Get24BitPosition(ExifReader reader, ExifTags tag, ExifTags alternateTag, out bool notFound)
        {
            Double[] positionValues;
            double position = default(double);

            notFound = true;
            try
            {
                if (reader.GetTagValue<Double[]>(tag, out positionValues))
                    position = positionValues[0] + positionValues[1]/60 + positionValues[2]/3600;

                notFound = double.IsNaN(position) || (Math.Abs(position) < 0.1);

                if (notFound && (reader.GetTagValue<Double[]>(alternateTag, out positionValues)))
                {
                    position = positionValues[0] + positionValues[1] / 60 + positionValues[2] / 3600;

                    notFound = double.IsNaN(position) || (Math.Abs(position) < 0.1);
                }  
                
            }
            catch
            {
            }

            return position;
        }




        private bool HasValue<T>(ExifReader reader, ExifTags tag)
        {
            T defValue = Activator.CreateInstance<T>();
            T value = Activator.CreateInstance<T>();
            try
            {
                reader.GetTagValue<T>(tag, out value);
                return (Comparer<T>.Default.Compare(value, defValue) != 0);
            }
            catch
            {
                return false;
            }

        }




        private string GetString(ExifReader reader, ExifTags tag, out bool notFound)
        {
            notFound = true;

            string value = null;

            bool isRead = false;
            try
            {

                isRead = reader.GetTagValue<string>(tag, out value);
            }
            catch
            {
            }

            if (isRead && ! string.IsNullOrEmpty(value.Trim()))
            {
                notFound = false;
                return value;
            }

            return string.Empty;
        }


        private T Get<T>(ExifReader reader, ExifTags tag, out bool notFound )
        {
            notFound = true;

            if (typeof(T) == typeof(string))
            {
                string strValue = GetString(reader, tag, out notFound);

                return (T)Convert.ChangeType(strValue, typeof(T));
            }

            T value = Activator.CreateInstance<T>();

            bool isRead = false;
            try
            {
               
                isRead = reader.GetTagValue<T>(tag, out value);
            }
            catch
            {
            }

            if (isRead && Comparer<T>.Default.Compare(value, Activator.CreateInstance<T>()) != 0)
            {
                notFound = false;
                return value;
            }

            return value;
        }

        private T Get<T>(ExifReader reader, ExifTags tag, ExifTags alternameTag, out bool notFound)
        {
            notFound = false;

            T value = Get<T>(reader, alternameTag, out notFound);

            return notFound ? Get<T>(reader, alternameTag, out notFound) : value;
        }

    }
}