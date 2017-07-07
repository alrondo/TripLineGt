namespace TripLine.Toolbox.UI.Bindings
{
    using System;
    using System.Linq.Expressions;
    using System.Windows.Data;
    using System.Windows.Shapes;

    public class Bind
    {
        public object Source { get; set; }
        public string Path { get; private set; }
    }

    public class RectangleBindings
    {
        private Rectangle _rect;

        public RectangleBindings(Rectangle rect)
        {
            _rect = rect;
        }

        public Rectangle Fill(Action<Bind> binding)
        {
            var bind = new Bind();
            binding(bind);

            var realBinding = new Binding(bind.Path);
            realBinding.Source = bind.Source;
            _rect.SetBinding(Rectangle.FillProperty, realBinding);

            return _rect;
        }
    }



    public static class RectangleBindingExt
    {
        public static RectangleBindings Bindings(this Rectangle rect)
        {
            return  new RectangleBindings(rect);
        }
    }
}