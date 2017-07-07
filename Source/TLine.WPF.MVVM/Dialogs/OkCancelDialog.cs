using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TLine.WPF.MVVM.Dialogs
{
    //This class is intended to be a base class for dialogs requiring a OK Cancel button combination. 
    public class OkCancelDialog : SimpleDialog, INotifyPropertyChanged
    {
        private readonly string _title;
        private readonly string _message;
        protected StackPanel ButtonStack;
        
        //stack panel to be populated by derived class.
        protected Grid ContentStack;
        protected Button OkButton;
        protected Button CancelButton;

        public event PropertyChangedEventHandler PropertyChanged;

        //property bound to the input textbox
        public string Response { get; set; }

        public OkCancelDialog(string title, string message)
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

            OkButton = new Button() {Content = "OK", Width = 100, Margin = new Thickness(0,0,10,0)};
            OkButton.Click += (sender, args) => CloseDialog();

            CancelButton = new Button() { Content = "Cancel", Width = 100, Margin = new Thickness(10, 0, 0, 0) };
            CancelButton.Click += (sender, args) => CloseDialog();
            
            ButtonStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0,20,0,0)
            };
            ButtonStack.Children.Add(OkButton);
            ButtonStack.Children.Add(CancelButton);

            ContentStack = new Grid();

            var stack = new Grid() {Margin = new Thickness(20, 20, 20, 20)};

            stack.CreateRows("*", "*", "*", "*").Cell()
                .Row(0).AddUi(title)
                .Row(1).AddUi(message)
                .Row(2).AddUi(ContentStack)
                .Row(3).AddUi(ButtonStack);

            
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