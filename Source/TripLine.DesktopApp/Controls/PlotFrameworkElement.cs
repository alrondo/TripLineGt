using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Timers;
    using System.Windows;
    using System.Windows.Media;

    public class PlotFrameworkElement : FrameworkElement
    {
        private VisualCollection _children;

        public List<double> Signal { get; set; }

        private SpectralEngine _engine = new SpectralEngine(2048,
        12, SpectralEngine.WindowType.Hanning, SpectralEngine.MagScale.dBFS);


        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
        "Signal", typeof(List<double>), typeof(PlotFrameworkElement), new PropertyMetadata(default(List<double>), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PlotFrameworkElement)d).Signal = (List<double>)e.NewValue;
            ((PlotFrameworkElement)d).Draw();
        }

        public PlotFrameworkElement()
        {

            _children = new VisualCollection(this);
            this.Loaded += OnLoaded;
            this.LayoutUpdated += OnLayoutUpdated;
            Unloaded += OnUnloaded;

        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
        }

        private DrawingVisual DrawSpecLines()
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            //// Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            if (Parent as FrameworkElement == null)
            {
                drawingContext.Close();

                return drawingVisual;

            }
            var height = (Parent as FrameworkElement).ActualHeight;
            var width = (Parent as FrameworkElement).ActualWidth;

            double start = 28.125;   //36.125 - 8 
            double end = 44.125;     //36.125 + 8
            double band = end - start;

            double x_scale = width / band;
            int eurostart = (int)((32.125 - start) * x_scale);
            int euroend = (int)((40.125 - start) * x_scale);
            int nastart = (int)((33.125 - start) * x_scale);
            int naend = (int)((39.125 - start) * x_scale);



            var pen = new Pen(Application.Current.Resources["ChannelPlotBrush"] as Brush, 2);
            pen.Freeze();
            drawingContext.DrawLine(pen, new Point(nastart, height * .9), new Point(nastart, height * .1));
            drawingContext.DrawLine(pen, new Point(naend, height * .9), new Point(naend, height * .1));

            drawingContext.Close();

            return drawingVisual;

        }

        private DrawingVisual DrawSignal(List<double> signal)
        {
            // Return an empty DrawingElement if there's no signal
            if (!signal.Any())
            {
                return new DrawingVisual();
            }

            var height = (Parent as FrameworkElement).ActualHeight;
            var width = (Parent as FrameworkElement).ActualWidth;

            var valuesToRender = signal.Select(d => Math.Abs((10 * Math.Log(d))))
                                     .ToList();

            // We sometimes have a few values that are we bigger than the other ones (ex: DC offset).
            // We eliminate these values from the computation of the 'max'
            var numberOfOutliersToEliminate = 10;

            double maximumValueToRenderExcludingOutliers = 0;

            if (valuesToRender.Count > 0)
            {
                maximumValueToRenderExcludingOutliers = valuesToRender.OrderByDescending(v => v)
                    .Skip(numberOfOutliersToEliminate)
                    .Max();
            }

            var peakPower = valuesToRender.Max();

            // Adjust the vertical scale to the max value, leaving a small visual border to avoid cropping
            Scaling = peakPower + 5;

            double x_scale = width / valuesToRender.Count;
            double y_scale = height / Scaling;


            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();
            Point lastpoint = new Point(-1, -1);

            Pen pen = new Pen(Application.Current.Resources["PlotBrush"] as Brush, 1);
            pen.Freeze();

            for (var i = 0; i < valuesToRender.Count; i++)
            {
                var point = new Point((float)(i * x_scale), (valuesToRender[i] * -1.0 * y_scale) + height);
                if (lastpoint.X != -1 && lastpoint.Y != -1)
                {
                    dc.DrawLine(pen, lastpoint, point);
                }
                lastpoint = point;
            }

            dc.Close();

            return drawingVisual;

        }

        public double Scaling { get; set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            _children.Add(DrawSpecLines());

        }

        private void Draw()
        {

            try
            {
                Dispatcher.Invoke(
            () =>
            {
                if (Signal != null)
                {
                    // Explanation::
                    // The SpecLines are drawn initially at index 0 (loaded).
                    // On each call,
                    // - We start with a list that contains: 0-Spec
                    // - The signal is drawn at index 1 and the speclines at index 2
                    // - Since the list exceeds 2 elements, we remove the first element, keeping the latest signal and speclines.
                    //
                    // We want to draw the speclines before removing the old ones to avoid flickering
                    // Also, if the window is resized, the speclines get drawn at the proper location
                    //

                    _children.Add(DrawSignal(Signal));
                    _children.Add(DrawSpecLines());

                    // Only keep the latest signal and speclines.
                    while (_children.Count > 2)
                    {
                        _children.RemoveAt(0);
                    }
                }

            });

            }
            catch
            {
                // Can crash here if closing the app.

            }

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