namespace TripLine.Toolbox.UI
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    public static class ListBoxFlowExt
    {
        public static ListBox AddList<T>(this ListBox list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Items.Add(item);
            }

            return list;
        }

        public static ListBox SetSelectedItem(this ListBox list, SelectionChangedEventHandler eventHandler)
        {
            list.SelectionChanged += eventHandler;
            return list;
        }
    }

}