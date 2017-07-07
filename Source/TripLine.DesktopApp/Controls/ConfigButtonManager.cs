using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TLine.DpSystem.Dto;
using TLine.DpSystem.Ui.Configuration.Core.ModelViews;
using TLine.DpSystem.Ui.Configuration.Core.Views;
using TLine.Toolbox.Extensions;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TLine.DpSystem.Service.Client;
    using TLine.DpSystem.Ui.Configuration.Core.Model;
    using TLine.DpSystem.Ui.Configuration.Core.Notify;
    using TLine.Toolbox.UI;
    using TLine.WPF.MVVM.Dialogs;

    public class ConfigButtonManager
    {
        private readonly DialogManager _diagManager;

        private readonly ServerClient _client;

        private readonly DpMainSystem _system;

        public Button ToolbarButton { get; private set; }
        public TextBlock NameTextBlock { get; private set; }

        private Grid _grid;

        private Border _popupContent;

        private Popup _menu;

        private Button _saveButton;

        private HardwareDefinitionDto _current;

        private Button _loadButton;

        public bool ShowIcon
        {
            get
            {
                return (ToolbarButton.Visibility == Visibility.Visible);
            }
            set
            {
                ToolbarButton.Visibility = value?Visibility.Visible:Visibility.Hidden;
                if (value)
                    GetCurrentHardwareDefAsync().Forget();
            }
        }

        private bool _unsavedChanges = false;


        private bool UnsavedChanges
        {
            get { return _unsavedChanges; }
            set
            {
                _unsavedChanges = value;
                _saveButton.IsEnabled = _unsavedChanges;
            }
        }


        private async Task GetCurrentHardwareDefAsync()
        {
            _current = await _client.GetCurrentHardwareDef();

            NameTextBlock.Text = GetDisplayFileName();
        }

        private string GetDisplayFileName()
        {
            var name = (_current?.AutoSaveFile == false) ? _current.DisplayName : string.Empty;

            if (UnsavedChanges)
                name = "* " + name;

            return name;
        }
         

        public ConfigButtonManager(DialogManager diagManager, ServerClient client, DpMainSystem system)
        {
            _diagManager = diagManager;
            _client = client;
            _system = system;
            _grid = new Grid();
            NameTextBlock = new TextBlock() {FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(12, 0, 0, 0)
            };
            ToolbarButton = new Button();
            ToolbarButton.Content = _grid;

            //hook ourself to the set command sent event to detect when the configuration has changed.
            _client.SetCommandsSent += ClientOnSetCommandsSent;

            _grid.Children.Add(new Path
            {
                Margin = new Thickness(5),
                Fill = Application.Current.Resources["ConfigButtonBrush"] as Brush,
                Stretch = Stretch.Fill,
                Data =
                    Geometry.Parse(
                        "F1 M 20,23.0001L 55.9999,23.0001C 57.1045,23.0001 57.9999,23.8955 57.9999,25.0001L 58,47C 58,48.1045 57.1045,49 56,49L 41.0001,48.9999L 41.0001,52.9999L 45.0001,52.9999C 46.1046,52.9999 47.0001,53.8953 47.0001,54.9999L 47,56.9999L 29.0001,56.9999L 29.0001,54.9999C 29.0001,53.8953 29.8955,52.9999 31.0001,52.9999L 35.0001,52.9999L 35.0001,48.9999L 20,49C 18.8955,49 18,48.1045 18,47L 18,25.0001C 18,23.8955 18.8955,23.0001 20,23.0001 Z M 21,26.0001L 21,46L 54.9999,46L 54.9999,26.0001L 21,26.0001 Z M 23,28L 27,28L 27,32L 23,32L 23,28 Z M 29,28L 53,28L 53,32L 29,32L 29,28 Z M 23,34L 27,34L 27,38L 23,38L 23,34 Z M 29,34L 53,34L 53,38L 29,38L 29,34 Z M 23,40L 27,40L 27,44L 23,44L 23,40 Z M 29,40L 53,40L 53,44L 29,44L 29,40 Z ")
            });

            ToolbarButton.Click += ToolbarButtonOnClick;

            _popupContent = new Border()
            {
                Background = Application.Current.Resources["ConfigButtonBackgroundBrush"] as Brush,
                BorderBrush = Application.Current.Resources["ConfigButtonBorderBrush"] as Brush,
                BorderThickness = new Thickness(1)
            };

            var grid = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10)};
            //grid.CreateRows("35", "35");
            _saveButton = new Button()
                            {
                                IsEnabled = false, 
                                Content = "Save", 
                                Height = 30, 
                                Margin = new Thickness(3), 
                                Style = Application.Current.Resources["MetroButton"] as Style
                            };
            _saveButton.Click += SaveButtonClick;
            grid.Children.Add(_saveButton);

            // Save button
            var saveAsButton = new Button()
                           {
                               Content = "Save As",
                               Height = 30,
                               Margin = new Thickness(3),
                               Style = Application.Current.Resources["MetroButton"] as Style
                           };
            saveAsButton.Click += SaveAsOnClick;
            grid.Children.Add(saveAsButton);

            var loadButton = new Button() 
                            { Content = "Load", 
                              Height = 30, 
                              Margin = new Thickness(3), 
                              Style = Application.Current.Resources["MetroButton"] as Style 
                            };
            loadButton.Click += LoadButtonOnClick;
            //grid.Cell().Row(1).AddUi(loadButton);
            grid.Children.Add(loadButton);
            //_popupContent.Child = grid;

            _menu = new Popup();
            _menu.Width = 150;
            _menu.Height = 150;
            _menu.Child = _popupContent;
            _menu.PlacementTarget = ToolbarButton;
            _menu.Placement = PlacementMode.Bottom;
            _menu.StaysOpen = false;

        }


        //called every time the user changes the hardware configuration 
        private void ClientOnSetCommandsSent(object sender, string s)
        {
            if (_current != null  && _current.AutoSaveFile == false)
            {
                UnsavedChanges = true;
                NameTextBlock.Text = GetDisplayFileName();
            }
        }


        private void SaveAsOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var diag = new SimpleDialog();
            var nameText = new TextBox();
            var desText = new TextBox() { TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Height = 300};
            var saveButton = new Button() {Content = "Save", Width = 100};
            
            var stack =
                new StackPanel() { Margin = new Thickness(20, 20, 20, 20) }
                    .Add(new Label { Content = "Save Hardware", 
                                     FontSize = 25, 
                                     FontWeight = FontWeights.Bold, 
                                     Margin = new Thickness(0, 0, 0, 20) })
                    .Add(new Label(){ Content = "Name"})
                    .Add(nameText)
                    .Add(new Label { Content = "Description" })
                    .Add(desText)
                    .Add(
                        new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0,20,0,0)
                            }.Add(
                                saveButton.SetClick(async (o, h) => await SaveAsConfigAsync(nameText.Text, desText.Text)))
                            .Add(
                                new Button { Content = "Cancel", Width = 100 }.SetClick((o, args) => diag.CloseDialog())));

            diag.Content = stack;
            _diagManager.ShowDialog(diag);
        }

        private async Task SaveAsConfigAsync(string name, string description)
        {
            //Check if a file with this name already exist
            var hwDefs = await _client.GetAllHardwareDef();
            if (hwDefs.Any(hwdef => hwdef.Name == name))
            {
                NotifySystem.Default.ShowWarning("A save file with this name already exist on the server.");
                return;
            }

            var hwDef = new HardwareDefinitionDto
            {
                Name = name,
                Description = description,
                CreationTime = DateTime.UtcNow
            };
            
            await _client.SaveCurrentHardwareDef(hwDef);

            UnsavedChanges = false;

            _current = hwDef;
            NameTextBlock.Text = GetDisplayFileName();
            _diagManager.CloseDialog();            
        }

        private Button _loadCancelButton;

        private async void LoadButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var hwDefs = await _client.GetAllHardwareDef();

            var diag = new SimpleDialog();
            var desText = new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 260,
                BorderThickness = new Thickness(1)
            };
            var list = new ListBox().AddList(hwDefs.Select(h => h.Name))
                .SetSelectedItem(
                    (o, args) =>
                    {

                        var l = o as ListBox;
                        if (l.SelectedItem == null)
                        {
                            _loadButton.IsEnabled = false;
                            return;
                        }

                        var hwd = hwDefs.First(h => h.Name.Equals(l.SelectedItem));
                        desText.Text = hwd.Description;
                        _loadButton.IsEnabled = true;
                    })
                ;
            list.BorderThickness = new Thickness(1);
            list.Margin = new Thickness(0, 0, 10, 0);

            _loadButton = new Button() { Content = "Load",Width = 100,IsEnabled = false, Margin = new Thickness(0, 0, 5, 0)}.SetClick(async (o, args) 
                => await LoadAndCloseDialogAsync(hwDefs, list, diag));
        
            _loadCancelButton =
                new Button {Content = "Cancel", Width = 100, Margin = new Thickness(10, 0, 0, 0)}.SetClick((o, args)
                    => _diagManager.CloseDialog());

            var grid = new Grid() {Height = 300}.CreateColumns("30*", "70*");
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto} );
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(desText.Height) });

            var labelConfigurations = new Label { Content = "Configurations" };
            labelConfigurations.SetValue(Grid.ColumnProperty, 0);
            labelConfigurations.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(labelConfigurations);

            list.SetValue(Grid.ColumnProperty, 0);
            list.SetValue(Grid.RowProperty, 1);
            list.MouseDoubleClick += async (o, args) => await LoadAndCloseDialogAsync(hwDefs, list, diag);

            grid.Children.Add(list);

            var labelDescription = new Label { Content = "Description" };
            labelDescription.SetValue(Grid.ColumnProperty, 1);
            labelDescription.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(labelDescription);

            desText.SetValue(Grid.ColumnProperty, 1);
            desText.SetValue(Grid.RowProperty, 1);
            grid.Children.Add(desText);

            var stack =
                new StackPanel() { Margin = new Thickness(20, 20, 20, 20) }
                    .Add(new Label { Content = "Load Hardware", FontSize = 25, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20) })
                    .Add(grid)
                    .Add(
                        new StackPanel()
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(0, 20, 0, 0)
                        }.Add(_loadButton)
                         .Add(_loadCancelButton));

            diag.Content = stack;
            _menu.IsOpen = false;
            _diagManager.ShowDialog(diag);
        }

        private async Task LoadAndCloseDialogAsync(List<HardwareDefinitionDto> hwDefs, ListBox list, SimpleDialog diag)
        {
            if (_loadButton.IsEnabled == false)
                return;

            _loadButton.IsEnabled = false;
            _loadButton.Content = "Loading";  //TODO ALRN - find a better user feedback 
            _loadCancelButton.IsEnabled = false;

            diag.CloseDialog();

            await LoadHardwareDefAsync(hwDefs.First(h => h.Name.Equals(list.SelectedItem)));        
            
            _loadCancelButton.IsEnabled = true;
            _loadButton.Content = "Load";
            _loadButton.IsEnabled = true;

        }

        private async Task LoadHardwareDefAsync(HardwareDefinitionDto hw)
        {
            var rc = await _diagManager.Navigator.NavigateTo(typeof(HardwareOverviewViewModel), hw);

            if (rc)
            {
                UnsavedChanges = false; // we loose any unsaved changes
            }

            CommandManager.InvalidateRequerySuggested();
        }
  

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveConfigAsync(_current.Name, _current.Description);
        }

        private async void SaveConfigAsync(string name, string des)
        {
            _current.Name = name;
            _current.Description = des;
            await _client.SaveCurrentHardwareDef(_current);

            UnsavedChanges = false;
            
            NameTextBlock.Text = GetDisplayFileName();

            CommandManager.InvalidateRequerySuggested();
        }


        public void DocsisVersionChanged()
        {
            GetCurrentHardwareDefAsync().Forget();

            UnsavedChanges = false;
        }

        private void ToolbarButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _menu.IsOpen = !_menu.IsOpen;
        }
    }
}