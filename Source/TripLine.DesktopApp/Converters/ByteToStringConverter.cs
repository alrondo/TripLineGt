using System;
using System.Globalization;
using System.Windows.Data;

namespace TripLine.DesktopApp.Converters
{
    // This converter will convert a number of bytes to a string representation
    // Example : 2.750 MB, 13.563 KB, 9.523 GB
    [ValueConversion(typeof(int), typeof(string))]
    public class ByteToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((double)value) / Math.Pow(2, 10) < 100)
            {
                return string.Format("{0:F3} KB", (double)value / Math.Pow(2, 10));
            }
            if ((double)value / Math.Pow(2, 20) < 1000)
            {
                return string.Format("{0:F3} MB", (double)value / Math.Pow(2, 20));
            }

            return string.Format("{0:F3} GB", (double)value / Math.Pow(2, 30));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
