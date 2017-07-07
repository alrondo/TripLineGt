using System.Threading.Tasks;

namespace TripLine.WPF.MVVM
{
    public interface IView
    {
        INavigator Nav { get; set; }
        void ViewLoaded();

        void ViewUnloaded();
    }
}