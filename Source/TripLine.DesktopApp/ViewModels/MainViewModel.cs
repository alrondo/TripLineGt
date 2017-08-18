using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly TripStore _tripStore;
        private readonly LocationService _locationService;

        private int _numTrips = 0;

        public int NumTrips
        {
            get
            {
                return _numTrips;
            }
            set
            {
                if (value == _numTrips)
                    return;

                _numTrips = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NumTripsString));
            }
        }


        public string NumTripsString => $"{NumTrips} trips";

        
        private int _numLocations = 0;

        public int NumLocations
        {
            get
            {
                return _numLocations;
            }
            set
            {
                if (value == _numLocations)
                    return;

                _numLocations = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NumLocationsString));

            }
        }

        public string NumLocationsString => $"{NumLocations} locations";



        public string NuPlacesString => $"99 places";

        public ICommand ShowOverviewCommand
        {
            get
            {
                return new VMBladeCommand(() => GoHome(), () => true, "");
            }
        }



        public ICommand ShowTripsCommand
        {
            get
            {
                return new VMBladeCommand(() => GoTripsView(), () => true, "");  
            }
        }

        public ICommand ShowLocationsCommand
        {
            get
            {
                return new VMBladeCommand(() => GoLocationsView(), () => true, "");
            }
        }

        

       

        public MainViewModel(ContentControl contentControl) : base("Home")
        {
            Initialize();

            if (_contentControl == null)
                _contentControl = contentControl;

            Instance = this;

            _tripCreationService = Navigator.Configuration.IoC.Resolve(typeof(TripCreationService)) as TripCreationService;
            
            _tripStore =  Navigator.Configuration.IoC.Resolve(typeof(TripStore)) as TripStore;
            _locationService = Navigator.Configuration.IoC.Resolve(typeof(LocationService)) as LocationService;

            NumTrips = _tripStore.GetTrips().Count;

            NumLocations = _locationService.GetLocations().Count();

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
            ViewTitle = "Your Trips";

            HighliteSelectOptions = new HighliteSelectOptions(HighliteTarget.Trip);
            await _navigator.NavigateTo(typeof(HighliteView));
        }


        public async Task GoLocationsView()
        {
            ViewTitle = "Your Trips";

            HighliteSelectOptions = new HighliteSelectOptions(HighliteTarget.Location);
            await _navigator.NavigateTo(typeof(HighliteView));
        }

        public string ViewTitle = "";

        public async Task GoHome()
        {
            ViewTitle = "A Little Overview";

            HighliteSelectOptions = null;
            await _navigator.NavigateTo(typeof(HighliteView));
        }

        public async Task GoWizard()
        {
            ViewTitle = "Trip Creation Wizard";

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
