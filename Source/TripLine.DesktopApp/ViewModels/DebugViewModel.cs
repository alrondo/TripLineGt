using System;
using System.Reflection;
using log4net;
using TripLine.Service;

namespace TripLine.DesktopApp.ViewModels
{
    public class DebugViewModel : BaseViewModel, IDisposable
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PhotoStore _photoStore;
        private readonly HighliteService _highliteService;

        public string Title { get; set; } = "Debug Debug";

        // Little trick to disable listbox selection...   We bind Listbox selectitems to NoSelection.  
        // The real selections are handled by the checkboxes inside the list items.
        public HighliteTopicViewModel NoSelection
        {
            get { return null; } // no selection
            set { OnPropertyChanged(); }
        }




        private readonly MainViewModel _mainViewModel;

        public DebugViewModel(PhotoStore photoStore, HighliteService highliteService) : base("Highlite")
        {
            _photoStore = photoStore;
            _highliteService = highliteService;
            _mainViewModel = MainViewModel.Instance;
        }

        public void Dispose()
        {

        }
    }
}