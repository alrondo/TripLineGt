using System;
using System.Collections.Generic;
using System.Linq;

namespace TripLine.Toolbox.Extensions
{
    public static class EnumerableExtensions
    {
        public static T GetRandomElement<T>(this IEnumerable<T> collection)
        {
            var list = collection as IReadOnlyList<T>;

            if (list == null)
            {
                list = collection.ToList();
            }

            var index = new Random().Next(list.Count);

            return list[index];
        }
    }
}
