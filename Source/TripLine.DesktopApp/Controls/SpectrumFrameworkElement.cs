using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    public class SpectrumFrameworkElement : FrameworkElement
    {
        public double[] Data { get; set; }
        public double Scaling { get; set; }

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
            "Data", typeof(double[]), typeof(SpectrumFrameworkElement), new PropertyMetadata(default(double[]), PropertyChangedCallback));
        public static readonly DependencyProperty ScalingProperty = DependencyProperty.Register(
            "Scaling", typeof(double), typeof(SpectrumFrameworkElement), new PropertyMetadata(default(double), ScalingPropertyCallback));

        private static void ScalingPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SpectrumFrameworkElement)d).Scaling = (double)e.NewValue;
            ((SpectrumFrameworkElement)d).Draw();
 
        }

        private VisualCollection _children;

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SpectrumFrameworkElement)d).Data = (double[])e.NewValue;
            ((SpectrumFrameworkElement)d).Draw();
        }

        public SpectrumFrameworkElement()
        {
            _children = new VisualCollection(this);
            Loaded += OnLoaded;
            Scaling = 60;
        }



        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            var height = (Parent as FrameworkElement).ActualHeight;
            var width = (Parent as FrameworkElement).ActualWidth;

            Clip = new RectangleGeometry(new Rect(new Point(0,0), new Point(width, height)));
        }

        public void Draw()
        {
            try
            {
                Dispatcher.Invoke(
            () =>
            {
                if (Data != null)
                {
                    _children.Add(DrawSignal(Data));
                    if (_children.Count == 2)
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

        private DrawingVisual DrawSignal(double[] data)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            var height = (Parent as FrameworkElement).ActualHeight;
            var width = (Parent as FrameworkElement).ActualWidth;

            var valuesToRender = data.Select(d => Math.Abs((10*Math.Log(d))))
                                     .ToList();

            var peakPower = valuesToRender.Max();

            // Adjust the vertical scale to the max value, leaving a small visual border to avoid cropping
            Scaling = peakPower + 5;

            double x_scale = width / valuesToRender.Count;
            double y_scale = height / Scaling;

            DrawingContext dc = drawingVisual.RenderOpen();
            Point lastpoint = new Point(-1, -1);

            Pen pen = new Pen(Application.Current.Resources["SpectrumSignalBrush"] as Brush, 1);
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