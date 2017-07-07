namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    using TLine.Toolbox.UI;

    public class Header : Grid
    {
        private StackPanel _stack;

        public Header()
        {
            this.CreateColumns("30", "*");
            _stack = new StackPanel();
            _stack.Orientation = Orientation.Horizontal;
            _stack.FlowDirection = FlowDirection.RightToLeft;
            this.Cell().Column(1).AddUi(_stack);
            
        }

        public UIElementCollection Items
        {
            get
            {
                return _stack.Children;
            }
        }


        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }
    }
}