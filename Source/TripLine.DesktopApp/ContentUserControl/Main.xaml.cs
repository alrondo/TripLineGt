using System;

using System.Windows;
using System.Windows.Controls;
using TripLine.WPF.MVVM;
using log4net;
using Tripline.DesktopApp.ContentUserControl;
using TripLine.DesktopApp.View;
using TripLine.DesktopApp.ViewModels;
using TripLine.Dtos;
using AutoMapper;
using MahApps.Metro.Controls;

namespace TripLine.DesktopApp.ContentUserControl
{

    public partial class Main : BaseUserControl
    {
        private ILog _log = LogManager.GetLogger("Main");
        //private readonly IWindowCommands _windowCommands;

        public readonly MainViewModel _mainViewModel;
        
        public Flyout MainFlyout { get; private set; }


        public Main(IWindowCommands commands, Flyout flyout, ContentControl flyoutContenetControl)
        {
            InitializeComponent();

            _mainViewModel = MainViewModel.Instance;

            MainFlyout = flyout;
            
            Loaded += OnLoaded;
        }

        private bool _startAsFlyout = false;

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            try
            {
                if (_startAsFlyout)
                    // show startup flyout
                    ShowFlyout(typeof(TripWizardView));
                else
                    // change startup view
                    await _mainViewModel.GoStartupView();
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

       
        public void ShowFlyout(Type viewType)  
        {
            var grid = (MainFlyout.Content as Grid);

            ContentControl flyoutContentControl     = null;
            foreach (var control in grid.Children)
            {
                if (control is ContentControl)
                {
                    flyoutContentControl = (ContentControl) control;
                    break;
                }
            }

            if (flyoutContentControl == null)
                throw new InvalidOperationException();
            
            var view = _mainViewModel.Navigator.GetView(viewType);

            var viewmodel = _mainViewModel.Navigator.GetViewModel(viewType);

            flyoutContentControl.Content = view;
            flyoutContentControl.DataContext = viewmodel;
            
            var prop = viewType.GetProperty("ModelView");
            if (prop != null) prop.SetValue(view, viewmodel);

            MainFlyout.IsOpen = true;

            view.ViewLoaded();
        }

        public void HideFlyout(BaseViewModel viewModel)
        {
            MainFlyout.IsOpen = false;
        }

      
    }
}
