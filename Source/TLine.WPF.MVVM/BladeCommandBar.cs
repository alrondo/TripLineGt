namespace Averna.WPF.BladeUi
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using Averna.Toolbox.UI;

    public class BladeCommandBar : Grid
    {
        private StackPanel _stackpanel;
        private double _expandedWidth;

        [ImportMany(typeof(ICommandBarItem), AllowRecomposition = true)]
        private ObservableCollection<ICommandBarItem> Items { get; set; } 


        public BladeCommandBar()
        {
            Items = new ObservableCollection<ICommandBarItem>();
            Items.CollectionChanged += ItemsOnCollectionChanged;
            Loaded += OnLoaded;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SetButtons();
        }

        private void SetButtons()
        {
            if (_stackpanel == null)
            {
                return;
            }

            _stackpanel.Children.Clear();
            foreach (var commandBarItem in Items)
            {
                var button = new Button() { Content = commandBarItem.ButtonContent};
                button.Click += ButtonOnClick;
                button.Tag = commandBarItem;
                _stackpanel.Children.Add(button);
            }
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var button = sender as Button;
            var command = button.Tag as ICommandBarItem;


            if (Children.Count > 1)
            {
                MinimizeExpandedUI(command);
                return;
            }
            this.Cell().Column(1).AddUi(command.ExpandedUI);
            _expandedWidth = ((FrameworkElement) command.ExpandedUI).MinWidth;
            Width += _expandedWidth;
            command.ShouldClose += CommandOnShouldClose;
        }

        private void MinimizeExpandedUI(ICommandBarItem command)
        {
            if (Children.Count > 1)
            {
                Children.RemoveAt(1);
                Width -= _expandedWidth;

            }
        }

        private void CommandOnShouldClose(ICommandBarItem command)
        {
            MinimizeExpandedUI(command);
            command.ShouldClose -= CommandOnShouldClose;

        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            Background = Application.Current.Resources["BladeBackground"] as Brush;
            _stackpanel = new StackPanel() { Width = 100};
            SetButtons();

            this.CreateColumns("100", "*").Cell().Column(0).AddUi(_stackpanel).Column(1);
        }
    }

    public interface ICommandBarItem
    {
        UIElement ButtonContent { get; }
        string Name { get; }
        UIElement ExpandedUI { get; }
        event Action<ICommandBarItem> ShouldClose;
    }
}