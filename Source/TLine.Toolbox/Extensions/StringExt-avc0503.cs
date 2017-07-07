using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TLine.Toolbox.Extensions
{
    using System.IO;

    public static class StringExt
    {
        public static string PathCombine(this string left, string right)
        {
            return Path.Combine(left, right);
        }

        public static Dir Dir(this string path)
        {
            return new Dir(path);
        }

        public static string Format(this string str, params object[] @params)
        {
            return string.Format(CultureInfo.InvariantCulture, str, @params);
        }

        public static Guid GetDeterministicGuid(this string input)
        {
            // The following code was taken from here: http://stackoverflow.com/questions/2642141/how-to-create-deterministic-guids
            // Note that it is the exact same code as it was used in the DIT to generate unique and reproducible facts IDs.

            //use MD5 hash to get a 16-byte hash of the string: 
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            byte[] hashBytes = provider.ComputeHash(inputBytes);

            //generate a guid from the hash: 
            Guid hashGuid = new Guid(hashBytes);

            return hashGuid;
        }

        // The following code was taken from here: http://stackoverflow.com/questions/16100/how-do-i-convert-a-string-to-an-enum-in-c
        public static T ToEnum<T>(this string value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return (T)Enum.Parse(typeof(T), value, true);
        }

        // The following code was taken from here: http://stackoverflow.com/questions/16100/how-do-i-convert-a-string-to-an-enum-in-c
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct, IConvertible
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            T result;
            return Enum.TryParse<T>(value, true, out result) ? result : defaultValue;
        }
    }
}