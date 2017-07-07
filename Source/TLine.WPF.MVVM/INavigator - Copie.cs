using System.Threading.Tasks;

namespace TripLine.WPF.MVVM
{
    using System;

    public interface INavigator
    {
        Task<bool> NavigateTo(Type type, object param = null);

        bool HasBreadCrumb();

        Task GoBack();

        IView CurrentView { get; }
    }
}