namespace TripLine.WPF.MVVM
{
    public interface IViewFor<T> : IView
    {
        T ModelView { get; }
    }
}