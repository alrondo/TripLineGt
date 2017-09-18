using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TripLine.Dtos;

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
                return new VMBladeCommand(async () => await ExecOpen(), () => true, "");  
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
}