namespace TripLine.Toolbox.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class EnumarableExt
    {
        public static void ForEach<T>(this IEnumerable<T> @enum, Action<T> act) where T : class
        {
            foreach (var item in @enum)
            {
                act(item);
            }
        }
    }
}