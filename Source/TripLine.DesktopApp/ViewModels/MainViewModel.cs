using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TripLine.DesktopApp.View;
using TripLine.Dtos;
using TripLine.Service;
using TripLine.WPF.MVVM;

namespace TripLine.DesktopApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private static Navigator _navigator;

        static ContentControl _contentControl;

        public Navigator Navigator
        {
            get { return _navigator; }

            set { _navigator = value; }
        }
        

        public HighliteSelectOptions HighliteSelectOptions { get; set; }
        

        public HighliteItemViewModel CurrentHighliteItemViewModel { get; set; }

        public int SelectedPhoto { get; set; }

        public static MainViewModel Instance { get; set; }

        private readonly TripCreationService _tripCreationService;

        public MainViewModel(ContentControl contentControl) : base("Home")
        {
            Initialize();

            if (_contentControl == null)
                _contentControl = contentControl;

            Instance = this;

            _tripCreationService = Navigator.Configuration.IoC.Resolve(typeof(TripCreationService)) as TripCreationService;
        }


        public async Task GoStartupView()
        {
            if (_tripCreationService.PeakForNewTripPhotos())
                await GoWizard();
            else
            {
                HighliteSelectOptions = null;
                await GoHome();
            }
                
        }


        public async Task GoTripsView()
        {
            HighliteSelectOptions = new HighliteSelectOptions(HighliteTarget.Trip);
          

            await _navigator.NavigateTo(typeof(HighliteView));
        }


        public async Task GoHome()
        {
            await _navigator.NavigateTo(typeof(HighliteView));
        }

        public async Task GoWizard()
        {
            await _navigator.NavigateTo(typeof(TripWizardView));
        }

        public async Task GoBack()
        {
            await _navigator.GoBack();
        }

        private static void Initialize()
        {
            if (_navigator != null)
                return;

            _navigator = TripLine.WPF.MVVM.Navigator.Load();

            //_navigator.Configuration.IoC.Register(_windowCommands);
            _navigator.Configuration.OnNavigation += (view, model) => _contentControl.Content = view;

        }


    }
}
