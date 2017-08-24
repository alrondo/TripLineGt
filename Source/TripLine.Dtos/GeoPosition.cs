using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TripLine.Dtos
{
    public class GeoPosition
    {
        public float Latitude { get; set; } = 0;

        public float Longitude { get; set; } = 0;


        public string LatLong => $"{Latitude},{Longitude}";


        public bool IsAlike(GeoPosition position, uint units = 3)
        {
            return (IsAlike(position.Latitude, position.Longitude, units));
        }


        public bool IsAlike(float latitude, float longitude, uint units=3)
        {
            return (      HasMinimalDifference(Longitude, longitude, units)
                       && HasMinimalDifference(Latitude, latitude, units));
        }

        public static bool HasMinimalDifference(float value1, float value2, uint units)
        {
            Debug.Assert(units >1);

            if ((value1 > 0 && value2 < 0) || (value1 < 0 && value2 > 0))
                return false;   // not same sign

            return (Math.Abs(value1 - value2) < Math.Pow(1.0, -units));

        }


        public GeoPosition()
        {

        }

        public GeoPosition(double latitude, double longitude)
        {
            Latitude = (float)latitude;
            Longitude = (float)longitude;
        }

        public string GetDisplay()
        {
            if (! IsValid())
                return "Unkown POS";
            
            return $"{Latitude},{Longitude}";
        }
        

        public bool IsValid ()
        {
            return (Math.Abs(Latitude) > 0.0 && Math.Abs(Longitude) > 0.0);
        }

    }
}