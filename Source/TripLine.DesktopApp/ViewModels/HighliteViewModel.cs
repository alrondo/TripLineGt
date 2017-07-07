using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


using TripLine.Toolbox.Extensions;


using TripLine.WPF.MVVM;
using System.Collections.ObjectModel;
using TripLine.Service;
using TripLine.Dtos;
using TripLine.DesktopApp.View;
using System.Windows.Input;
using System.Threading.Tasks;

namespace TripLine.DesktopApp.ViewModels
{
    
    public class HighliteItemViewModel : BaseViewModel, IHighliteItem
    {
        public string DefaultImg { get; set; } = "pack://application:,,,/Resources/hawai.jpg";

        public string Thumbnail { get; set; } 
        public string PhotoUrl { get; set; }

        public string DisplayName { get; set; } = "abc";
        public string Description { get; set; }

        public int    Id      { get; set; }

        public HighliteTarget Target { get; set; }

        public event Action<HighliteItemViewModel> OnOpen;

        public event Action<HighliteItemViewModel> OnRemoved;

    
        public ICommand OpenCommand
        {
            get
            {
                return new VMBladeCommand(async () => await ExecOpen(), () => true, ""); // CanExecuteOk(), "");
            }
        }

        public HighliteItemViewModel() : base("HighliteItem")
        { }


        private async Task ExecOpen()       
        {
            this?.OnOpen(this);
            
            OnPropertyChanged(nameof(DisplayName));
        }


    }


    public class HighliteTopicViewModel : BaseViewModel, IHighliteTopic
    {
        public string DisplayName { get; set; }

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public HighliteItemViewModel NoSelection
        {
            get { return null; }   // no selection
            set { OnPropertyChanged(); }
        }

        public List<HighliteItemViewModel> Items { get; set; }

        public HighliteTopicViewModel() : base("HighliteTopic")
        { }


    }



    //[NotifyProperty(ApplyToStateMachine = false)]
    public class HighliteViewModel : BaseViewModel , IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly HighliteService _highliteService;

        public string DisplayName { get; set; } = "A little overview";

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public HighliteTopicViewModel NoSelection
        {
            get { return null; }   // no selection
            set { OnPropertyChanged(); }
        }




        private readonly MainViewModel _mainViewModel;

        public HighliteViewModel (PhotoStore photoStore, HighliteService highliteService) : base("Highlite")
        {
            _photoStore = photoStore;
            _highliteService = highliteService;
            _mainViewModel = MainViewModel.Instance;
            Load();
        }

        public void Dispose()
        {
           
        }
        
        
        public ObservableCollection<HighliteTopicViewModel> _topics = new ObservableCollection<HighliteTopicViewModel>();

        public ObservableCollection<HighliteTopicViewModel> Topics
        {
            get
            {
                return _topics;
            }
            set
            {
                if (value == _topics)
                    return;

                _topics = value;
                OnPropertyChanged();
            }
        }

        private void Load ()
        {
            List<string> choseLocationNames = new List<string>();

            var highlites = _highliteService.GetHighlites();
            
            Topics = new ObservableCollection<HighliteTopicViewModel>(CreateHighlites(highlites));

            RollOffItemCommands();
        }


        private void RollOffItemCommands()
        {
            foreach (var topic in Topics)
            { 
                topic.Items.ForEach(c => c.OnOpen += OnItemOpen);
                topic.Items.ForEach(c => c.OnRemoved += OnItemRemoved);
            }
           
        }

        private async void OnItemOpen(HighliteItemViewModel obj)
        {
            _mainViewModel.CurrentHighliteItemViewModel = obj;

            await _mainViewModel.Navigator.NavigateTo(typeof(AlbumViewModel));

        }

        private async void OnItemRemoved(HighliteItemViewModel obj)
        {
            throw new NotImplementedException();
        }

        private List<HighliteTopicViewModel> CreateHighlites(List<HighliteTopic> highlites)
        {
            List<HighliteTopicViewModel> vmodels = highlites.Select(h
              => CreateHighliteTopicViewModel(h)).ToList();
                
            return vmodels;
        }


        private HighliteTopicViewModel CreateHighliteTopicViewModel( HighliteTopic topic)
        {
            HighliteTopicViewModel vmodel = AutoMapper.Mapper.Map<HighliteTopicViewModel>(topic);
            vmodel.Items = topic.Items.Take(5).Select(i => CreateHighliteItemViewModel(i)).ToList();


            return vmodel;
         }

        private HighliteItemViewModel CreateHighliteItemViewModel(IHighliteItem item)
        {
            HighliteItemViewModel vmodel = AutoMapper.Mapper.Map<HighliteItemViewModel>(item);

            return vmodel;
        }

        //private async void OnSelected(HighliteItem hitem)
        //{
        //}


        //private void OnRemoved(HighliteItem hitem)
        //{
        //}





    }
}
