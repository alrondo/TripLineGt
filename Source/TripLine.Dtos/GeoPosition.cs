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

        private const float _epsilon = (float)0.0001;

        public bool IsAlike(GeoPosition position)
        {
            return (IsAlike(position.Latitude, position.Longitude));
        }


        public bool IsAlike(float latitude, float longitude)
        {
            float epsilon =  (float)0.0001;

            return (      HasMinimalDifference(Longitude, longitude, _epsilon)
                       && HasMinimalDifference(Latitude, latitude, _epsilon));
        }

        public static bool HasMinimalDifference(float value1, float value2, float epsilon)
        {
            Debug.Assert(epsilon <= 0.1);

            if ((value1 > 0 && value2 < 0) || (value1 < 0 && value2 > 0))
                return false;   // not same sign

            bool near = (Math.Abs(value1 - value2) <  epsilon);

            return near;
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