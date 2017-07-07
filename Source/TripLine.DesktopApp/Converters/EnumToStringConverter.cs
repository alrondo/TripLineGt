using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace TripLine.DesktopApp.Converters
{
    
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
                else
                {
                    return value.ToString();
                }
            }
            return value.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ( targetType.IsEnum)
            {
                string[] names = Enum.GetNames(targetType);
                foreach (string name in names)
                {
                    var val = Convert(Enum.Parse(targetType, name), null, null, null);
                    if (val.Equals(value))
                    {
                        var en = Enum.Parse(targetType, name);
                        return en;
                    }
                }

                throw new ArgumentException("The string is not a description or value of the specified enum.");
            }
            else
            {
                var returnValue = value.ToString();
                returnValue = returnValue.Replace(" ", String.Empty);
                return returnValue;
            }
        }
    }
}