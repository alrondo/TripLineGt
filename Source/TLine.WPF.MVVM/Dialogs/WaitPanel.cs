using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using MahApps.Metro.Controls;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace TripLine.WPF.MVVM.Dialogs
{

    public class WaitPanel
    {
        private SimpleDialog _dialog = new SimpleDialog();

        private const string _defaultWaitMessage = "Please wait!";
        private const string _defaultPromptQuestion = "Do you want to proceed?";

        private Grid _mainGrid;
        private MetroWindow _window;

        private DockPanel _progressPanel;
        private DockPanel _promptPanel;
        private Label     _titleLabel;

        private string ProgressTitle { get; }
        private string WaitMessage { get; }


        private string PromptTitle { get; }

        private string PromptMessage { get; set; }
        private string PromptQuestion { get; set; }
        

        public delegate void UserConfirmationHandler(bool confirmationIsOk);

        private UserConfirmationHandler _userConfirmationHandler = null;

        public WaitPanel(string promptTitle, string promptMessage, string progressTitle,
                         string waitMessage = _defaultWaitMessage, string promptQuestion = _defaultPromptQuestion)
        {
            PromptTitle = promptTitle;
            PromptMessage = promptMessage;

            ProgressTitle = progressTitle;
            WaitMessage = waitMessage;
            PromptQuestion = promptQuestion;
        }

        public async Task<bool> ShowWaitPanelWithPrompt()
        {
            bool promptResponse = false;
            bool confirmationPromptResponded = false;

            ShowWaitPanel(true, (bool ok) =>
            {
                promptResponse = ok;
                confirmationPromptResponded = true;
            });
            
            // wait for the user response...
            while (!confirmationPromptResponded)
            {
                await Task.Delay(200);
            }

            if (promptResponse == false)
            {   // cancel
                HideWaitPanel();
            }

            return promptResponse;
        }

        public void ShowWaitPanel()
        {
            ShowWaitPanel(false, null);
        }

        private void ShowWaitPanel(bool doPrompt = true, UserConfirmationHandler confirmationHandler = null)
        {
            if (doPrompt == true && confirmationHandler == null)
                throw new ArgumentNullException("confirmationHandler is required when doPrompt==true");

            _userConfirmationHandler = confirmationHandler;

            _window = (MetroWindow) Application.Current.MainWindow;
            _window.ShowTitleBar = false;

            if (_window.Content is Grid)
            {
                _mainGrid = _window.Content as Grid;

                _dialog = new SimpleDialog();

                Panel promptButtonPanel = null;

                if (_userConfirmationHandler != null)
                    promptButtonPanel = CreateButtonPanel();

                if (_mainGrid.ColumnDefinitions.Count > 0)
                    Grid.SetColumnSpan(_dialog, _mainGrid.ColumnDefinitions.Count);
                if (_mainGrid.RowDefinitions.Count > 0)
                    Grid.SetRowSpan(_dialog, _mainGrid.RowDefinitions.Count);

                _dialog.Content = CreateContentPanel(promptButtonPanel);
            }

            _mainGrid.Children.Add(_dialog);
        }


        private double _fontSize = 14.0;

        private Panel CreateButtonPanel()
        {
            StackPanel buttonStack = new StackPanel() {Orientation = Orientation.Horizontal};
            buttonStack.HorizontalAlignment = HorizontalAlignment.Center;

            Button proceedButton = new Button() {Width = 60, Height = 30, FontSize = _fontSize, VerticalContentAlignment = VerticalAlignment.Center};
            proceedButton.Margin = new Thickness(0, 0, 0, 15);
            proceedButton.Content = "Yes";
            proceedButton.Click += OkButton_Click;

            Button cancelButton = new Button() {Width = 60, Height = 30, FontSize = _fontSize, VerticalContentAlignment = VerticalAlignment.Center };
            cancelButton.Margin = new Thickness(25, 0, 0, 15);
            cancelButton.Content = "No";
            cancelButton.Click += CancelButton_Click;

            buttonStack.Children.Add(proceedButton);
            buttonStack.Children.Add(cancelButton);
            return buttonStack as Panel;
        }
        
        private Panel CreateContentPanel(Panel buttonPanel = null)
        {
            StackPanel contentStack = new StackPanel() {Orientation = Orientation.Vertical, Margin = new Thickness(50), HorizontalAlignment = HorizontalAlignment.Center };

            var titlePanel = new StackPanel() {};

            _titleLabel = new Label() { FontSize = 18, HorizontalAlignment = HorizontalAlignment.Left, FontWeight = FontWeights.Bold };

            _titleLabel.Content = buttonPanel != null ? PromptTitle : ProgressTitle;

            titlePanel.Children.Add(_titleLabel);

            // progreass
            _progressPanel = new DockPanel()  {Margin = new Thickness(5, -5, 0, 10)};

            var progressMessage = CreateTextBloc(WaitMessage, right: 30);
            progressMessage.VerticalAlignment = VerticalAlignment.Center;
            
            var progressRing = new ProgressRing() {IsActive = true, Margin = new Thickness(30, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Left};

            DockPanel.SetDock(progressMessage, Dock.Left);
            DockPanel.SetDock(progressRing, Dock.Left);
            
            _progressPanel.Children.Add(progressMessage);
            _progressPanel.Children.Add(progressRing);

            // Prompt (if any)
            if ( buttonPanel != null)
            {   
                _progressPanel.Visibility = Visibility.Collapsed;

                _promptPanel = new DockPanel() { Margin = new Thickness(5, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };

                var promptMessage = CreateTextBloc(PromptMessage, bottom: 10);
                var promptQuestion = CreateTextBloc(PromptQuestion, top:5, bottom: 20);
                promptQuestion.VerticalAlignment = VerticalAlignment.Center;
 
                DockPanel.SetDock(promptMessage, Dock.Top);
                DockPanel.SetDock(promptQuestion, Dock.Top);
                DockPanel.SetDock(buttonPanel, Dock.Bottom);

                _promptPanel.Children.Add(promptMessage);
                _promptPanel.Children.Add(promptQuestion);
                _promptPanel.Children.Add(buttonPanel);
            }

            // add panels to content stack...
            contentStack.Children.Add(titlePanel);
            contentStack.Children.Add(_progressPanel);
            if(_promptPanel != null)
                contentStack.Children.Add(_promptPanel);

            return contentStack as Panel;
        }



        private TextBlock CreateTextBloc(string text, double fontSize=18.0, 
                                        double left=0, double top=0, double right=0, double bottom=0 )
        {
            return new TextBlock()
            {
                Text = text,
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(left, top, right, bottom)
            };
        }

      
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window == null)
                return;

            _titleLabel.Content = ProgressTitle;

            if (_promptPanel.Visibility == Visibility.Visible)
            {
                _progressPanel.Height = _promptPanel.ActualHeight;
                _progressPanel.Width = _promptPanel.ActualWidth;

                _promptPanel.Visibility = Visibility.Collapsed;
                _progressPanel.Visibility = Visibility.Visible;
            }
            

       
            _userConfirmationHandler(true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window == null)
                return;

            _userConfirmationHandler(false);
            HideWaitPanel();
        }


        public void HideWaitPanel()
        {
            if (_window != null && _mainGrid != null && _dialog != null)
            {
                _window.ShowTitleBar = true;
                _mainGrid.Children.Remove(_dialog);
            }

            _window = null;
        }
    }
}
