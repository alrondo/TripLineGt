using System;
using System.Threading.Tasks;
using System.Windows.Input;
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


        public int PhotoId { get; set; }

        public int    TargetId      { get; set; }

        public HighliteTarget Target { get; set; }

        public event Action<AlbumItemViewModel> OnOpen;
        public event Action<AlbumItemViewModel> OnRemove;

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
}