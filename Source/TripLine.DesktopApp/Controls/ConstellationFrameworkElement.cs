
namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ConstellationFrameworkElement : FrameworkElement
    {
        // Create a collection of child visual objects. 
        private VisualCollection _children;

        public Point[] Constellation { get; set; }

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
            "Constellation", typeof(Point[]), typeof(ConstellationFrameworkElement), new PropertyMetadata(default(Point[]), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConstellationFrameworkElement)d).Constellation = (Point[])e.NewValue;
            ((ConstellationFrameworkElement)d).Redraw();
        }

        public ConstellationFrameworkElement()
        {
            _children = new VisualCollection(this);
            // _children.Add(DrawTarget());
            this.Loaded += OnLoaded;
            this.LayoutUpdated += OnLayoutUpdated;
        }


        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            //Redraw();
        }

        private void Redraw()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (Constellation != null)
                    {
                        lock (_children)
                        {
                            _children.Add(DrawConstellation(Constellation));
                            if (_children.Count == 4)
                            {
                                _children.RemoveAt(1);
                            }

                            _children.Add(DrawTarget());
                            if (_children.Count == 4)
                            {
                                _children.RemoveAt(1);
                            }
                        }
                    }

                });
            }
            catch
            {
                // Can crash here if closing the app.

            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _children.Clear();
            if (_children.Count == 0)
                _children.Add(DrawTarget());

            Loaded -= OnLoaded;
            //Redraw();
        }

        private DrawingVisual DrawConstellation(Point[] points)
        {
            var drawingVisual = new DrawingVisual();
           
            //drawingVisual.CacheMode = new BitmapCache();
            
            
            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext dc = drawingVisual.RenderOpen();

            var height = (Parent as FrameworkElement).ActualHeight;
            var width = (Parent as FrameworkElement).ActualWidth;


            double x_scale = width / 44000.0f;
            double y_scale = height / 44000.0f;
            Pen pen = new Pen(Application.Current.Resources["ConstellationBrush"] as Brush, 1);
            pen.Freeze();

            foreach (var point in points)
            {
                var p = new Point((point.X + 22000.0f) * x_scale, (point.Y + 22000.0f) * y_scale);
                if (p.X >= 0 && p.X <= width && p.Y >= 0 && p.Y <= height)
                {
                    dc.DrawLine(pen, p, new Point(p.X+1, p.Y));
                }
            }

            dc.Close();

            return drawingVisual;
        }

        private DrawingVisual DrawTarget()
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            var height = (Parent as FrameworkElement).ActualHeight;
            var width = (Parent as FrameworkElement).ActualWidth;

            var pen = new Pen(Application.Current.Resources["ConstellationTargetBrush"] as Brush, 2);
            pen.Freeze();

            drawingContext.DrawLine(pen, new Point(0, height / 2), new Point(width, height / 2));
            drawingContext.DrawLine(pen, new Point(width / 2, 0), new Point(width / 2, height));

            drawingContext.Close();

            return drawingVisual;
        }
        
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method. 
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }
}