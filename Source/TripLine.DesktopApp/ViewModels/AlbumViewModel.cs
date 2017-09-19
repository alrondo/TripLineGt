﻿using log4net;
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
        private readonly HighliteService    _highliteService;
        private readonly LocationService    _locationService;
        private readonly MainViewModel      _mainViewModel;
        private readonly DebugInfoViewModel _debugInfoVModel;

        public string DisplayName { get; set; } = "Your selected album";

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public HighliteTopicViewModel NoSelection
        {
            get { return null; }   // no selection
            set { OnPropertyChanged(); }
        }


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
            _sections = LoadFromHighliteTarget(hitemVM.Target, hitemVM.TargetId);

            SelectedSection = Sections.First();

            var photo = _photoStore.GetPhoto(SelectedSection.Items.First().PhotoId);
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
            try
            {
                var trip = _tripStore.GetTrip(id);

                _tripStore.DumpTrip(id, "LoadFromTrip");

                return new ObservableCollection<AlbumSectionViewModel>(CreateSections(trip));

            }
            catch
            {
                MessageBox.Show("Oups! My mistake!", $"Not able fo find a trip with id {id}", MessageBoxButton.OK);
                return new ObservableCollection<AlbumSectionViewModel>();
            }
        }

        public ObservableCollection<AlbumSectionViewModel> LoadFromLocation(int id)
        {
            var location = _locationService.GetLocation(id);
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

            vmodel.PhotoId = photo.Id;

            SetupCommands(vmodel);
            return vmodel;
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

    }
}
