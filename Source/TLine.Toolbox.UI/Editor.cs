namespace TripLine.Toolbox.UI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using TripLine.Toolbox.Extensions;

    public class Editor
    {
        private FrameworkElement _source;
        private FrameworkElement _editor;
        private int _listPosition;
        private Action<UIElement, UIElement, Action> _swap;
        private Panel _parent;
        private bool _editorIsShown = false;
        private bool _comboBoxOpened;

        public event Action<Editor> OnEditorShow;

        public bool ShowingEditor
        {
            get
            {
                return _editorIsShown;
            }
        }
        public Editor(FrameworkElement source)
        {
            _source = source;
            if (_source.Parent is StackPanel)
            {
                _parent = (_source.Parent as StackPanel);
                _listPosition = _parent.Children.IndexOf(_source);
                _swap = (s, r, a) =>
                {
                    _parent.Children.Remove(s);
                    if (a!=null)
                        a();
                    if (!_parent.Children.Contains(r))
                        _parent.Children.Insert(_listPosition, r);
                };
            }
            else if (_source.Parent is Viewbox)
            {
                var parent = _source.Parent as Viewbox;
                _swap = (s, r, a) =>
                {
                    parent.Child = null;
                    if (a != null)
                        a();
                    parent.Child = r;
                };

            }


            _source.MouseEnter += SourceOnMouseEnter;
            _source.MouseLeave += SourceOnMouseLeave;
            _source.MouseUp += SourceOnMouseUp;
        }

        private void SourceOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var border = _source.Parent as Border;
            if (border == null) return;
                
            _editor.Height = _source.Height;
            var bind = _source.GetDefaultBind();
            _editor.SetDefaultBind(bind);
            _swap(border, _editor, () => border.Child = null);
            _editor.Focus();
            _editorIsShown = true;
            _comboBoxOpened = false;
            if (OnEditorShow != null) OnEditorShow(this);
        }

        private void SourceOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_editorIsShown)
                return;
            var border = _source.Parent as Border;

            _swap(border, _source, () => border.Child = null);
        }

        private void SourceOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var border = new Border() { BorderBrush = Brushes.Blue, BorderThickness = new Thickness(1)};
            _swap(_source, border, () => border.Child = _source);

        }

        public Editor Set(Control edit)
        {
            _editor = edit;
            if (_editor is TextBox)
            {
                (_editor as TextBox).LostFocus += (sender, args) => SwapToOriginal();

                _editor.KeyUp += (sender, args) =>
                {
                    if (args.Key == Key.Enter)
                    {
                        SwapToOriginal();
                    }
                };
            }
            if (_editor is ComboBox)
            {
                //(_editor as ComboBox).LostFocus += (sender, args) => SwapToOriginal();
                _editor.CastTo<ComboBox>().DropDownOpened += (sender, args) =>
                {
                    _comboBoxOpened = true;
                };
                (_editor as ComboBox).SelectionChanged += (sender, args) =>
                {
                    if (_editorIsShown && _comboBoxOpened)
                        SwapToOriginal();
                };
            }

            // Don't swap to original on checkboxes, just show the editor
            if (_editor is CheckBox)
            {
                var bind = _source.GetDefaultBind();
                _editor.SetDefaultBind(bind);
                _swap(_source, _editor, null);
                _editorIsShown = true;
            }
            return this;
        }

        public void SwapToOriginal()
        {
            if (_editorIsShown && !(_editor is CheckBox))
            {
                _swap(_editor, _source, null);
                _editorIsShown = false;
            }
        }
    }
}
