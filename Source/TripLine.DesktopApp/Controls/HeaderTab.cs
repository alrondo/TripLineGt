namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class HeaderTab : StackPanel
    {
        public event Action<HeaderButton> OnSelectionChange;

        public HeaderButton Current { get; set; }

        public HeaderTab()
        {
            Orientation = Orientation.Horizontal;
            Loaded += OnLoaded;
            
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            
        }

        public void ClearButtons()
        {
            Children.Clear();
        }

        public void AddButton(HeaderButton button)
        {
            button.Click += ButtonOnClick;
            button.Margin = new Thickness(10,0,0,0);
            Children.Add(button);
            if (button.Selected) Current = button;
        }

        public void SetCurrentButton(HeaderButton button)
        {
            foreach (var child in Children.Cast<HeaderButton>())
            {
                if (child == button)
                {
                    Current = child;
                    Current.Selected = true;
                    continue;
                }
                child.Selected = false;
            }

            if (OnSelectionChange != null)
                OnSelectionChange(button);
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            SetCurrentButton(sender as HeaderButton);
        }

        
    }

    public class HeaderButton : Button
    {

        private Brush _unselectedColor;

        private Brush _selectedColor;

        private bool _selected;

        public HeaderButton()
        {
            Loaded += OnLoaded;
            
            Click += OnClick;
            Selected = false;
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected == value) return;
                _selected = value;
                ChangeColor();
            }
        }

        private void ChangeColor()
        {
            if (Selected)
            {
                Background = _selectedColor;
                Foreground = Application.Current.Resources["TabButtonTextSelected"] as Brush;
            }
            else
            {
                Background = _unselectedColor;
                Foreground = Application.Current.Resources["TabButtonTextUnselected"] as Brush;

            }
        }

        private void OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Selected) return;

            Selected = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            Style = Application.Current.Resources["TabButtonStyle"] as Style;
            _unselectedColor = Brushes.Transparent;
            _selectedColor = Application.Current.Resources["SelectedTabColor"] as Brush;
            BorderThickness = new Thickness(0);
            FontSize = 20;
            ChangeColor();
        }
    }
}