using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum enumValue)
        {
            var enumValueString = enumValue.ToString();
            var enumType = enumValue.GetType();
            var enumMemberInfo = enumType.GetMember(enumValueString).First();

            var descriptionAttribute = enumMemberInfo
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault();

            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }
            else
            {
                return enumValueString;
            }
        }

        public static bool IsValidValueOf<TEnum>(this int value)
        {
            var t = typeof(TEnum);
            var validValues = Enum.GetValues(t).Cast<int>().ToArray();
            return validValues.Contains(value);
        }

        public static int MinValueOf<TEnum>()
        {
            var t = typeof(TEnum);
            var validValues = Enum.GetValues(t).Cast<int>().ToArray();
            return validValues.Min();
        }

        public static int MaxValueOf<TEnum>()
        {
            var t = typeof(TEnum);
            var validValues = Enum.GetValues(t).Cast<int>().ToArray();
            return validValues.Max();
        }


    }
}
