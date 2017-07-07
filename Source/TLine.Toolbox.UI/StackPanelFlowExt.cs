namespace TripLine.Toolbox.UI
{
    using System.Windows;
    using System.Windows.Controls;

    public class StackFlow
    {
        private readonly StackPanel _stack;

        internal StackFlow(StackPanel stack)
        {
            _stack = stack;
        }

    }

    public static class StackPanelFlowExt
    {
        public static StackPanel Add(this StackPanel panel, UIElement element)
        {
            panel.Children.Add(element);
            return panel;
        }
    }
}