namespace TripLine.WPF.MVVM.Dialogs
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class SimpleDialog : Grid
    {
        private UIElement _content;

        public event Action Close;

        public UIElement Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                Show();
            }
        }

        private void Show()
        {
            Children.Add(Content);
            SetRow(Content, 1);
            SetColumn(Content, 1);
        }

        public SimpleDialog()
        {
            Initialized += OnInitialized;
        }

        public void CloseDialog()
        {
            if (Close != null)
            {
                Close();
            }
        }

        private void OnInitialized(object sender, EventArgs eventArgs)
        {
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Auto) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition(){ Width = new GridLength(20, GridUnitType.Star)});
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Star) });

            var rect = new Rectangle() { Fill = Application.Current.Resources["SimpleDialogOutsideBackgroundBrush"] as Brush, Opacity = 0.70 };
            //rect.MouseUp += RectOnMouseUp;
            Children.Add(rect);
            SetRowSpan(rect, 3);
            SetColumnSpan(rect, 3);

            rect = new Rectangle() { Fill = Application.Current.Resources["SimpleDialogBackgroundColor"] as Brush };
            Children.Add(rect);
            SetRow(rect, 1);
            SetColumnSpan(rect, 3);
        }


        private void RectOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
        }
    }
}