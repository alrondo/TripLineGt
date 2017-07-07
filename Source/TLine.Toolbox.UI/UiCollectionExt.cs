namespace TripLine.Toolbox.UI
{
    using System;
    using System.Windows.Controls;

    public static class UiCollectionExt
    {
        public static void ForEach<T>(this UIElementCollection collection, Action<T> act) where T : class 
        {
            foreach (var item in collection )
            {
                if (item is T)
                    act(item as T);
            }
        }

        public static void ForEach(this UIElementCollection collection, Action<object> act)
        {
            foreach (var item in collection)
            {
                    act(item);
            }
        }

    }
}