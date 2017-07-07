using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

using TripLine.Toolbox.Extensions;

using TripLine.WPF.MVVM;

using TripLine.Dtos;
using TripLine.Service;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Averna.WPF.BladeUi;

namespace TripLine.DesktopApp.ViewModels
{
    public class TripWizardViewModel : BaseViewModel, IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly PhotoStore _photoStore;
        //private readonly LocationService _locationService;

        private readonly TripCreationService _tripCreationService;

        private readonly MainViewModel _mainViewModel;


        private System.Timers.Timer _refreshTimer = new System.Timers.Timer();


        private System.Threading.Thread _backgroundThread;

        // , PhotoStore photoStore, LocationService locationService

        public TripWizardViewModel(TripCreationService tripCreationService) : base("TripWizard")
        {
            _tripCreationService = tripCreationService;
            //_photoStore = photoStore;
            //_locationService = locationService;
            _mainViewModel = MainViewModel.Instance;

            DetectingNewTripComponents = false;

            Status = "Detecting new photos";
            InitialDetection = true;

            _refreshTimer.Interval = 400.0;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += _refreshTimer_Elapsed;
            _refreshTimer.Enabled = true;

            _backgroundThread = new System.Threading.Thread(DetectTripsTask);
            _backgroundThread.Start();

        }

        private async Task ExecOk()
        {
            //_tripCreationService.AddAll();

            _tripCreationService.AddAll();
            await Task.Delay(10);

            await _mainViewModel.GoHome();
        }

        private async Task ExecCancel()
        {
            _tripCreationService.Stop();

            _tripCreationService.RejectAll();

            await Task.Delay(10);

            await _mainViewModel.GoHome();
        }
        

        private void DetectTripsTask()
        {
            SetStatus($"check your folders for new photos.");

            int numPhotoFiles = _tripCreationService.DetectNewFiles().NumNewPhotos;

            if (numPhotoFiles == 0)
            {
                SetStatus("No new photos available.");
                Stopped();
            }
   

            System.Threading.Thread.Sleep(4000);

            SetStatus("detect your travel photos.");

            _tripCreationService.DetectTripsFromNewPhotos();
            
        }

        private void Stopped()
        {
            _refreshTimer.Enabled = false;
            _refreshTimer.Stop();

            SetValueFromDetectTripResult(_tripCreationService.TripCreationDetectResult);

            _mainViewModel.GoHome();

        }

        private void _refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var result = _tripCreationService.TripCreationDetectResult;

            SetValueFromDetectTripResult(result );
        }

        

        private void SetValueFromDetectTripResult(TripCreationDetectResult result)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() => UnsafeSetValueFromDetectTripResult(result));
                return;
            }

            UnsafeSetValueFromDetectTripResult(result);
        }

        private void UnsafeSetValueFromDetectTripResult(TripCreationDetectResult result, bool fromDispatcher = false)
        {
            NewPhotoCount = result.NumNewPhotos;
            NewTravelPhotosCount = result.NumNewTravelPhotos;

            NewPhotoSessionCount = result.NumNewSessions;
            NewDestinationCandidateCount = result.NumNewDestinations;
            NewTripCandidateCount = result.NumNewTrips;

            if (result.State.TaskState == BuildTaskState.Stopped)
            {
                InitialDetection = false;
                DetectingNewTripComponents = false;
                DetectionCompleted = true;
                Status = "";

                if (result.NumNewTravelPhotos <= 0)
                    _mainViewModel.GoHome().Forget();
            }
            else
            if (result.NumNewTravelPhotos > 0)
            {
                InitialDetection = false;
                DetectingNewTripComponents = true;

                Status = "detect trips & destinations.";
            }
        }
        
        private void SetStatus(string status, bool fromDispatcher=false)
        {
            if (!fromDispatcher && Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() => SetStatus(status, true));
                return;
            }

            Status = status;
        }
        

        public void Dispose() { }


        private bool _initialDetection;

        public bool InitialDetection
        {
            get
            {
                return _initialDetection;
            }
            set
            {
                if (value == _initialDetection)
                    return;

                _initialDetection = value;
                OnPropertyChanged();
            }
        }



        private bool _detectingNewTripComponents;

        public bool DetectingNewTripComponents
        {
            get
            {
                return _detectingNewTripComponents;
            }
            set
            {
                if (value == _detectingNewTripComponents)
                    return;

                _detectingNewTripComponents = value;
                OnPropertyChanged();
            }
        }


        private bool _detectionCompleted;

        public bool DetectionCompleted
        {
            get
            {
                return _detectionCompleted;
            }
            set
            {
                if (value == _detectionCompleted)
                    return;

                _detectionCompleted = value;
                OnPropertyChanged();
            }
        }


        private string _status = "Please wait";

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value == _status)
                    return;

                _status = value;
                OnPropertyChanged();
            }
        }

        private int _newPhotoCount = 0;

        public int NewPhotoCount
        {
            get
            {
                return _newPhotoCount;
            }
            set
            {
                if (value == _newPhotoCount)
                    return;

                _newPhotoCount = value;
                OnPropertyChanged();
            }
        }

        public bool NewTravelPhotoFound => NewTravelPhotosCount > 0;


        private bool CanExecuteOk()
        {
            return DetectionCompleted;
        }
        

        private int _newTripPhotoCount = 0;

        public int NewTravelPhotosCount
        {
            get
            {
                return _newTripPhotoCount;
            }

            set
            {
                if (value == _newTripPhotoCount)
                    return;

                _newTripPhotoCount = value;
                OnPropertyChanged();
                OnOtherPropertyChanged(nameof(NewTravelPhotoFound));
            }
        }

        private int _newPhotoSessionCount = 0;


        public int NewPhotoSessionCount
        {
            get
            {
                return _newPhotoSessionCount;
            }
            set
            {
                if (value == _newPhotoSessionCount)
                    return;

                _newPhotoSessionCount = value;
                OnPropertyChanged();
            }
        }

        private int _newTripCandidateCount = 0;


        public int NewTripCandidateCount
        {
            get
            {
                return _newTripCandidateCount;
            }
            set
            {
                if (value == _newTripCandidateCount)
                    return;

                _newTripCandidateCount = value;
                OnPropertyChanged();
            }
        }

        private int _newDestinationCandidateCandidateCount = 0;


        public int NewDestinationCandidateCount
        {
            get
            {
                return _newDestinationCandidateCandidateCount;
            }
            set
            {
                if (value == _newDestinationCandidateCandidateCount)
                    return;

                _newDestinationCandidateCandidateCount = value;
                OnPropertyChanged();
            }
        }


        public ICommand OkCommand
        {
            get
            {
                return new VMBladeCommand(async () => await ExecOk(), () => CanExecuteOk(), "");
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new VMBladeCommand(async () => await ExecCancel(), () => true, "");
            }
        }

    }


}
