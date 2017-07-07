using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TripLine.WPF.MVVM.Dialogs
{
    public class InputDialog : SimpleDialog, INotifyPropertyChanged
    {
        private readonly string _title;
        private readonly string _message;
        protected StackPanel ButtonStack;
        protected Button OkButton;
        protected Button CancelButton;
        protected TextBox InputTextBox;

        public event PropertyChangedEventHandler PropertyChanged;

        //property bound to the input textbox
        public string Response { get; set; }

        public InputDialog(string title, string message)
        {
            _title = title;
            _message = message;

            CreateMe();
        }

        private void CreateMe()
        {
            var title = new Label
            {
                Content = _title,
                FontSize = 25,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var message = new Label
            {
                Content = _message,
            };

            var bind = new Binding("Response") {Source = this};
            InputTextBox = new TextBox();

            InputTextBox.SetBinding(TextBox.TextProperty, bind);

            OkButton = new Button() {Content = "OK", Width = 100, Margin = new Thickness(0,0,10,0)};

            CancelButton = new Button() { Content = "Cancel", Width = 100, Margin = new Thickness(10, 0, 0, 0) };
            CancelButton.Click += (sender, args) =>
            {
                Response = null;
                CloseDialog();
            };

            ButtonStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0,20,0,0)
            };
            ButtonStack.Children.Add(OkButton);
            ButtonStack.Children.Add(CancelButton);

            var stack = new StackPanel() {Margin = new Thickness(20, 20, 20, 20)};
            
            stack.Children.Add(title);
            stack.Children.Add(message);
            stack.Children.Add(InputTextBox);
            stack.Children.Add(ButtonStack);

            Content = stack;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}