namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using TLine.Toolbox.UI;

    public class HostConnectionCtrl : Button
    {
        public HostConnectionCtrl()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            Width = 300;

            var command = new Binding("SelectedCommand");
            SetBinding(Button.CommandProperty, command);

            ContextMenu cm = new ContextMenu();
            MenuItem mi = new MenuItem();
            mi.Header = "Delete Connection";
            mi.SetBinding(HeaderButton.CommandProperty, new Binding("RemoveCommand"));
            cm.Items.Add(mi);
            this.ContextMenu = cm;
            

            var stack = new StackPanel();

            var image = new Image();
            image.Source = new BitmapImage(ResourceLoader.CreateAbsoluteUri("Resources/dp1000.png"));
            stack.Children.Add(image);

            var hostName = new Label();
            hostName.FontSize = 22;
            hostName.SetDefaultBind(new Binding("ConnectionStr"){Source = DataContext});
            hostName.HorizontalAlignment = HorizontalAlignment.Center;
            hostName.Foreground = Application.Current.Resources["HeaderForegroundColor"] as Brush;
            stack.Children.Add(hostName);
            Content = stack;
            Background = Application.Current.Resources["HostConnectionButton"] as Brush;
        }
    }
}