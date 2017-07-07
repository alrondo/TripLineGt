namespace TripLine.Toolbox.UI
{
    using System.Windows;
    using System.Windows.Controls;

    public static class ButtonFlowExt
    {
        public static Button SetClick(this Button button, RoutedEventHandler action)
        {
            button.Click += action;
            return button;
        }
    }
}