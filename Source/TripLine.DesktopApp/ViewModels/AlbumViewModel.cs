using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


using TripLine.Toolbox.Extensions;


using TripLine.WPF.MVVM;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TripLine.Service;
using TripLine.Dtos;

namespace TripLine.DesktopApp.ViewModels
{
    public class AlbumItemViewModel :  BaseViewModel
    {
        public string DefaultImg { get; set; } = "pack://application:,,,/Resources/hawai.jpg";

        public string Thumbnail => PhotoUrl;
        public string PhotoUrl { get; set; }

        public string DisplayName { get; set; } = "abc";
        public string Description { get; set; }

        public int    Id      { get; set; }

        public HighliteTarget Target { get; set; }

        public event Action<AlbumItemViewModel> OnOpen;

        public event Action<AlbumItemViewModel> OnRemoved;


        public AlbumItemViewModel() : base("AlbumItem")
        { }

        public ICommand OpenCommand
        {
            get
            {
                return new VMBladeCommand(async () => await ExecOpen(), () => true, ""); // CanExecuteOk(), "");
            }
        }


        private async Task ExecOpen()
        {
            this?.OnOpen(this);

            OnPropertyChanged(nameof(DisplayName));
        }


    }


    public class AlbumSectionViewModel : BaseViewModel
    {
        public string DisplayName { get; set; }

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public AlbumItemViewModel NoSelection
        {
            get { return null; }   // no selection
            set
            {
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedItem));
            }
        }


        private List<AlbumItemViewModel> _items;
        public List<AlbumItemViewModel> Items
        {
            get { return _items; }
            set
            {
                if (value == _items)
                    return;

                _items = value;
                OnPropertyChanged();

                SelectedItem = _items?.First();
            }
        }
        private AlbumItemViewModel _selectedItem;
        public AlbumItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        public AlbumSectionViewModel( ) : base("AlbumSection")
        {
            Items = Items;
        }
    }


    public class AlbumViewModel : BaseViewModel , IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly TripStore _tripStore;
        private readonly PhotoStore _photoStore;
        private readonly HighliteService _highliteService;
        private readonly LocationService _locationService;
        private readonly MainViewModel _mainViewModel;


        public string DisplayName { get; set; } = "Your selected album";

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public HighliteTopicViewModel NoSelection
        {
            get { return null; }   // no selection
            set { OnPropertyChanged(); }
        }


        DebugInfoViewModel _debugInfoVModel;
        public string Title => "Photo";
        public AlbumViewModel (TripStore tripStore, PhotoStore photoStore, HighliteService highliteService,
    							LocationService locationService, DebugInfoViewModel debugInfo) : base("Highlite")
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
            _highliteService = highliteService;
            _locationService = locationService;
            _debugInfoVModel = debugInfo;
            _mainViewModel = MainViewModel.Instance;
            Load();
        }

        public void Dispose()
        {
           
        }


        public event Action<AlbumViewModel> OnInfo;


        public ICommand InfoCommand
        {
            get
            {
                return new VMBladeCommand( () => ExecInfo(), () => true, ""); // CanExecuteOk(), "");
            }
        }

        private void ExecInfo()
        {
            ShowInfo = ! ShowInfo;

            this.OnInfo?.Invoke(this);

            //OnPropertyChanged(nameof(DisplayName));
        }

        public ObservableCollection<AlbumSectionViewModel> _sections = new ObservableCollection<AlbumSectionViewModel>();

        public ObservableCollection<AlbumSectionViewModel> Sections
        {
            get
            {
                return _sections;
            }
            set
            {
                if (value == _sections)
                    return;

                _sections = value;
                OnPropertyChanged();
            }
        }

        public AlbumSectionViewModel SelectedSection { get; set; }


        public DebugInfoViewModel DebugInfo  => _debugInfoVModel;


        private bool _showInfo = false;
        public bool ShowInfo
        {
            get { return _showInfo; }
            set
            {
                if (value == _showInfo)
                    return;

                _showInfo = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowPhoto));
            }         
        }

        public bool ShowPhoto => !ShowInfo;

        private void Load ()
        {
            List<string> choseLocationNames = new List<string>();

            var hitemVM =_mainViewModel.CurrentHighliteItemViewModel;
            _sections = LoadFromHighliteTarget(hitemVM.Target, hitemVM.Id);
         
            SelectedSection = Sections.First();

            var photo = _photoStore.GetPhoto(SelectedSection.Items.First().Id);
            DebugInfo.Load(photo);
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(SelectedSection));
            OnPropertyChanged(nameof(DebugInfo));

            ShowInfo = false;
        }

        public ObservableCollection<AlbumSectionViewModel> LoadFromHighliteTarget(HighliteTarget target, int id)
        {
            switch (target)
            {
                case HighliteTarget.Trip:
                    return LoadFromTrip(id);

                case HighliteTarget.Location:
                    return LoadFromLocation(id);
                default:
                    throw new NotImplementedException();
            }
        }


        public ObservableCollection<AlbumSectionViewModel> LoadFromTrip(int id)
        {
            var trip = _tripStore.GetTrip(id);

            return  new ObservableCollection<AlbumSectionViewModel>(CreateSections(trip));
        }

        public ObservableCollection<AlbumSectionViewModel> LoadFromLocation(int id)
        {
            var location = _locationService.GetLocation(id);
            var photos = _photoStore.GetPhotosAtLocation(id);
            return new ObservableCollection<AlbumSectionViewModel>(CreateSections(location));
        }

        private List<AlbumSectionViewModel> CreateSections(Trip trip)
        {
            var list = new List<AlbumSectionViewModel>();

            foreach (var dest in trip.Destinations)
                list.Add(CreateSection(dest));


            return list;
        }

        private List<AlbumSectionViewModel> CreateSections(Location location)
        {
            var list = new List<AlbumSectionViewModel>();
            list.Add(CreateSection(location));
            return list;
        }
        private AlbumSectionViewModel CreateSection(Destination destination)
        {
            AlbumSectionViewModel vmodel = AutoMapper.Mapper.Map<AlbumSectionViewModel>(destination);

            var photos = _photoStore.GetPhotos().Where(p => p.DestId == destination.Id).ToList();

            vmodel.Items = photos.Select(p => CreateAlbumItemViewModel(p)).ToList();

            return vmodel;
        }

        private AlbumSectionViewModel CreateSection(Location location)
        {
            AlbumSectionViewModel vmodel = AutoMapper.Mapper.Map<AlbumSectionViewModel>(location);

            var photos = _photoStore.GetPhotosAtLocation(location.Id).ToList();
            vmodel.Items = photos.Select(p => CreateAlbumItemViewModel(p)).ToList();
            return vmodel;
        }
        private AlbumItemViewModel CreateAlbumItemViewModel(Photo photo)
        {
            AlbumItemViewModel vmodel = AutoMapper.Mapper.Map<AlbumItemViewModel>(photo);

            SetupCommands(vmodel);
            return vmodel;
        }



        private void  SetupCommands(AlbumItemViewModel vmodel)
        {
            vmodel.OnOpen += OnItemOpen;
            vmodel.OnRemoved += OnItemRemoved;
        }
        
        private async void OnItemOpen(AlbumItemViewModel obj)
        {
            SelectedSection.SelectedItem = obj;

        }

        private async void OnItemRemoved(AlbumItemViewModel obj)
        {
            throw new NotImplementedException();
        }

    }
}
