using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing.Imaging;

namespace TripLine.DesktopApp.Converters
{
    public class PathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
                throw new InvalidOperationException("Target type must be System.Windows.Media.ImageSource.");

            string filePath = value as string;

            try
            {

                Bitmap      bi = new Bitmap(filePath);
                BitmapImage img = ConvertToBitmapImage(bi);
                //BitmapImage img = new BitmapImage();
                img.BeginInit();
                //img.UriSource = new Uri(filePath);
                img.EndInit();
                return img;
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        private static BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();

            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Seek(0, SeekOrigin.Begin);

            img.StreamSource = stream;
            img.EndInit();

            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class zzzPathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            string filepath = value as string;
            if (filepath != null && File.Exists(filepath) )
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();

                FileStream ms = new FileStream(filepath, FileMode.Open);
                //bi.Save(ms, image.RawFormat);
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                return bi;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, 
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}