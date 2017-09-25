using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;


using TripLine.Toolbox.Extensions;


using TripLine.WPF.MVVM;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using TripLine.Service;
using TripLine.Dtos;
using System.Windows;

namespace TripLine.DesktopApp.ViewModels
{
    public class AlbumViewModel : BaseViewModel , IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TripStore _tripStore;
        private readonly PhotoStore _photoStore;
        private readonly LocationService    _locationService;
        private readonly MainViewModel      _mainViewModel;
        private readonly DebugInfoViewModel _debugInfoVModel;
        private readonly HighliteService _highliteService;
        private readonly AlbumService _albumService;


        public string DisplayName { get; set; } = "Your selected album";

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public HighliteTopicViewModel NoSelection
        {
            get { return null; }   // no selection
            set { OnPropertyChanged(); }
        }


        private Album _album;

        public Album Album
        {
            get { return _album; }   // no selection
            set
            {
                if (value == _album)
                    return;

                OnPropertyChanged(); 
                
            }
        }


        public string Title => "Photo";
        public AlbumViewModel (TripStore tripStore, PhotoStore photoStore, HighliteService highliteService,  AlbumService albumService,
    							LocationService locationService, DebugInfoViewModel debugInfo) : base("Highlite")
        {
            _tripStore = tripStore;
            _photoStore = photoStore;
            _albumService = albumService;
            _highliteService = highliteService;
            _locationService = locationService;
            _debugInfoVModel = debugInfo;
            _mainViewModel = MainViewModel.Instance;
            _mainViewModel.OnRemove += _mainViewModel_OnRemove;
            Load();
        }

        private async void _mainViewModel_OnRemove()
        {
            var hitemVM = _mainViewModel.CurrentHighliteItemViewModel;

            if (hitemVM != null && hitemVM.Target == HighliteTarget.Trip)
            {
                _tripStore.Remove(hitemVM.TargetId);

                await _mainViewModel.GoBack();
            }

        }

        public void Dispose()
        {
            _mainViewModel.OnRemove -= _mainViewModel_OnRemove;
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

            LoadFromHighliteTarget(hitemVM.Target, hitemVM.TargetId);

            SelectedSection = Sections.First();

            var photo = _photoStore.GetPhoto(SelectedSection.Items.First().PhotoId);
            DebugInfo.Load(photo);

            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(SelectedSection));
            OnPropertyChanged(nameof(DebugInfo));

            ShowInfo = false;
        }

        public void LoadFromHighliteTarget(HighliteTarget target, int id)
        {

            switch (target)
            {
                case HighliteTarget.Trip:
                    _album = _albumService.GetTripAlbum(id);
                    break;

                case HighliteTarget.Location:
                    _album = _albumService.GetLocationAlbum(id);

                    break;

                default:
                    throw new NotImplementedException();
            }


            Sections = new ObservableCollection<AlbumSectionViewModel>();
            _album.Sections.ForEach(s => Sections.Add(CreateAlbumSections(s)));
        }

        
        private void  SetupCommands(AlbumItemViewModel vmodel)
        {
            vmodel.OnOpen += OnItemOpen;
            vmodel.OnRemove += OnItemRemoved;
        }
        
        private async void OnItemOpen(AlbumItemViewModel albumItem)
        {
            SelectedSection.SelectedItem = albumItem;

            var photo = _photoStore.GetPhoto(albumItem.PhotoId);

            Debug.WriteLine($"> Photo {albumItem.DisplayName}");
            Debug.WriteLine($">       {albumItem.PhotoUrl}");

            photo.Dump("Open item ");

        }

       

        private async void OnItemRemoved(AlbumItemViewModel obj)
        {
            throw new NotImplementedException();
        }
        

        private AlbumSectionViewModel CreateAlbumSections(AlbumSection section)
        {
            AlbumSectionViewModel vmodel = AutoMapper.Mapper.Map<AlbumSectionViewModel>(section);

            foreach (var item in vmodel.Items)
            {
                SetupCommands(item);
            }
            return vmodel;
        }

    }
}
