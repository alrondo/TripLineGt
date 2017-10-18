using System.Collections.Generic;
using System.Linq;

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

                if (value == null)
                    value = _items?.First();

                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public void DeleteSelectedItem()
        {
            if (SelectedItem == null)
                return;

            _items.Remove(SelectedItem);
            SelectedItem = _items?.First();
            OnPropertyChanged(nameof(Items));
        }


        public AlbumSectionViewModel( ) : base("AlbumSection")
        {
            Items = Items;
        }
    }
}