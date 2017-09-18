using System.Collections.Generic;
using TripLine.Dtos;

namespace TripLine.DesktopApp.ViewModels
{
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
}