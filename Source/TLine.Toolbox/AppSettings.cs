using System;
using System.CodeDom;
using System.Configuration;
using System.Globalization;
using TripLine.Toolbox.Extensions;

namespace TripLine.Toolbox
{
    public class AppSettings
    {

        public static T GetAppSetting<T>(string key, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key value cannot be null or empty");
            }

            string value = null;
            try
            {
                value = ConfigurationManager.AppSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                // Ignore
            }

            if (value != null)
            {
                var type = typeof(T);
                try
                {
                    if (type.IsEnum)
                    {
                        return (T)Enum.Parse(type, value.ToString(), true);
                    }

                    return (T)Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    var msg = StringExt.Format("Failed to convert AppSetting '{0}={1}' to type '{2}'. Using default value '{3}'", key, value, type, defaultValue);
                    throw new ConfigurationErrorsException(msg, ex);
                        
                }
            }

            return defaultValue;
        }


    }
}