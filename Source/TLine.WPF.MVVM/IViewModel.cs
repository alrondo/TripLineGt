using System.Threading.Tasks;

namespace TripLine.WPF.MVVM
{
    public interface IViewModel
    {
        INavigator Navigator { get; set; }

        void BeforeNavigationTo();

        Task<bool> NavigatedTo(object param);

        bool BeforeNavigationAway();

        void NavigatedAway();
 
    }
}