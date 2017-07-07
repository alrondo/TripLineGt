namespace TripLine.Toolbox.UI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    public static class ControlExt
    {
        public static Editor BindEditor(this FrameworkElement ctrl)
        {
            return new Editor(ctrl);
        }

        public static Binding GetDefaultBind(this FrameworkElement ctrl)
        {
            if (ctrl is Label)
                return BindingOperations.GetBinding(ctrl, Label.ContentProperty);
            if (ctrl is ComboBox)
                return BindingOperations.GetBinding(ctrl, ComboBox.SelectedItemProperty);
            if (ctrl is TextBox)
                return BindingOperations.GetBinding(ctrl, TextBox.TextProperty);
            if (ctrl is CheckBox)
                return BindingOperations.GetBinding(ctrl, CheckBox.IsCheckedProperty);
            if (ctrl is TextBlock)
                return BindingOperations.GetBinding(ctrl, TextBlock.TextProperty);

            return null;
        }

        public static void SetDefaultBind(this FrameworkElement ctrl, Binding bind)
        {
            if (ctrl is Label)
                ctrl.SetBinding(Label.ContentProperty, bind);
            if (ctrl is ComboBox)
                ctrl.SetBinding(ComboBox.SelectedItemProperty, bind);
            if (ctrl is TextBox)
                ctrl.SetBinding(TextBox.TextProperty, bind);
            if (ctrl is CheckBox)
                ctrl.SetBinding(CheckBox.IsCheckedProperty, bind);
            if (ctrl is TextBlock) ctrl.SetBinding(TextBlock.TextProperty, bind);
        }

        public static FrameworkElement SetDefaultBind(this FrameworkElement ctrl, string path, object source)
        {
            var binding = new Binding(path) { Source = source };
            ctrl.SetDefaultBind(binding);
            return ctrl;
        }
    }
}