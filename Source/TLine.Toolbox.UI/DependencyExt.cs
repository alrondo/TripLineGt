namespace TripLine.Toolbox.UI
{
    using System.Windows;
    using System.Windows.Media;

    public static class DependencyExt
    {
        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;

            if (parent != null)

                return parent;

            else

                return TryFindParent<T>(parentObject);

        }
    }
}