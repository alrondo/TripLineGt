using System.Threading.Tasks;

namespace Tripline.DesktopApp.ContentUserControl
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class BaseUserControl : UserControl
    {
        public BaseUserControl()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Resources.MergedDictionaries.Clear();

            Loaded -= OnLoaded;

        }
    }
}