using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using TLine.DpSystem.Dto;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TLine.DpSystem.Ui.Configuration.Core.Converters;
    using TLine.DpSystem.Ui.Configuration.Core.Model;
    using TLine.DpSystem.Ui.Configuration.Core.Properties;
    using TLine.Toolbox.UI;

    public class ChannelLabel : Grid, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
            "Child", typeof(object), typeof(ChannelLabel), new PropertyMetadata(default(object), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChannelLabel)d).Child = e.NewValue;
        }

        private TextBlock _text;

        private Ellipse _circle;
        private TextBlock _circleInnerText = new TextBlock();

        private Rectangle _streamDirection;

        private FrameworkElement _filterIndex;

        private bool _enabled;

        private Rectangle _disableFilter;

        private Grid _grid;

        public object Child
        {
            get { return (object)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value);
                DataContext = value;
            }
        }

        public FrameworkElement FilterIndex
        {
            get { return _filterIndex; }
            set
            {
                if (_filterIndex != null)
                {
                    _grid.Children.Remove(_filterIndex);
                }
                _filterIndex = value;
                if (value != null)
                {
                    _grid.Children.Add(_filterIndex);
                    SetColumn(_filterIndex, 1);
                }
            }
        }

        public ChannelLabel()
        {
            Loaded += OnLoaded;
            this.OnPropChanged().When(nameof(Child)).Call(Remap);
            Enabled = true;

            // Define child grid. 
            _grid = new Grid();
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

        }
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    SetVisual();
                }
            }
        }

        
        

        

        private void SetVisual()
        {
            if (_text == null) return;
            if (_enabled)
            {
                _text.Foreground = Application.Current.Resources["HeaderForegroundColor"] as Brush;
                SetStatusCircleVisibilty(true);
                if (FilterIndex != null && !_grid.Children.Contains(FilterIndex)) _grid.Children.Add(FilterIndex);
            }
            else
            {
                _text.Foreground = Application.Current.Resources["ChannelDisabled"] as Brush;
                SetStatusCircleVisibilty(false);
                if (FilterIndex != null && _grid.Children.Contains(FilterIndex)) _grid.Children.Remove(FilterIndex);
            }
        }
        private void Remap()
        {
            //DataContext = Child;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Redraw();
        }

        private void Redraw()
        {
            double offset = 0.40;
            double childHeight = ActualHeight - (ActualHeight * offset);

            if (_streamDirection != null)
            {
                _streamDirection.Height = childHeight;
                // Could not figure out why the icon looked too large, hence the magic number
                _streamDirection.Width = childHeight / 1.2;
            }
            if (_circle != null)
            {
                _circle.Height = childHeight;
                _circle.Width = childHeight;

                _circleInnerText.Width = childHeight;

            }
            if (_filterIndex != null)
            {
                _filterIndex.Height = childHeight;
                _filterIndex.Width = childHeight;
            }
        }

        private void UpdateColor(object sender, PropertyChangedEventArgs args)
        {
            Dispatcher.Invoke(
                () =>
                    {
                        if (_circle != null)
                        {
                            _circle.Fill = ChannelColors.GetChannelColor((IChannel)sender);
                            _circleInnerText.Text = ChannelColors.GetChannelStatusText((IChannel)sender);
                        }

                    });
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {

            var parent = this.TryFindParent<ListBox>();
            if (parent != null)
            {
                if (DataContext is UiCard)
                {
                    Height = 45;
                    _grid.Margin = new Thickness(5, 5, 10, 5);
                }
                if (DataContext is IChannel)
                {
                    Height = 38;
                    _grid.Margin = new Thickness(20, 5, 10, 5);
                    EventInfo runtimeEvent = DataContext.GetType().GetRuntimeEvent("PropertyChanged");
                    var del = Delegate.CreateDelegate(runtimeEvent.EventHandlerType, this, this.GetType().GetMethod("UpdateColor", 
                        BindingFlags.NonPublic|BindingFlags.Instance));
                    runtimeEvent.AddEventHandler(DataContext, del);
                }
            }
            
            _text = new TextBlock();
            var bind = new Binding("Name");
            bind.Converter = new ToUpperonverter();
            _text.SetBinding(TextBlock.TextProperty, bind);
            _text.HorizontalAlignment = HorizontalAlignment.Left;
            var view = new Viewbox() { Child = _text };
            view.HorizontalAlignment = HorizontalAlignment.Left;
            _text.Foreground = Application.Current.Resources["HeaderForegroundColor"] as Brush;

            _grid.Children.Add(view);
            // Unused property
            _disableFilter = new Rectangle(){Opacity = 0.75};

            // Add an icon for the stream direction
            if (DataContext is UiCard && 
               (DataContext as UiCard).StreamDirection != StreamDirectionEnum.Unknown)
            {
                _streamDirection = new Rectangle();
                _streamDirection.HorizontalAlignment = HorizontalAlignment.Right;
                _streamDirection.VerticalAlignment = VerticalAlignment.Center;
                _streamDirection.Fill = Application.Current.Resources["HeaderForegroundColor"] as Brush;

                if ((DataContext as UiCard).StreamDirection == StreamDirectionEnum.Up)
                {
                    _streamDirection.OpacityMask = new VisualBrush(Application.Current.Resources["appbar_arrow_up"] as Visual);
                }
                else if ((DataContext as UiCard).StreamDirection == StreamDirectionEnum.Down)
                {
                    _streamDirection.OpacityMask = new VisualBrush(Application.Current.Resources["appbar_arrow_down"] as Visual);
                }
                _grid.Children.Add(_streamDirection);
                SetColumn(_streamDirection, 2);
            }
            else
            {
                BuildStatusCircleVisualObject();
            }
            Children.Add(_grid);
            UpdateColor(DataContext, null);
            Loaded -= OnLoaded;
        }

        public static string CircleStatusText { get; }= "*";

        void BuildStatusCircleVisualObject()
        {
            _circle = new Ellipse();
            double offset = 0.40;
            _circle.Height = ActualHeight - (ActualHeight * offset);
            _circle.Width = ActualHeight - (ActualHeight * offset);
            _circle.Fill = Application.Current.Resources["ChannelDisabled"] as Brush;
            _circle.HorizontalAlignment = HorizontalAlignment.Right;
            _circle.VerticalAlignment = VerticalAlignment.Center;
            _circle.Margin = new Thickness(10, 0, 0, 0);
   
            SetColumn(_circle, 2);

            _circleInnerText.Margin = new Thickness(14, CircleStatusText == "*" ? 10 : 0, 0, 0);
            _circleInnerText.Text = "";
            _circleInnerText.FontSize = CircleStatusText == "*" ? 22.0 : 12.0;

            string fontName = CircleStatusText == "*" ? "Elephant" : "Terminal";
            _circleInnerText.FontFamily = new FontFamily(fontName);
            _circleInnerText.FontStretch = CircleStatusText == "*"
                ? FontStretches.UltraExpanded
                : FontStretches.SemiCondensed;

            _circleInnerText.FontWeight = FontWeights.UltraBold;

            _circleInnerText.HorizontalAlignment = HorizontalAlignment.Right;
            _circleInnerText.VerticalAlignment = VerticalAlignment.Center;
            _circleInnerText.TextAlignment = TextAlignment.Center;

            SetColumn(_circleInnerText, 2);

            SetStatusCircleVisibilty(true);
        }


        public void SetStatusCircleVisibilty(bool visible)
        {
            if (visible && !_grid.Children.Contains(_circle))
            {
                _grid.Children.Add(_circle);

                _circleInnerText.Text = "";
                _grid.Children.Add(_circleInnerText);
            }
            else
            if (!visible && _grid.Children.Contains(_circle))
            {
                _grid.Children.Remove(_circle);

                _circleInnerText.Text = "";
                _grid.Children.Remove(_circleInnerText);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}